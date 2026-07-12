## 📌 Summary
<!-- Explain the changes, motivation, and context. Why is this needed? -->

Fixes # (issue) / Relates to # (issue)

---

## ✅ Checklist

### Code Correctness
- [ ] Code performs the intended functionality (happy path verified).
- [ ] All edge cases are handled appropriately (empty, null, limits, error conditions).
- [ ] No off-by-one errors, incorrect conditions, or potential null references.
- [ ] Robust against invalid or unexpected inputs (validation & graceful failure paths).

### Code Readability
- [ ] Code is easy to understand without relying on extensive comments.
- [ ] Variable and function names are meaningful and descriptive.
- [ ] Comments (where present) are clear, concise, and helpful (no noise).
- [ ] Code is logically organized into small functions/classes with single responsibility.
- [ ] Magic numbers / hardcoded values are avoided (use named constants/configs).

### Coding Standards
- [ ] Adheres to project/org coding guidelines (style, lint rules, analyzers).
- [ ] Consistent formatting (indentation, spacing, braces, line length).
- [ ] Naming conventions are consistent (camelCase / PascalCase / snake_case as per standards).
- [ ] Handle null/undefined values appropriately to prevent runtime errors.
- [ ] Split code into functions if a function exceeds ~30–40 lines.
- [ ] Functions accept a max of 3 parameters; if more, use an options/object parameter.
- [ ] Resources are properly cleaned up (close files, connections, dispose objects as per language conventions).

### Hardcoded Values
- [ ] No hardcoded **credentials/secrets** (API keys, passwords, tokens, certificates).
- [ ] No hardcoded **file paths or URLs** (use configuration/environment variables).
- [ ] No hardcoded **configuration values** (ports, connection strings, feature flags).
- [ ] No **magic numbers** (replace with named constants).
- [ ] No hardcoded **business logic constants** (tax rates, discounts, thresholds).
- [ ] No hardcoded **UI/display strings** (use localization/resources).
- [ ] No **environment-specific values** (region codes, tenant IDs) in code.

### Duplicate Code
- [ ] No logic duplication across files/modules.
- [ ] Shared functionality extracted into reusable methods/utilities.
- [ ] No copy-paste of business rules without abstraction.

### Memory Management
- [ ] No memory leaks (dispose/close unmanaged resources; release large objects).
- [ ] Proper use of **IDisposable / using** (or language equivalent).
- [ ] Collections/lists cleared or scoped appropriately.
- [ ] Avoid unnecessary large in-memory allocations; use streaming/pagination where applicable.

### Error Handling & Logging
- [ ] Proper error handling for expected and unexpected scenarios.
- [ ] Meaningful error messages and logs are returned or recorded (no sensitive data).
- [ ] Logs are detailed enough for debugging but sanitized of secrets/PII.
- [ ] Each exception is logged with sufficient context and code location/line number when available.
- [ ] Try/catch at appropriate **parent level** (avoid blanket low-level swallowing).

### Performance
- [ ] No obvious performance bottlenecks or inefficiencies (avoid redundant work).
- [ ] Loops/recursion/nested conditions are optimized; avoid N+1 calls.
- [ ] Caching used where appropriate; cache invalidation is handled.
- [ ] Asynchronous/non-blocking I/O used where beneficial.

#### Database Performance
- [ ] If any **SQL procedure** changes: performance reviewed and evidence provided.
- [ ] If any **MongoDB query** changes: performance reviewed and **required indexes** created/updated.

### Security Checklist
- [ ] Sensitive data (passwords, API keys) stored and transmitted securely.
- [ ] Protections against **SQL injection**, **XSS**, **CSRF**, and similar vulnerabilities.
- [ ] Authentication and authorization implemented correctly (principle of least privilege).
- [ ] Error handling does not expose sensitive information.
- [ ] Do not return any **IDs** or sensitive identifiers as plain text where not appropriate.
- [ ] No hardcoded secrets; dependencies scanned (Snyk/CodeQL/OWASP) and high/critical issues resolved.

### Tests
- [ ] Unit tests cover new/changed logic (edge cases included).
- [ ] Integration/e2e tests updated as needed.
- [ ] All tests pass locally and in CI.

### Documentation
- [ ] README / Wiki / ADRs updated if relevant.
- [ ] Inline comments for complex logic or non-obvious decisions.
- [ ] Migration/rollout notes included (if applicable).

---

## 🔍 Reviewer Notes
<!-- Anything reviewers should pay special attention to?
     e.g., architectural decisions, trade-offs, known limitations -->

---

## 🚀 Deployment Impact
- [ ] No migration needed
- [ ] DB migration included
- [ ] Config/Secrets update required

---

## 📷 Screenshots / Logs (if UI or critical changes)
<!-- Add before/after screenshots or logs for easier review -->
