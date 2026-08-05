using CommonLogger;
using Dapper;
using System.Data;
using Transaction.SQLConnection.Exceptions;
using Transaction.SQLConnection.Interfaces;

namespace Transaction.SQLConnection.Sql;

/// <summary>
/// High-performance database executor using Dapper without transaction management.
/// Use for simple read operations that don't require transactions.
/// </summary>
public sealed class DbExecutorAsync(IConnectionFactoryAsync connectionFactory, IAILogger logger) : IDbExecutorAsync
{
    private readonly IConnectionFactoryAsync _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    private readonly IAILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const int DefaultCommandTimeout = 30;

    public async Task<IEnumerable<T>> QueryAsync<T>(
        string storedProcedure,
        object? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default) where T : new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedure);

        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

            return await connection.QueryAsync<T>(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: commandTimeout ?? DefaultCommandTimeout);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            _logger.LogError(ex, $"Error executing query: {storedProcedure}");
            throw new DatabaseException($"Failed to execute query: {storedProcedure}", storedProcedure, ex);
        }
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string storedProcedure,
        object? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default) where T : new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedure);

        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

            return await connection.QuerySingleOrDefaultAsync<T>(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: commandTimeout ?? DefaultCommandTimeout);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            _logger.LogError(ex, $"Error executing QuerySingleOrDefault: {storedProcedure}");
            throw new DatabaseException($"Failed to execute query: {storedProcedure}", storedProcedure, ex);
        }
    }

    public async Task<int> ExecuteAsync(
        string storedProcedure,
        object? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedure);

        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

            return await connection.ExecuteAsync(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: commandTimeout ?? DefaultCommandTimeout);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            _logger.LogError(ex, $"Error executing stored procedure: {storedProcedure}");
            throw new DatabaseException($"Failed to execute stored procedure: {storedProcedure}", storedProcedure, ex);
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(
        string storedProcedure,
        object? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedure);

        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

            return await connection.ExecuteScalarAsync<T>(
                storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: commandTimeout ?? DefaultCommandTimeout);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            _logger.LogError(ex, $"Error executing scalar: {storedProcedure}");
            throw new DatabaseException($"Failed to execute scalar: {storedProcedure}", storedProcedure, ex);
        }
    }
}