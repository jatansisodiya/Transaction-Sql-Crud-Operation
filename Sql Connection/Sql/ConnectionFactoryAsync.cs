using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Transaction.SQLConnection.Interfaces;

namespace Transaction.SQLConnection.Sql;

/// <summary>
/// Factory for creating SQL Server connections asynchronously.
/// </summary>
public sealed class ConnectionFactoryAsync : IConnectionFactoryAsync
{
    private readonly string _connectionString;
    private readonly ILogger<ConnectionFactoryAsync> _logger;

    public string ConnectionStringName { get; }

    public ConnectionFactoryAsync(IConfiguration configuration, ILogger<ConnectionFactoryAsync> logger, string connectionStringName = "DefaultConnection")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ConnectionStringName = connectionStringName;
        _connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");
    }

    public async Task<SqlConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
            _logger.LogDebug("SQL connection opened successfully.");
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open SQL connection.");
            await connection.DisposeAsync();
            throw;
        }
    }
}