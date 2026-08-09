# Project Rules for Antigravity

These rules apply to ALL tasks in this project. Follow them strictly at all times.

---

## General Principles

- **NO hardcoded values** — never embed magic numbers, API keys, credentials, or URLs directly in source code. Use configuration files, environment variables, or named constants.
- **DRY (Don't Repeat Yourself)** — extract duplicated logic into reusable functions, classes, or modules.
- **Readability first** — use meaningful variable and function names; write well-commented code.
- **Functions must be ≤ 40 lines** — if a function exceeds 40 lines, split it into smaller, focused units.

---

## Memory Management

- Always release resources (database connections, file handles, streams) promptly.
- Use `using` statements in C# for any `IDisposable` objects (e.g., `SqlConnection`, `SqlCommand`).
- Never hold references longer than necessary to avoid memory leaks.
- Check memory usage in Visual Studio Diagnostic Tool when running or testing APIs.

---

## Error Management

- Never swallow exceptions silently — always log them with sufficient context.
- Log errors to MongoDB (with a 6-month TTL on the collection).
- Send error details (request info, error message, and code line number) via email notification.
- Provide user-friendly error messages; never expose sensitive information in error logs or responses.

---

## Performance

- Avoid unnecessary computations, redundant loops, or excessive database calls.
- Every GET API that returns a list **must** support `pageIndex` and `pageSize` parameters.
- Profile performance-critical areas using Visual Studio Diagnostic Tool.
- API load test response times must be **below 1 second**.

---

## Security

- Validate and sanitize **all** user inputs to prevent SQL injection, XSS, etc.
- Use parameterized queries — never concatenate user input into SQL strings.
- Encrypt sensitive headers (e.g., `merchantId`, `userId`) before sending.
- Store sensitive data securely; never store passwords in plain text.
- Use HTTPS for all API calls; implement rate limiting on every endpoint.
- Keep dependencies up-to-date to patch known vulnerabilities.
- Follow OWASP Top Ten security recommendations.

---

## API Design (API-First Approach)

- Design the API contract (endpoints, request/response formats, status codes) **before** implementation.
- **No HTTP verbs** (Get/Update/Delete/Create) in API display names.
- All URL characters must be **lowercase**; URLs should be professional and generalized.
- Body parameters must use **camelCase**.
- Response fields must use **camelCase** (e.g., `serviceTitle`).
- All IDs in responses must be in a consistent encrypted format.
- Use environment variables for base URLs — never hardcode them.
- Validate that the specific API does not already exist before creating a new one.

### Required HTTP Response Codes

| Code | Meaning |
|------|---------|
| `200` | Standard success response |
| `400` | Any exception / bad request |
| `401` | Unauthorized access |
| `429` | Rate limit exceeded |

### Documentation Requirements

- Every API must have a ReadMe document with full details.
- Use common snippets for comments and reusable code blocks.
- Add proper commenting for every piece of business logic.

---

## Testing Requirements

- Write **unit tests** for all new functionality.
- Use Jest + React Testing Library for frontend; aim for **>80% test coverage**.
- Use MSW (Mock Service Worker) for mocking API responses in frontend tests.
- Perform **load testing** for every API; response time must stay below 1 second.
- All APIs must be tested via the "Try It" feature in API documentation.

---

## Frontend (React) Standards

- Use **functional components with hooks** — no class components.
- Break UI into reusable components; no component should exceed 40 lines.
- Use **TypeScript** for type safety (recommended).
- Use **camelCase** for props and variables; **PascalCase** for component names.
- Centralize all API calls in the `services/` folder.
- Use React Context or Redux Toolkit for global state; avoid prop drilling.
- Sanitize user inputs before rendering; avoid storing sensitive data in `localStorage`.

---

## Reference Files

For full detail on these guidelines, see:
- `.github/copilot-instructions.md` — General coding checklist
- `.github/CODING_GUIDELINES.md` — React + API-First frontend standards
- `.github/CODING_GUIDELINES_API.md` — Full API-First lifecycle and design standards
