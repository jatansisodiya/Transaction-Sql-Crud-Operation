---
name: new-api-endpoint
description: >
  Step-by-step guide for creating a new API endpoint in this project following
  the API-First approach. Use this skill when the user says: "add API", 
  "create endpoint", "new route", "add controller action", "new feature API",
  or "develop new API".
---

# New API Endpoint — Step-by-Step Guide

Follow these steps **in order** every time a new API endpoint is added to this project.

## Step 1: Check for Existing API

Before creating anything, search the codebase:
- Search controllers for a similar endpoint
- Check route names to avoid duplication
- If it exists, use the existing one — do NOT create a duplicate

## Step 2: Design the API Contract (API-First)

Define before writing any code:

```
Method:   POST / GET / PUT / DELETE
URL:      /api/v1/<resource>          (lowercase, no HTTP verbs)
Request:  { camelCase body params }
Response: { camelCase fields }
Codes:    200 (success), 400 (error), 401 (unauthorized), 429 (rate limit)
```

**Rules:**
- URL must be all lowercase
- No HTTP verbs in display name (not "GetUser" → use "User Profile")
- Body params in camelCase
- IDs in responses must use encrypted format

## Step 3: Create the Controller Action

- Max **40 lines** per method — split into service calls if longer
- Wrap in try/catch — never swallow exceptions silently
- Validate all inputs before processing
- Return proper HTTP status codes

```csharp
[HttpGet]
[Route("transactions")]
public async Task<IActionResult> GetTransactions(
    [FromQuery] int pageIndex = 0,
    [FromQuery] int pageSize = 10)
{
    try
    {
        var result = await _transactionService.GetAllAsync(pageIndex, pageSize);
        return Ok(result);
    }
    catch (Exception ex)
    {
        await _logger.LogErrorAsync(ex, Request);
        return BadRequest(new { message = "An error occurred." });
    }
}
```

## Step 4: Create the Service Layer

- Business logic goes here, NOT in the controller
- Use dependency injection
- Max 40 lines per method

## Step 5: Data Access — SQL Rules

- **ALWAYS** use parameterized queries
- **NEVER** concatenate user input into SQL strings
- Use `using` statements for `SqlConnection` and `SqlCommand`

```csharp
// ✅ CORRECT
using (var conn = new SqlConnection(_connectionString))
using (var cmd = new SqlCommand("SELECT * FROM Transactions WHERE Id = @id", conn))
{
    cmd.Parameters.AddWithValue("@id", id);
    // ...
}

// ❌ WRONG - SQL Injection risk
var sql = "SELECT * FROM Transactions WHERE Id = " + id;
```

## Step 6: Pagination (Required for All List APIs)

Every GET endpoint returning a list MUST support:
- `pageIndex` (zero-based)
- `pageSize` (default: 10)

## Step 7: Error Logging

Every error must:
1. Be logged to **MongoDB** (6-month TTL collection)
2. Send an **email** with: request details, error message, line number

## Step 8: Write Unit Tests

- Test the controller action
- Test the service layer
- Mock the database layer
- Aim for > 80% coverage

## Step 9: Update Documentation

- Add the endpoint to the ReadMe document
- Include: URL, method, request params, response structure, status codes
- Add proper XML comments to the controller action
