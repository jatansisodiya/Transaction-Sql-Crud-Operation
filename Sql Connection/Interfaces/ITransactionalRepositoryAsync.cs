using Microsoft.Data.SqlClient;

namespace Transaction.SQLConnection.Interfaces;

/// <summary>
/// Repository interface for transactional stored procedure execution.
/// Supports single and multiple result sets with automatic transaction management.
/// </summary>
public interface ITransactionalRepositoryAsync : IAsyncDisposable
{
    /// <summary>
    /// Indicates whether a transaction is currently active.
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Executes a stored procedure within a transaction context.
    /// Supports returning: int (scalar), single entity (T), or list (List&lt;T&gt;).
    /// </summary>
    /// <typeparam name="T">Return type: int, T (entity), or List&lt;T&gt;</typeparam>
    /// <param name="storedProcedureName">Name of the stored procedure to execute.</param>
    /// <param name="parameters">Optional SQL parameters.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="resultSetIndex">Index of result set to read (0 = first, 1 = second, etc.).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of type T.</returns>
    Task<T> ExecuteInTransactionAsync<T>(
        string storedProcedureName,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        int resultSetIndex = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a stored procedure and returns two result sets as a tuple.
    /// </summary>
    /// <typeparam name="T1">Type for first result set.</typeparam>
    /// <typeparam name="T2">Type for second result set.</typeparam>
    /// <param name="storedProcedureName">Name of the stored procedure.</param>
    /// <param name="parameters">Optional SQL parameters.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple containing both result sets.</returns>
    Task<(T1 Result1, T2 Result2)> ExecuteMultipleResultSetsAsync<T1, T2>(
        string storedProcedureName,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a stored procedure and returns three result sets as a tuple.
    /// </summary>
    /// <typeparam name="T1">Type for first result set.</typeparam>
    /// <typeparam name="T2">Type for second result set.</typeparam>
    /// <typeparam name="T3">Type for third result set.</typeparam>
    /// <param name="storedProcedureName">Name of the stored procedure.</param>
    /// <param name="parameters">Optional SQL parameters.</param>
    /// <param name="commandTimeout">Optional command timeout in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple containing all three result sets.</returns>
    Task<(T1 Result1, T2 Result2, T3 Result3)> ExecuteMultipleResultSetsAsync<T1, T2, T3>(
        string storedProcedureName,
        SqlParameter[]? parameters = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction manually.
    /// Use for scenarios requiring multiple stored procedure calls in a single transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackAsync();
}
