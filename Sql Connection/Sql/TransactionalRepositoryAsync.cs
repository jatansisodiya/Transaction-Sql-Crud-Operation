using CommonLogger;
using Microsoft.Data.SqlClient;
using System.Data;
using Transaction.SQLConnection.Exceptions;
using Transaction.SQLConnection.Interfaces;
using Transaction.SQLConnection.Mapping;

namespace Transaction.SQLConnection.Sql;

/// <summary>
/// Base repository with transactional support for stored procedure execution.
/// Implements Unit of Work pattern with support for multiple result sets.
/// </summary>
public class TransactionalRepositoryAsync(IConnectionFactoryAsync connectionFactory, IAILogger logger) : ITransactionalRepositoryAsync
{
    private readonly IConnectionFactoryAsync _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    private readonly IAILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const int DefaultCommandTimeout = 30;

    private SqlConnection? _connection;
    private SqlTransaction? _transaction;
    private bool _disposed;
    private bool _isExternalTransaction;

    /// <summary>
    /// Gets the current SQL connection (available after BeginTransactionAsync).
    /// </summary>
    protected SqlConnection? Connection => _connection;

    /// <summary>
    /// Gets the current SQL transaction (available after BeginTransactionAsync).
    /// </summary>
    protected SqlTransaction? Transaction => _transaction;

    /// <inheritdoc />
    public bool HasActiveTransaction => _transaction is not null;

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_transaction is not null)
        {
            _isExternalTransaction = true;
            _logger.LogInformation("Transaction already active, reusing existing transaction.");
            return;
        }

        _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        _transaction = (SqlTransaction)await _connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        _isExternalTransaction = false;

        _logger.LogInformation("Transaction started successfully.");
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_transaction is null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        if (_isExternalTransaction)
        {
            _logger.LogInformation("Skipping commit for externally managed transaction.");
            return;
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Transaction committed successfully.");
        }
        finally
        {
            await CleanupTransactionAsync();
        }
    }

    /// <inheritdoc />
    public async Task<T> ExecuteInTransactionAsync<T>(
        string storedProcedureName,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        int resultSetIndex = 0,
        bool isRead = false,
        CancellationToken cancellationToken = default)
    {
        ValidateExecutionState(storedProcedureName, resultSetIndex);

        bool shouldManageTransaction = !isRead && !HasActiveTransaction;

        try
        {
            var result = await ExecuteStoredProcedureAsync<T>(
                storedProcedureName,
                parameters,
                commandTimeout,
                resultSetIndex,
                isRead,
                cancellationToken);

            if (shouldManageTransaction)
            {
                await CommitAsync(cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error executing stored procedure: {storedProcedureName} at result set {resultSetIndex} (isRead: {isRead})");

            if (shouldManageTransaction)
            {
                await RollbackAsync();
            }

            throw new DatabaseException(
                $"Failed to execute stored procedure: {storedProcedureName}",
                storedProcedureName,
                resultSetIndex,
                ex);
        }
        finally
        {
            if (isRead && !HasActiveTransaction && _connection is not null)
            {
                await CleanupTransactionAsync();
            }
        }
    }

    /// <inheritdoc />
    public async Task<(T1 Result1, T2 Result2)> ExecuteMultipleResultSetsAsync<T1, T2>(
        string storedProcedureName,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        bool isRead = false,
        CancellationToken cancellationToken = default)
    {
        ValidateExecutionState(storedProcedureName);

        bool shouldManageTransaction = !isRead && !HasActiveTransaction;
        T1 result1;
        T2 result2;

        try
        {
            await using var command = await CreateCommandAsync(storedProcedureName, parameters, commandTimeout, isRead, cancellationToken);
            
            // Use explicit scope for reader to ensure it's closed before commit
            await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                // Read first result set
                result1 = await EntityMapper.ReadResultSetAsync<T1>(reader, cancellationToken);

                // Move to second result set
                if (!await reader.NextResultAsync(cancellationToken))
                {
                    throw new InvalidOperationException("Expected second result set but none found.");
                }

                result2 = await EntityMapper.ReadResultSetAsync<T2>(reader, cancellationToken);
                
                // Explicitly close the reader before commit
                await reader.CloseAsync();
            }

            if (shouldManageTransaction)
            {
                await CommitAsync(cancellationToken);
            }

            return (result1, result2);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            _logger.LogError(ex, "Error executing multiple result sets: {StoredProcedure} (isRead: {IsRead})", new { storedProcedureName, isRead });

            if (shouldManageTransaction)
            {
                await RollbackAsync();
            }

            throw new DatabaseException($"Failed to execute stored procedure with multiple result sets: {storedProcedureName}", storedProcedureName, ex);
        }
        finally
        {
            if (isRead && !HasActiveTransaction && _connection is not null)
            {
                await CleanupTransactionAsync();
            }
        }
    }

    /// <inheritdoc />
    public async Task<(T1 Result1, T2 Result2, T3 Result3)> ExecuteMultipleResultSetsAsync<T1, T2, T3>(
        string storedProcedureName,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        bool isRead = false,
        CancellationToken cancellationToken = default)
    {
        ValidateExecutionState(storedProcedureName);

        bool shouldManageTransaction = !isRead && !HasActiveTransaction;
        T1 result1;
        T2 result2;
        T3 result3;

        try
        {
            await using var command = await CreateCommandAsync(storedProcedureName, parameters, commandTimeout, isRead, cancellationToken);
            
            // Use explicit scope for reader to ensure it's closed before commit
            await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
            {
                result1 = await EntityMapper.ReadResultSetAsync<T1>(reader, cancellationToken);

                if (!await reader.NextResultAsync(cancellationToken))
                {
                    throw new InvalidOperationException("Expected second result set but none found.");
                }
                result2 = await EntityMapper.ReadResultSetAsync<T2>(reader, cancellationToken);

                if (!await reader.NextResultAsync(cancellationToken))
                {
                    throw new InvalidOperationException("Expected third result set but none found.");
                }
                result3 = await EntityMapper.ReadResultSetAsync<T3>(reader, cancellationToken);
                
                // Explicitly close the reader before commit
                await reader.CloseAsync();
            }

            if (shouldManageTransaction)
            {
                await CommitAsync(cancellationToken);
            }

            return (result1, result2, result3);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            _logger.LogError(ex, "Error executing multiple result sets: {StoredProcedure} (isRead: {IsRead})", new { storedProcedureName, isRead });

            if (shouldManageTransaction)
            {
                await RollbackAsync();
            }

            throw new DatabaseException($"Failed to execute stored procedure with multiple result sets: {storedProcedureName}", storedProcedureName, ex);
        }
        finally
        {
            if (isRead && !HasActiveTransaction && _connection is not null)
            {
                await CleanupTransactionAsync();
            }
        }
    }

    private async Task<T> ExecuteStoredProcedureAsync<T>(
        string storedProcedureName,
        SqlParameter[]? parameters,
        int? commandTimeout,
        int resultSetIndex,
        bool isRead,
        CancellationToken cancellationToken)
    {
        await using var command = await CreateCommandAsync(storedProcedureName, parameters, commandTimeout, isRead, cancellationToken);

        var targetType = typeof(T);

        // Case 1: Return int (scalar value) for first result set
        if (targetType == typeof(int) && resultSetIndex == 0)
        {
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is not null && result != DBNull.Value
                ? (T)(object)Convert.ToInt32(result)
                : (T)(object)0;
        }

        // Cases 2, 3, 4: Return List<T>, single T, or specific result set
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        // Navigate to the requested result set
        for (int i = 0; i < resultSetIndex; i++)
        {
            if (!await reader.NextResultAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Result set at index {resultSetIndex} not found. Stored procedure returned only {i + 1} result set(s).");
            }
        }

        return await EntityMapper.ReadResultSetAsync<T>(reader, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RollbackAsync()
    {
        if (_transaction is null)
        {
            _logger.LogWarning("Rollback called but no active transaction exists.");
            return;
        }

        if (_isExternalTransaction)
        {
            _logger.LogInformation("Rollback requested for externally managed transaction - will be handled by caller.");
            return;
        }

        try
        {
            await _transaction.RollbackAsync();
            _logger.LogInformation("Transaction rolled back successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during transaction rollback.");
        }
        finally
        {
            await CleanupTransactionAsync();
        }
    }

    private void ValidateExecutionState(string storedProcedureName, int resultSetIndex = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedureName);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (resultSetIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(resultSetIndex), "Result set index must be non-negative.");
        }
    }

    private async Task<SqlCommand> CreateCommandAsync(
        string storedProcedureName,
        SqlParameter[]? parameters,
        int? commandTimeout,
        bool isRead,
        CancellationToken cancellationToken)
    {
        // Centralized CQRS Connection & Transaction Management:
        if (!isRead && !HasActiveTransaction)
        {
            // Write/Command operation without active transaction -> Begin transaction
            await BeginTransactionAsync(cancellationToken);
        }
        else if (isRead && !HasActiveTransaction && _connection is null)
        {
            // Read/Query operation without active transaction -> Open standalone connection without transaction
            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        }

        // CQRS logic: If isRead = true, detach transaction so read query runs non-transactionally without locks
        SqlTransaction? activeTransaction = isRead ? null : _transaction;

        var command = new SqlCommand(storedProcedureName, _connection, activeTransaction)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = commandTimeout ?? DefaultCommandTimeout
        };

        if (parameters is { Length: > 0 })
        {
            command.Parameters.AddRange(parameters);
        }

        return command;
    }

    private async Task CleanupTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        _isExternalTransaction = false;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (_transaction is not null && !_isExternalTransaction)
        {
            _logger.LogWarning("Disposing repository with active transaction. Rolling back...");
            await RollbackAsync();
        }

        await CleanupTransactionAsync();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}