using Microsoft.Data.SqlClient;

namespace Transaction.SQLConnection.Interfaces;

/// <summary>
/// Factory interface for creating SQL database connections asynchronously.
/// </summary>
public interface IConnectionFactoryAsync
{
    /// <summary>
    /// Creates and opens a new SQL connection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An open SqlConnection instance.</returns>
    Task<SqlConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the connection string (for diagnostic purposes only).
    /// </summary>
    string ConnectionStringName { get; }
}