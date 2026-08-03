# Transaction.SQLConnection

A production-ready, reusable SQL connection class library for .NET 10+ applications implementing the **Unit of Work pattern** and **CQRS (Command Query Responsibility Segregation)** with **ADO.NET** for MS SQL Server.

## Folder Structure

```text
Transaction.SQLConnection/
│
├── Transaction.SQLConnection.csproj
├── README.md
│
├── Interfaces/
│   ├── IConnectionFactoryAsync.cs
│   ├── ITransactionalRepositoryAsync.cs
│   └── IDbExecutorAsync.cs
│
├── Sql/
│   ├── ConnectionFactoryAsync.cs
│   ├── TransactionalRepositoryAsync.cs
│   └── DbExecutorAsync.cs
│
├── Mapping/
│   └── EntityMapper.cs
│
└── Exceptions/
    └── DatabaseException.cs
```

## Features

- ✅ **CQRS Support (Command Query Responsibility Segregation)** - Separate Read Queries (`isRead = true`) from Write Commands (`isRead = false`)
- ✅ **Centralized `CreateCommandAsync` Orchestration** - Single point for connection initialization, transaction attachment, and command setup
- ✅ **Unit of Work Pattern** - Transaction management across single or multiple operations
- ✅ **Stored Procedure Execution** - Full support for SPs with input/output parameters
- ✅ **Multiple Result Sets** - Support for SPs returning multiple SELECT result sets as tuples
- ✅ **Async/Await** - Fully asynchronous implementation with primary constructor syntax (.NET 10)
- ✅ **Clean Architecture & DI Ready** - Interface-based design with Microsoft Dependency Injection support

---

## CQRS (Command Query Responsibility Segregation) Support

The library supports CQRS separation in `ITransactionalRepositoryAsync` via the `isRead` parameter (default `false`):

- **Query / Read-only Operation (`isRead: true`)**: Opens a standalone connection without creating transaction locking or log flushing overhead.
- **Command / Write Operation (`isRead: false`, Default)**: Automatically begins a transaction (if none is active), executes the command, and commits on success or rolls back on failure.

### Centralized Command Orchestration (`CreateCommandAsync`)
All execution methods delegate to `CreateCommandAsync`, which manages connections and transactions centrally:

```csharp
private async Task<SqlCommand> CreateCommandAsync(
    string storedProcedureName,
    SqlParameter[]? parameters,
    int? commandTimeout,
    bool isRead,
    CancellationToken cancellationToken)
{
    // Centralized CQRS Connection & Transaction Management
    if (!isRead && !HasActiveTransaction)
    {
        await BeginTransactionAsync(cancellationToken);
    }
    else if (isRead && !HasActiveTransaction && _connection is null)
    {
        _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
    }

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
```

---

## Quick Start & CQRS Examples

### 1. CQRS Read Query (`isRead: true`)
```csharp
// Execute read query without transaction locking overhead
var users = await repository.ExecuteInTransactionAsync<List<User>>(
    "usp_GetAllUsers",
    parameters: null,
    isRead: true);
```

### 2. CQRS Write Command (`isRead: false`)
```csharp
// Execute write command with automatic transaction management
var newUserId = await repository.ExecuteInTransactionAsync<int>(
    "usp_CreateUser",
    [new SqlParameter("@UserName", "JohnDoe")],
    isRead: false);
```

### 3. Multiple Result Sets Query (`isRead: true`)
```csharp
// Read multiple result sets as a tuple
var (users, summary) = await repository.ExecuteMultipleResultSetsAsync<List<User>, UserSummary>(
    "usp_GetUsersWithSummary",
    isRead: true);
```

---

## Summary of Execution Methods

| Case | Method Signature | Return Type | CQRS Usage (`isRead`) |
|------|-----------------|-------------|------------------------|
| **1** | `ExecuteInTransactionAsync<int>(sp, params, isRead: false)` | `int` | Scalar Command (Insert/Update) |
| **2** | `ExecuteInTransactionAsync<User>(sp, params, isRead: true)` | `User` | Single Entity Query |
| **3** | `ExecuteInTransactionAsync<List<User>>(sp, params, isRead: true)` | `List<User>` | List Query |
| **4** | `ExecuteInTransactionAsync<T>(sp, params, resultSetIndex: 1, isRead: true)` | `T` | Specific Result Set Index Query |
| **5** | `ExecuteMultipleResultSetsAsync<T1, T2>(sp, params, isRead: true)` | `(T1, T2)` | Two Result Sets Query |
| **6** | `ExecuteMultipleResultSetsAsync<T1, T2, T3>(sp, params, isRead: true)` | `(T1, T2, T3)` | Three Result Sets Query |

---

## API Reference

### ITransactionalRepositoryAsync

| Method | Description |
|--------|-------------|
| `ExecuteInTransactionAsync<T>(..., isRead = false)` | Execute SP for CQRS Query (`isRead: true`) or Command (`isRead: false`) |
| `ExecuteMultipleResultSetsAsync<T1, T2>(..., isRead = false)` | Execute SP returning 2 result sets as tuple |
| `ExecuteMultipleResultSetsAsync<T1, T2, T3>(..., isRead = false)` | Execute SP returning 3 result sets as tuple |
| `BeginTransactionAsync()` | Manually start a transaction across multiple operations |
| `CommitAsync()` | Commit active transaction |
| `RollbackAsync()` | Rollback active transaction |
| `HasActiveTransaction` | Boolean property indicating active transaction state |

### IDbExecutorAsync (Standalone Lightweight Executor)

| Method | Description |
|--------|-------------|
| `QueryAsync<T>()` | Execute SP query and return list |
| `QuerySingleOrDefaultAsync<T>()` | Execute SP query and return single entity |
| `ExecuteAsync()` | Execute non-query SP and return rows affected |
| `ExecuteScalarAsync<T>()` | Execute SP and return scalar value |

---

## Best Practices

1. **Pass `isRead: true` for all Read operations**: Avoids transaction log overhead and prevents unnecessary row/table locks.
2. **Keep `isRead: false` (Default) for Write operations**: Guarantees atomic transaction execution and rollback protection.
3. **Use Primary Constructors**: Clean dependency injection registration for repository classes.
4. **Register as Scoped in DI**: Recommended for HTTP request-based lifecycles.



