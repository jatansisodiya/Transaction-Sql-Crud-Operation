namespace Transaction.SQLConnection.Exceptions;

/// <summary>
/// Custom exception for database-related errors with context information.
/// </summary>
public class DatabaseException : Exception
{
    /// <summary>
    /// The stored procedure that caused the exception.
    /// </summary>
    public string? StoredProcedure { get; }

    /// <summary>
    /// The result set index being read when the exception occurred.
    /// </summary>
    public int? ResultSetIndex { get; }

    public DatabaseException(string message) : base(message) { }

    public DatabaseException(string message, Exception innerException) : base(message, innerException) { }

    public DatabaseException(string message, string storedProcedure, Exception innerException)
        : base(message, innerException)
    {
        StoredProcedure = storedProcedure;
    }

    public DatabaseException(string message, string storedProcedure, int resultSetIndex, Exception innerException)
        : base(message, innerException)
    {
        StoredProcedure = storedProcedure;
        ResultSetIndex = resultSetIndex;
    }
}