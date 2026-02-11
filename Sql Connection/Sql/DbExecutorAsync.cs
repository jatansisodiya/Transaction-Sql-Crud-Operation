using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using Transaction.SQLConnection.Exceptions;
using Transaction.SQLConnection.Interfaces;
using Transaction.SQLConnection.Mapping;

namespace Transaction.SQLConnection.Sql;

/// <summary>
/// Executes database commands without transaction management.
/// Use for simple read operations that don't require transactions.
/// </summary>
public sealed class DbExecutorAsync : IDbExecutorAsync
{
    private readonly IConnectionFactoryAsync _connectionFactory;
    private readonly ILogger<DbExecutorAsync> _logger;
    private const int DefaultCommandTimeout = 30;

    public DbExecutorAsync(IConnectionFactoryAsync connectionFactory, ILogger<DbExecutorAsync> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(
        string storedProcedure,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default) where T : new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedure);

        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            await using var command = CreateCommand(connection, storedProcedure, parameters, commandTimeout);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            return await EntityMapper.ReadListAsync<T>(reader, cancellationToken);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            _logger.LogError(ex, "Error executing query: {StoredProcedure}", storedProcedure);
            throw new DatabaseException($"Failed to execute query: {storedProcedure}", storedProcedure, ex);
        }
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(
        string storedProcedure,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default) where T : new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedure);

        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            await using var command = CreateCommand(connection, storedProcedure, parameters, commandTimeout);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            return await EntityMapper.ReadSingleAsync<T>(reader, cancellationToken);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            _logger.LogError(ex, "Error executing QuerySingleOrDefault: {StoredProcedure}", storedProcedure);
            throw new DatabaseException($"Failed to execute query: {storedProcedure}", storedProcedure, ex);
        }
    }

    public async Task<int> ExecuteAsync(
        string storedProcedure,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedure);

        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            await using var command = CreateCommand(connection, storedProcedure, parameters, commandTimeout);

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            _logger.LogError(ex, "Error executing stored procedure: {StoredProcedure}", storedProcedure);
            throw new DatabaseException($"Failed to execute stored procedure: {storedProcedure}", storedProcedure, ex);
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(
        string storedProcedure,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedure);

        try
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            await using var command = CreateCommand(connection, storedProcedure, parameters, commandTimeout);

            var result = await command.ExecuteScalarAsync(cancellationToken);

            if (result is null || result == DBNull.Value)
            {
                return default;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }
        catch (Exception ex) when (ex is not DatabaseException)
        {
            _logger.LogError(ex, "Error executing scalar: {StoredProcedure}", storedProcedure);
            throw new DatabaseException($"Failed to execute scalar: {storedProcedure}", storedProcedure, ex);
        }
    }

    private static SqlCommand CreateCommand(
        SqlConnection connection,
        string storedProcedure,
        SqlParameter[]? parameters,
        int? commandTimeout)
    {
        var command = new SqlCommand(storedProcedure, connection)
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
}