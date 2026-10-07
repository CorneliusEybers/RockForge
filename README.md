# RockForge

RockForge is a .NET 10 Web API created for the Virgin Active Digital Integration Technical Assessment.

The service manages weekly member commitments ("Rocks"), applies extensible category-specific validation, exposes structured and safe API errors, supports API-key authentication, provides correlated structured logging, and enriches member data from an external profile API with retry, timeout, and graceful-degradation behaviour.

The solution was intentionally implemented without a database or ORM, using in-memory storage as required by the assessment.

## Solution Structure

RockForge
- RockForge.API
- RockForge.Application
- RockForge.Domain
- RockForge.Infrastructure
- RockForge.Tests

### RockForge.API

Exposes the REST API endpoints and handles HTTP-specific concerns, including controllers, request/response models, API-key middleware, correlation-ID middleware, request logging, global exception handling, Swagger/OpenAPI configuration, and dependency-injection composition.

### RockForge.Application

Contains application use cases, workflows, validation, service orchestration, and contracts used by Infrastructure.

Examples include:

- IRockService
- RockService
- base Rock validation
- category validation strategy contracts
- enriched-profile orchestration
- profile client abstraction
- application-level exceptions

### RockForge.Domain

Contains the core business model and domain behaviour, including Rock, RockCategory, RockStatus, valid state-transition behaviour, and the domain-level state-transition exception.

### RockForge.Infrastructure

Contains technical implementations that interact with systems outside the core application. In this assessment it includes the typed HTTP client used to retrieve external member profile information.

### RockForge.Tests

Contains NUnit tests covering the important application and domain behaviour.

The current test suite intentionally focuses on the highest-value scenarios rather than testing framework behaviour.

# Getting Started

## Prerequisites
- .NET 10 SDK
- Visual Studio 2026 or another compatible .NET IDE
- HTTPS development certificate configured for ASP.NET Core

## Clone
in powershell
git clone https://github.com/CorneliusEybers/RockForge.git
cd RockForge

## Restore
from powershell
dotnet restore

## Build
powershell
dotnet build

## Run
Run the API project from Visual Studio, or:
from powershell
dotnet run --project .\RockForge.API\RockForge.API.csproj

The local HTTPS address used during development is:
https://localhost:7076


Swagger is available at:
https://localhost:7076/swagger

# API Authentication
All business endpoints require an API key supplied in the following header:
X-Api-Key
For assessment convenience, a development API key is included in `appsettings.Development.json`:
3f7b4a18-8d8b-4f0c-9d32-5d14b8e6c2a1

When using Swagger:
1. Open Swagger.
2. Click [Authorize] button
3. Enter the API key above.
4. Execute requests normally.
Swagger itself is intentionally accessible without an API key so that the evaluator can open the UI and provide the key through the Authorize function.

Missing or invalid keys return HTTP `401 Unauthorized`.

> Production note: the development key is committed only to make the assessment immediately runnable. In production the secret would not be stored in source control. It would be stored in Azure Key Vault and accessed through Managed Identity.

# API Endpoints

## Create a Rock
POST /members/{memberId}/rocks
Example request:
json
{
  "title": "Rock001234",
  "category": "Health",
  "dueDate": "2026-11-07",
  "note": "Cornelius001"
}

A successfully created Rock receives a generated `Guid`, belongs to the `memberId` in the route, and starts in `Pending` status.

## Get Rocks for a Member
GET /members/{memberId}/rocks

Optional status filter:
GET /members/{memberId}/rocks?status=Pending
GET /members/{memberId}/rocks?status=Completed
GET /members/{memberId}/rocks?status=Missed

## Update Rock Status
PATCH /members/{memberId}/rocks/{rockId}

Example:

json
{
  "status": "Completed"
}

Valid transitions are:
Pending -> Completed
Pending -> Missed

Transitions from a final state to another state are rejected. Invalid state transitions return HTTP `422 Unprocessable Entity`.

## Get Enriched Member Profile
GET /members/{memberId}/profile/enriched

This endpoint combines locally stored Rocks with profile data returned by JSONPlaceholder.
Example successful shape:
json
{
  "profile": {
    "id": 1,
    "name": "Leanne Graham",
    "username": "Bret",
    "email": "Sincere@april.biz",
    "phone": "1-770-736-8031 x56442",
    "website": "hildegard.org"
  },
  "rocks": [],
  "enrichmentAvailable": true
}

If the third-party profile service is unavailable after resilience handling, RockForge still returns the locally available Rocks:
json
{
  "profile": null,
  "rocks": [],
  "enrichmentAvailable": false
}

The endpoint therefore degrades gracefully instead of returning HTTP `500` simply because the enrichment provider is unavailable.

# Input Validation

General Rock validation is centralised in the Application layer.

The base validation rules are:

- `memberId` must not be empty
- `title` must not be empty or whitespace
- `dueDate` must not be in the past
- `category` must be a defined `RockCategory`

Validation errors are represented by a typed application exception and allowed to flow to the central API exception boundary.

The API returns a safe RFC 7807-style Problem Details response such as:

json
{
  "title": "Validation failed",
  "status": 400,
  "detail": "Title must not be empty or whitespace.",
  "instance": "/members/007CTE001/rocks",
  "traceId": "..."
}

The client receives only information that is safe and useful to the API consumer. Internal implementation details, stack traces, SQL information, assembly names and internal paths are not exposed in public error responses.

# Category Validation Strategy
Category-specific Rock rules are implemented using the Strategy Pattern.
Each category has its own implementation of:
IRockValidationStrategy

Current strategies:

| Category | Rule                                          |
|----------|-----------------------------------------------|
| Revenue  | Due date must fall within the current quarter |
| Health   | Title must contain at least 10 characters     |
| Career   | Note is required                              |
| Other    | No additional category-specific validation    |

The strategies are registered independently through dependency injection.
When a future category is added, its category-specific behaviour can be implemented in a new strategy class without modifying the existing category strategies.
This keeps the validation design aligned with the Open/Closed Principle:

> Open for extension, closed for modification.

# In-Memory Storage

The assessment requires in-memory storage and no database or ORM.

`RockService` therefore uses an in-memory concurrent dictionary.

The Rock service is registered as a Singleton so that the in-memory data survives across HTTP requests for the lifetime of the application process.

This is appropriate for the assessment, but not for a scaled production environment because data is lost when the application process restarts and multiple application instances would each have their own independent in-memory copy.

For production, the storage implementation should be replaced behind an abstraction with a durable external data store.

# Global Error Handling

The API uses one central exception-handling boundary rather than endpoint-level `try/catch` blocks.

Known exceptions are mapped to safe HTTP responses.

| Failure | HTTP Status          |
|--------------------------|-----|
| Validation failure       | 400 |
| Rock not found           | 404 |
| Invalid state transition | 422 |
| Unexpected failure       | 500 |

Unexpected exceptions return a generic safe response:

json
{
  "title": "An unexpected error occurred",
  "status": 500,
  "detail": "An unexpected error occurred while processing the request.",
  "instance": "/members/007CTE001/rocks",
  "traceId": "..."
}

The underlying exception remains available internally through application logging for support and diagnosis.

This keeps the API contract safe while preserving technical detail for developers and operational support.

# Structured Logging and Correlation

RockForge uses `Microsoft.Extensions.Logging` with JSON console logging.

For every request:

- an incoming `X-Correlation-Id` is used when provided
- otherwise a new correlation ID is generated
- the correlation ID is returned to the caller
- the correlation ID is added to the logging scope
- every log written during the request inherits the same correlation ID
- request method, path, outcome, status code and duration are logged

This avoids manually passing correlation IDs through controllers, services and validation components.

A support engineer can use the returned correlation ID to locate the related request logs.

Example response header:

text
X-Correlation-Id: 52a450292aa0468b9b6cacd023712abe


Logging uses structured message templates rather than interpolated log strings.

---

# External Profile Integration and Resilience

The profile provider is treated as an unreliable external dependency.

The external integration is isolated behind a typed `HttpClient` implementation and an application-level interface.

The resilience design includes:

- typed `HttpClient`
- timeout handling
- minimum three retries for transient failures
- exponential backoff
- jitter
- structured retry logging
- graceful degradation

Each retry records useful operational information such as retry attempt, delay, failure reason, and request correlation context.

Exponential backoff means that RockForge waits progressively longer between retries. Jitter adds a small random variation to those delays so that multiple clients do not all retry a failing dependency at exactly the same moment.

The design objective is:

text
External dependency failure
        |
        v
Retry transient failure
        |
        v
Still unavailable
        |
        v
Return locally available Rocks
        |
        v
enrichmentAvailable = false


The application can therefore continue to fulfil the useful local part of the request even when enrichment is unavailable.

# Automated Tests

The test project uses NUnit.

The current assessment-focused test suite covers two high-value scenarios for each testable API behaviour.

### Rock creation

- valid Rock is created with `Pending` status
- Health Rock with an invalid short title is rejected

### Rock retrieval

- only Rocks belonging to the requested member are returned
- status filtering returns only matching Rocks

### Rock status transitions

- `Pending -> Completed` succeeds
- `Completed -> Missed` is rejected

### Profile enrichment

- available profile data is combined with Rocks
- unavailable profile service still returns Rocks with `enrichmentAvailable = false`

Run the test suite with:

powershell
dotnet test

# Manual Test Evidence

`Test Data.txt` is included in the repository as a record of the manual request/response scenarios used while developing and verifying the API.

It contains examples covering:

- Rock creation
- input validation
- category validation
- not-found behaviour
- structured correlation headers
- API-key authentication
- profile enrichment
- unavailable enrichment

# Development Workflow

I have also included `Investigate.txt`.

This file is intentionally not a polished requirements document. It is an example of the working technical-analysis document I use as part of my normal development process.

For each task I generally:

1. Create a dedicated work-item folder.
2. Create an `Investigate.txt`.
3. Inspect the existing technical flow before changing it.
4. Record assumptions, findings and implementation decisions.
5. Where relevant, create an `Investigate.sql` to inspect the existing data before modifying database behaviour.
6. Implement the work on a dedicated Git branch.
7. Test it.
8. Commit and raise a pull request.
9. Merge the completed work into `main`.
10. Pull the latest `main` before beginning the next task.

The branch sequence used for this assessment was:

text
000-Create-GitRepository-VsSolution-AppArchitecture
001-Rock-Management
002-Input-Validation
003-Category-Validation-Strategies
004-Global-Error-Handling
005-Structured-Logging-Correlation
006-Api-Key-Authentication
007-Profile-Integration-Resilience
008-Tests
009-Readme


The normal cycle is:

text
main -> branch -> implement -> test -> commit / PR -> merge into main -> pull main


`Investigate.txt` therefore provides additional visibility into not only what was built, but how the implementation was investigated and reasoned about during development.

# Azure Production Deployment Architecture

## Compute Choice

For a production deployment I would use **Azure App Service** for RockForge.

RockForge is a conventional ASP.NET Core request/response Web API with middleware, authentication, structured logging and outbound HTTP integration.

Azure App Service is a strong fit because it provides:

- managed hosting for ASP.NET Core
- HTTPS support
- horizontal and vertical scaling
- deployment slots
- Managed Identity integration
- Application Insights integration
- straightforward CI/CD support
- low infrastructure-management overhead

I would not initially choose Azure Functions because RockForge is an always-available REST API rather than an event-driven or short-lived serverless workload.

**Azure Container Apps** would also be a suitable option if the wider platform standardised on containers or if RockForge later became one service within a broader microservice architecture.

For the current workload, App Service provides the simpler operational model.

## Production Data Storage

The assessment intentionally uses in-memory storage.

For production, the in-memory implementation would be replaced by durable external persistence so that application instances can remain stateless and can scale horizontally.

The Application layer should continue to depend on an abstraction rather than a concrete database technology. This allows the persistence implementation to evolve without moving data-access concerns into the Domain or API layers.

## API Management

The production API would be exposed through **Azure API Management (APIM)**.

APIM would provide central API-governance capabilities such as:

- authentication and subscription management
- API-key or token validation
- rate limiting and quotas
- API versioning
- central policy enforcement
- request and response transformation where required
- analytics and operational visibility
- controlled exposure of internal backend services

The API-key middleware implemented in this assessment demonstrates the required application-level behaviour.

In a larger production platform, some or all of this responsibility could be moved to APIM according to the organisation's security and platform standards.

## Azure Front Door versus API Management

Azure Front Door and Azure API Management solve different problems.

### Azure API Management

APIM governs and manages the API itself. It is primarily concerned with consumers, API policies, security, throttling, versioning, subscription management, and API lifecycle governance.

### Azure Front Door

Front Door operates at the global HTTP edge.

I would introduce it where the system required:

- Web Application Firewall protection
- multi-region routing
- global failover
- geographic routing
- global edge TLS termination
- improved global availability

For a small single-region API, I would not add Front Door simply for architectural completeness.

For a globally exposed multi-region production deployment, a possible request flow would be:

text
External Client
      |
      v
Azure Front Door
      |
      v
Azure API Management
      |
      v
Azure App Service
      |
      +----> Production Data Store
      |
      +----> External Profile Provider


This keeps edge-routing concerns, API-governance concerns and application concerns separate.

## Secrets and Configuration

Production secrets must not be committed to source control.

Secrets such as API keys, credentials, connection strings, and third-party integration secrets would be stored in **Azure Key Vault**.

RockForge would access Key Vault through **Managed Identity**, avoiding the need to store a second credential simply to retrieve the first one.

The application would continue reading its settings through the standard ASP.NET Core configuration abstractions so that business code remains independent of where configuration is physically stored.

## Observability

Production telemetry would be collected through:

- Azure Application Insights
- Azure Log Analytics

The existing structured logging and correlation-ID implementation already provides the foundation for production tracing.

Monitoring would include:

- request volume
- response latency
- failed requests
- unhandled exceptions
- external dependency failures
- retry activity
- timeout activity
- health and availability checks

Operational alerts would be created against agreed service thresholds.

The correlation ID returned to API consumers would make it possible for support staff to trace a reported request through its related logs.

## Infrastructure as Code

I would provision the Azure environment using **Bicep**.

The infrastructure definition would include resources such as:

- App Service Plan
- App Service
- API Management
- Key Vault
- Application Insights
- Log Analytics
- Managed Identity
- RBAC assignments
- optionally Azure Front Door for a global deployment
- production data services when introduced

Infrastructure as Code keeps environments repeatable and allows infrastructure changes to go through source control, pull-request review and automated deployment.

## CI/CD

I would use either GitHub Actions or Azure DevOps depending on the organisation's existing delivery platform.

A production pipeline would broadly perform:

1. Restore NuGet packages.
2. Build the solution in Release configuration.
3. Run automated tests.
4. Fail immediately if build or tests fail.
5. Run static-analysis or quality gates where required.
6. Provision or update Azure infrastructure through Bicep.
7. Deploy to a non-production environment or App Service deployment slot.
8. Execute smoke and health checks.
9. Promote or swap the validated release into production.
10. Retain deployment history and support controlled rollback.

Production promotion should use approval controls appropriate to the organisation.

# NuGet Packages

The solution uses the following notable packages:

- `Swashbuckle.AspNetCore`
- `Microsoft.Extensions.Http.Resilience`
- `Microsoft.Extensions.Logging.Abstractions`
- `NUnit`
- `NUnit3TestAdapter`
- `Microsoft.NET.Test.Sdk`

The exact package versions are available in the project files.

# Design Trade-offs and Improvements

Given more time, I would consider the following improvements:

- replace in-memory persistence with durable storage behind an abstraction
- add integration tests around the API middleware pipeline
- add more validation strategy tests
- add explicit tests for API-key authentication
- add correlation-ID middleware tests
- add resilience-policy tests using a controlled HTTP test handler
- add health-check endpoints
- add API versioning
- introduce production metrics and operational dashboards
- define the Azure infrastructure in Bicep
- add a complete CI/CD workflow
- move production secrets to Key Vault
- define production authentication according to the consuming-system requirements rather than relying solely on a static API key

I intentionally avoided introducing unnecessary infrastructure into the assessment implementation where the written requirement could be met clearly with a smaller design.

# AI Usage

AI tooling was used extensively during this assessment as a development assistant and technical reviewer.

My use of AI included:

- discussing architecture and separation of responsibilities
- reviewing design choices against the assessment requirements
- exploring established .NET patterns before implementation
- generating and reviewing implementation examples
- discussing exception-handling and logging design
- reviewing resilience patterns
- helping structure unit-test scenarios
- assisting with documentation and README structure

I worked through, compiled, debugged and tested the implementation incrementally rather than treating generated code as an unquestioned final answer.

AI was used to accelerate investigation, expose established patterns and challenge design decisions.

The implementation decisions, debugging, integration, verification and final solution remained part of the development process.

# Final Notes

The primary objective of RockForge was not only to make the required endpoints function, but to provide a small API with production-oriented engineering characteristics:

- clear separation of responsibilities
- extensible validation
- centralised error handling
- safe public errors
- structured correlated logging
- protected endpoints
- resilient third-party integration
- graceful degradation
- automated tests
- documented production deployment thinking

Supporting working notes and manual verification evidence are available in:

text
Investigate.txt
Test Data.txt