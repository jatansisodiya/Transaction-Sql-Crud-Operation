# Copilot Instructions

This document outlines important coding considerations and best practices for contributors. Please review and follow these guidelines when using Copilot or submitting code.

---

## 1. Hardcoded Values

- **DO NOT** use hardcoded values (magic numbers, API keys, credentials, URLs) in source code.
- Use configuration files, environment variables, or constants for such values.
- Example (Bad):
  ```csharp
  string apiEndpoint = "https://example.com/api/data";
  ```
- Example (Good):
  ```csharp
  string apiEndpoint = ConfigurationManager.AppSettings["ApiEndpoint"];
  ```

## 2. Duplicate Code

- Eliminate duplicate code by extracting reusable logic into functions, classes, or modules.
- Promote DRY (Don't Repeat Yourself) principles.
- Use Copilot's suggestions to identify and refactor duplicated logic.

## 3. Coding Guidelines API

- Follow our project's [coding standards](./CODING_GUIDELINES_API.md) and naming conventions.
- Ensure code is readable, maintainable, and well-commented.
- Use meaningful variable and function names.
- Write unit tests for new functionality.

## 4. Memory Management

- Release resources (files, database connections, etc.) promptly.
- Use `using` statements (C#) or try-with-resources/finally blocks (Java, Python, etc.).
- Avoid memory leaks by not holding references longer than necessary.

## 5. Error Management

- Handle exceptions gracefully; do not swallow errors silently.
- Log errors with sufficient context for debugging.
- Provide user-friendly error messages where applicable.
- Avoid exposing sensitive information in error logs or messages.

## 6. Performance Management

- Optimize code for efficiency (avoid unnecessary computations, loops, or database calls).
- Profile and monitor performance-critical areas.
- Use efficient data structures and algorithms.
- Clean up unused objects and manage resources to prevent performance bottlenecks.

## 7. Security Checklist

- Validate and sanitize all user inputs to prevent injection attacks.
- Use secure authentication and authorization mechanisms.
- Store sensitive data securely (encrypt passwords, use secure storage).
- Keep dependencies up-to-date to patch known vulnerabilities.
- Avoid exposing implementation details or sensitive data in logs or error messages.
- Follow the [OWASP Top Ten](https://owasp.org/www-project-top-ten/) security recommendations.

---

**Remember:** Regularly review and update this checklist as project requirements or security standards evolve.
