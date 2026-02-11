using Microsoft.Data.SqlClient;

namespace Transaction.SQLConnection.Interfaces;

/// <summary>
/// Low-level database executor for standalone queries without transaction management.
/// </summary>
public interface IDbExecutorAsync
{
    /// <summary>
    /// Executes a stored procedure and returns a collection of results.
    /// </summary>
    /// <typeparam name="T">Entity type to map results to.</typeparam>
    /// <param name="storedProcedure">Name of the stored procedure.</param>
    /// <param name="parameters">Optional SQL parameters.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of mapped entities.</returns>
    Task<IEnumerable<T>> QueryAsync<T>(
        string storedProcedure,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default) where T : new();

    /// <summary>
    /// Executes a stored procedure and returns a single result.
    /// </summary>
    /// <typeparam name="T">Entity type to map result to.</typeparam>
    /// <param name="storedProcedure">Name of the stored procedure.</param>
    /// <param name="parameters">Optional SQL parameters.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Single mapped entity or default.</returns>
    Task<T?> QuerySingleOrDefaultAsync<T>(
        string storedProcedure,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default) where T : new();

    /// <summary>
    /// Executes a non-query stored procedure and returns affected rows.
    /// </summary>
    /// <param name="storedProcedure">Name of the stored procedure.</param>
    /// <param name="parameters">Optional SQL parameters.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of rows affected.</returns>
    Task<int> ExecuteAsync(
        string storedProcedure,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a stored procedure and returns a scalar value.
    /// </summary>
    /// <typeparam name="T">Type of scalar value to return.</typeparam>
    /// <param name="storedProcedure">Name of the stored procedure.</param>
    /// <param name="parameters">Optional SQL parameters.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Scalar value or default.</returns>
    Task<T?> ExecuteScalarAsync<T>(
        string storedProcedure,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);
}
