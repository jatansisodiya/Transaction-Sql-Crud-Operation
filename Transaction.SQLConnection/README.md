# Transaction.SQLConnection

A production-ready, reusable SQL connection class library for .NET 8+ applications implementing the **Unit of Work pattern** with **ADO.NET** for MS SQL Server.

## Folder Structure

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
├── Extensions/
│   └── ServiceCollectionExtensions.cs
│
├── Mapping/
│   └── EntityMapper.cs
│
└── Exceptions/
    └── DatabaseException.cs
	
## Features

- ✅ **Common SQL Connection Management** - Centralized connection factory
- ✅ **Unit of Work Pattern** - Transaction management across multiple operations
- ✅ **Stored Procedure Execution** - Full support for SPs with parameters
- ✅ **Single Transaction Across Multiple SPs** - Execute multiple SPs in one transaction
- ✅ **Automatic Rollback** - If any SP fails, all previous operations roll back
- ✅ **Multiple Result Sets** - Support for SPs returning multiple SELECT results
- ✅ **Async/Await** - Fully asynchronous implementation
- ✅ **Clean Architecture** - Interface-based design with DI support
- ✅ **Exception-Safe** - Proper resource cleanup and error handling

---

## Installation

### 1. Add Project Reference

### 2. Configure Connection String

### 3. Register Services

---

## Quick Start

### Basic Repository Implementation

---

## Summary of All Cases

| Case | Method Signature | Return Type | Description |
|------|-----------------|-------------|-------------|
| **1** | `ExecuteInTransactionAsync<int>(sp, params)` | `int` | Scalar value (identity, count, etc.) |
| **2** | `ExecuteInTransactionAsync<User>(sp, params)` | `User` | Single entity |
| **3** | `ExecuteInTransactionAsync<List<User>>(sp, params)` | `List<User>` | List of entities |
| **4** | `ExecuteInTransactionAsync<T>(sp, params, resultSetIndex: 1)` | `T` | Specific result set by index |
| **5** | `ExecuteMultipleResultSetsAsync<T1, T2>(sp, params)` | `(T1, T2)` | Two result sets as tuple |
| **6** | `ExecuteMultipleResultSetsAsync<T1, T2, T3>(sp, params)` | `(T1, T2, T3)` | Three result sets as tuple |

---

## Detailed Examples

### Case 1: Return Scalar Value (int)

### Case 2: Return Single Entity (User)
### Case 3: Return List of Entities (List<User>)
### Case 4: Return Specific Result Set by Index
### Case 5: Return Multiple Result Sets as Tuple (T1, T2)
### Case 6: Return Multiple Result Sets as Tuple (T1, T2, T3)

---	

## Transaction Management

### Single SP Transaction (Automatic)

When calling `ExecuteInTransactionAsync` without an active transaction, it automatically:
1. Begins transaction
2. Executes SP
3. Commits on success / Rolls back on failure

### Multiple SPs in Single Transaction (Manual)

For multiple SPs that must succeed or fail together:

---

## Transaction Flow Diagram


---

## API Reference

### ITransactionalRepositoryAsync

| Method | Description |
|--------|-------------|
| `ExecuteInTransactionAsync<T>()` | Execute SP with auto/manual transaction |
| `ExecuteMultipleResultSetsAsync<T1, T2>()` | Execute SP returning 2 result sets |
| `ExecuteMultipleResultSetsAsync<T1, T2, T3>()` | Execute SP returning 3 result sets |
| `BeginTransactionAsync()` | Start a new transaction manually |
| `CommitAsync()` | Commit the current transaction |
| `RollbackAsync()` | Rollback the current transaction |
| `HasActiveTransaction` | Check if transaction is active |

### IDbExecutorAsync (Standalone, No Transaction)

| Method | Description |
|--------|-------------|
| `QueryAsync<T>()` | Execute SP and return list |
| `QuerySingleOrDefaultAsync<T>()` | Execute SP and return single entity |
| `ExecuteAsync()` | Execute non-query SP |
| `ExecuteScalarAsync<T>()` | Execute SP and return scalar |

---

## Best Practices

1. **Use `using` or `await using`** for repository instances to ensure cleanup
2. **Wrap multi-SP operations** in try-catch with explicit rollback
3. **Use `resultSetIndex`** when you need only one result set from a multi-result SP
4. **Use tuple methods** when you need all result sets at once
5. **Register repositories as Scoped** in DI for per-request lifecycle

---

## Error Handling



