# Introduction
The API-First Approach is a design and development methodology that emphasizes the creation
of Application Programming Interfaces (APIs) before building the underlying applications. This
approach ensures that the APIs are well-documented, standardized, and ready for consumption by
various clients (web, mobile, third-party systems, etc.) before the implementation of the service
logic begins.

## Scope
The primary goal of this document is to guide teams in adopting an API-First approach for building
software solutions. The procedure document will provide a comprehensive framework for
designing APIs before implementing the backend or client-side logic, ensuring consistency,
standardization, and reusability of APIs across applications.

## Review Cycle
This document shall be reviewed annually or as needed in response to significant changes in the
development process or coding methods.

## Definitions
Term
Description
API
Application Programming Interface
# Benefits of API-first Approach
## Increased Integration Speed
APIs allow different software systems to connect quickly, reducing the time it takes to integrate new
features and services.
## Reusability and Duplication
Developers can reuse APIs across multiple projects, cutting down on repetitive work and saving
time on coding and testing.
## Parallel Development
Frontend and backend teams can work in parallel since both know the API specification in advance.
## Greater Focus on Security
Designing APIs early forces teams to think about security, authentication, and authorization
mechanisms right from the beginning. This can result in better-secured applications, as teams are
less likely to overlook security aspects when APIs are the primary point of interaction.
## Improved Testing and Quality Assurance
API testing can be automated early in the process. This helps detect issues sooner and ensures the
API behaves as expected. The ability to mock API responses and endpoints allows for easier testing
of both client and server components without requiring the full backend infrastructure to be in place.
## Faster Onboarding for New Developers
New developers or external teams can quickly get up to speed with a project because the API contract
provides them with a clear understanding of how the system works and how to interact with it. Since
the API is well-defined, developers can start building and integrating faster without needing to read
through a lot of legacy code.
## Consistency
Using standardized API formats (like Swagger) ensures uniformity and predictability.
# API - First Design
The API-First approach is a software development methodology where the design and development
of the API are prioritized before the development of the application or services that consume the
API. This approach focuses on creating the API contract (i.e., the specification for the API
endpoints, request/response formats, etc.) before starting the implementation of the backend logic.
The goal is to ensure that all components (frontend, backend, and services) are aligned around a
clear, well-documented, and standardized API interface.

## Key Steps in the API-First Approach
### Define the API Specification
Define the API using a standard format. This should include endpoint definitions, request/response
formats, status codes, and any relevant error messages.
### Validate the API Design
When validating an API, first check whether the specific API already exists. If it does, use the
existing one.
Make sure the design is clear and well-documented. Stakeholders should review the API contract
to ensure it meets the application's requirements and expectations.
### Generate API Documentation and Mock Server
Use tools (e.g., Readme UI, Postman) to generate interactive API documentation and create mock
servers to simulate the API's behavior before implementation.
### Implement the API
Backend developers implement the actual logic for each API endpoint, following the contract
defined earlier.
### Test the API
Automated tests are run to validate that the API functions as expected, adhering to the defined
contract.
### Integrate with Frontend and Other Services
Frontend developers use the API specifications to develop user interfaces, while other services or
systems can begin integrating with the API.
## API Life cycle
- SRS documents kick off with manager
The Software Requirements Specification (SRS) document is initiated by the project manager to
define the business goals, functional and non-functional requirements, and the expectations for the
API. This document serves as the foundation for the entire API development process.
- Document of API SDD
The API Software Design Document (SDD) outlines the technical blueprint for the API, detailing
the structure, components, and interactions necessary for its development. This includes:
- How many API
Identify the total number of APIs to be created. Each API will be responsible for a specific
function or resource in the system, and this number should align with the business requirements.
- How many Classes
Define the number of classes or components involved in the API. This could include controllers,
services, models, and any auxiliary classes that support the API functionality.
- Request
Describe the types of HTTP requests (such as GET, POST, PUT, DELETE) each API will
handle. Include the required request headers, query parameters, URL structures, and body
content if applicable.
- Response
Define the structure of the API responses, including the data returned, HTTP status codes (e.g.,
# OK, 400 Bad Request), and error messages. This ensures consistency in the responses sent
by the API.
- Prepare SDD document
Create the complete Software Design Document (SDD), which provides a high-level overview
of the architecture, API endpoints, data flow, authentication methods, and other key details.
This document helps developers follow a standardized approach for building the API.
- Prepare API documentation
Prepare comprehensive API documentation for developers and stakeholders. This should
include detailed descriptions of each endpoint, request/response formats, parameters, error
codes, and example use cases. The documentation should be clear, easy to understand, and
complete to help others integrate with the API smoothly.
- Follow all API guidelines
Ensure that the API design and development follow established coding standards and guidelines.
This includes consistency in naming conventions, clear documentation of methods and endpoints,
security best practices (e.g., authentication, authorization), and versioning conventions to ensure
the API is maintainable and scalable.
- URL should be more professional and generalized. All characters should be in small
i. Group names are not required for third-party APIs.
cases.
- API Display name
  - Do not include HTTP verbs like Get/Update/Delete/Create in the API name.
- API Description
  - Use ChatGPT for generating the API descriptions.
- Body Parameters
ii.
If query parameters are required, they should be part of the URL.
iii.
If more query string parameters are needed, use the POST method instead
of GET.
b. Body params should be in camelCase.
c. Parameters must include display names with appropriate row content.
d. Set a "required" flag for mandatory parameters.
e. Ensure proper data types for parameters.
f. Set default values where applicable.
- Response Code
  - 200k: Standard Success Response.
b. Fail 400: any exception.
c. Fail 401: Unauthorized access.
d. Fail 429: Rate Limit
- CURL Request
  - All APIs developed should be tested through the "TRY It" feature on
docs..com.
- Response
  - Responses should follow camelCase (e.g., serviceTitle).
b. All IDs should be in the same encrypted format every time (e.g., 98 -
"dfsdfkshfdk").
c. Error messages in responses should clearly describe the problem (e.g., "The userId
provided does not match with the merchant").
d. The response structure should be well-defined.
- Development
Develop the API according to the specifications in the SDD and API documentation. This involves
writing the backend code for each endpoint, ensuring proper request validation, response formatting,
and handling errors. The development phase should ensure that the API is both functional and
secure.
- Security audit
Conduct a detailed security audit of the API to identify and mitigate potential vulnerabilities. The
security audit ensures that the API is protected against common security threats by implementing
the following measures:
- Credential Stuffing: Use rate limiting, multi-factor authentication (MFA), and account
lockouts after failed attempts to prevent automated login attempts.
- Brute Force: Implement account lockouts, strong password policies, and rate limiting to block
brute force attacks.
- Token Theft: Secure tokens using encryption, secure transmission (SSL/TLS), and proper
storage practices like HttpOnly and SameSite cookies.
- Denial of Service (DoS): Apply rate limiting, traffic monitoring, and DDoS protection services
to prevent overload.
- Cross-Site Scripting (XSS): Sanitize inputs, encode outputs, and implement Content Security
Policy (CSP) headers to block malicious scripts.
- SQL Injection: Use parameterized queries, validate inputs, and employ web application
firewalls (WAF) to block injections.
- Data Breaches: Encrypt sensitive data, use salted password hashing, and implement strong
access controls and logging to prevent breaches.
- Cross-Site Request Forgery (CSRF): Use anti-CSRF tokens, validate headers, and apply
SameSite cookies to prevent unauthorized actions.
- Code Review
Conduct a code review session where peers inspect the code to ensure it adheres to coding standards,
is maintainable, efficient, and free from defects. This collaborative process allows for the
identification of bugs, optimization opportunities, and potential improvements in functionality.
- Unit Test
Write unit tests to validate individual components or functions within the API. These tests ensure
that each function performs as expected in isolation, reducing the risk of errors during integration.
Unit tests help catch bugs early and improve the reliability of the API.
- Load Test
Perform load testing to evaluate how the API behaves under varying levels of traffic. For any API
load test, the response time should ideally be below one second to ensure optimal user experience
and system efficiency.
- Handover
- Submit All Checklist
Ensure that all required documentation, code, and tests are completed by submitting a
comprehensive checklist that verifies every task and requirement has been fulfilled. This
checklist ensures that nothing is overlooked and the API is ready for deployment or handover.
- Handover meeting
A formal handover meeting is conducted to transition the completed API to the respective
team. During this meeting, the development team reviews the API documentation,
demonstrates its functionality, and answers any questions. The team also provides
instructions for usage, monitoring, and troubleshooting, ensuring a smooth handover for
ongoing maintenance or future updates.
## API Development Guidelines
- ReadMe document should be generated with all details.
- Use common snippets for comment and common code.
- Create a new function if more then 40 lines are in the same function.
- Proper commenting for every business logic.
- Page index / page size required for every get API list.
- Error management and way where needs to log.
- Errors should come in email only with proper request and error details and code line

number.
- Memory management.
- Logs should be in mongo [6-month ttl in mongo collection].
- Check memory in visual studio when run any API.

## API Documentation
### Accessing ’s API Documentation
- Visit  Documentation - (-enterprise-group).
- Click on ‘Get Started’ and then select ‘API Reference’. This will redirect you to the 
API documentation.
- Introduction - The introduction page provides an overview of ‘What You Can Achieve with
 APIs’, outlining the key capabilities and opportunities for integration with the
 platform.
- Changelog - The next section, Changelog, highlights significant updates to the 
developer platform. Note that only major changes are documented here, not every minor
update.
- Generating an Access Token - To proceed with integration, navigate to the ‘Generate Access
Token’ section. This API is used to generate an access token utilizing the client credentials
available in your  Developer Settings.
Here is the list of APIs for reference

### Request Parameters
Add the required body parameters as specified in the documentation.

This image demonstrates a curl request based on given parameters in the ‘Shell’ language for assigning
an employee.
After providing the required body parameters, the API returns a response.


### Response Body
The response body includes:
- responseID: A unique identifier for the API request.
- responseCode: The status code associated with the response.
- message: A descriptive message regarding the response.
- data: Any additional information returned by the API.
- accessToken: The generated token required for accessing other API endpoints.
After generating the access token, the next step in the documentation directs you to the ‘Assign
Employee’ section.

### Response Details
The API may respond with one of the following status codes:
- 200 – OK: The request was successful.
- 400 – Bad Request: There was an issue with the request parameters.
- 401 – Unauthorized: Authentication failed due to invalid credentials.
- 429 – Too Many Requests: Rate limit exceeded; too many requests were sent in a short period.

This will ensure that developers can easily interact with the API, make accurate requests, handle
responses correctly, and fix issues during the integration process.

## API Review
### Monitor API
We monitor CPU and Memory using Visual Studio Diagnostic Tool

### Security Validation
We are currently validating the following security points for each API:
- Broken Authentication / Token authentication
- Rate limiting
- XSS
- Missing server-side validation
- No length Protection
- IDOR,
- Arbitrary File Upload,
- sensitive information Disclosure,
- security misconfigurations,
- CORS misconfiguration
- Cleartext (sensitive information)
- Parameter tampering
- Missing Validation / Data validation
- information disclosure,
- Internal Path disclosed
- HTTP Security response headers missing
- Open Redirection
- Server Path Disclosed
- Server versions disclosed
- Improper session handling
- Plain text password
- Snyk scan
By implementing the above points, we will mitigate the below listed security attacks
- credential stuffing
- brute force
- token theft
- Denial of Service
- Cross-Site Scripting (XSS)
- SQL injection attacks
- data breaches.
- Cross-Site Request Forgery
### Load Test Result
For any API load test, the response time should ideally be below one second to ensure optimal user
experience and system efficiency.
