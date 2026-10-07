# RockForge

A place where commitments are forged, tested, and completed.

Technical Assessment Doctorly VirginActive

TechAssessVirginActive



Nuget Packages Installed:

=========================

Swashbuckle.AspNetCore for Swagger...



Scrum Tasks:

============

000-Create-GitRepository-VsSolution-AppArchitecture

001-Rock-Management

002-Input-Validation

003-Category-Validation-Strategies

004-Global-Error-Handling

005-Structured-Logging-Correlation

006-Api-Key-Authentication

007-Profile-Integration-Resilience

008-Tests-and-Readme



Scrum Task Cycle:

=================

main -> branch -> implement -> test -> commit(PR) -> merge into main -> pull main



GIT:

====

Branch: 000-Create-GitRepository-VsSolution-AppArchitecture

Commits: Repo created, Solution created

Pull Request: https://github.com/CorneliusEybers/RockForge/pull/1



Branch: 001-Rock-Management

Commits: Working Basic Solution End-to-End

Pull Request: https://github.com/CorneliusEybers/RockForge/pull/2



Branch: 002-Input-Validation

Commits: Validations Complete

Pull Request: https://github.com/CorneliusEybers/RockForge/pull/3



Branch: 003-Category-Validation-Strategies

Commits: Category Validation Done

Pull Request: https://github.com/CorneliusEybers/RockForge/pull/4



Branch: 004-Global-Error-Handling

Commits: Central Exception handling done with fail-safe 500

Pull Request: https://github.com/CorneliusEybers/RockForge/pull/5



Branch: 005-Structured-Logging-Correlation

Commits: Structured Logging Correlation achieved

Pull Request: https://github.com/CorneliusEybers/RockForge/pull/6



Branch:

Commits:

Pull Request:



Notes and Decisions:

====================

Getting Started:

\----------------

\- My SOP to jump in that I follow every day in my work is the same as

&#x20; what I share in this readme file. Every task on my board I do in

&#x20; the way displayed in this readme.

\- I start every task in its own folder on my local 

&#x20; all task folders are held together in the Workitems folder.

\- Every task starts with a text file like this called Investigate.txt

\- Before every task/story I do a small technical assessment to 

&#x20; investigate technically what is already there and how does the new 

&#x20; requirement fit into the existing system.

\- I also create a SQL file called Investigate.sql where I do the queries

&#x20; to see how the data looks on the database before the changes



Application Creation:

\---------------------

\- I chose the name for the application as RockForge.

&#x20; It is a short easy name to for people to use and remember.

&#x20; A place where commitments are forged, tested, and completed.

\- Create the Repository in GIT.

\- Visual Studio project RockForge.API inside solution RockForge

&#x20; to be scalable we can later add multiple projects

&#x20; as well as the Unit test project.

\- All projects created inside the Solution to facilitate the 

&#x20; Application Architecture



Application Architecture:

\-------------------------

RockForge.API - Exposes the REST API endpoints and handles incoming HTTP requests/responses.

RockForge.Application - Contains application use cases, business workflows, and service orchestration.

RockForge.Domain - Contains the core business entities, enums, rules, and domain behaviour.

RockForge.Infrastructure - Contains external integrations, in-memory persistence, and technical implementations.

RockForge.Tests - Contains automated tests for critical business rules and application behaviour.



Rock Management

\---------------

\- Get the application to work!!

\- Start at the endpoint, create the endpoint and then 

&#x20; create all the components/methods in the subsequent layers.

\- Be careful to stick to the original Architecture decisions made.

\- For storage I used a Dictionary, to keep the dictionary 

&#x20; alive for one run I scoped the service as Singleton in Program.cs

\- All Endpoints working, checked with test data as per attached.



Input Validation:

\-----------------

\- I created one central validator that exists in the Application layer.

\- It throws excpetions for validation errors that bubbles up through 

&#x20; the system and by using the excpetions to also cater for validation'

&#x20; I set everythin up for Central Excpetion handling later

\- No separate message to the user for validation

&#x20; Let it all run via the Exception syste,

\- API to return only the friendly error to the subscriber

&#x20; while the technical(vulnerable) Error data remains internal.

\- Foundation laid for Centralised Exception Handling



Category-Validation:

\--------------------

\- Each category to have its own validation strategy

\- Future category should require adding a new class 

&#x20; and not modifying existing validation logic,

\- Implement Open/Closed Principle

&#x20; \~ For a new RockCategory we will create a new strategy

&#x20;   with it's own interface.

&#x20; \~ Register in Program.cs usinf the same Interface(IRockValidationStrategy)

&#x20;   because the controller request DI to inject using the interface

&#x20;   DI will inject all the services of that interface...

&#x20; \~ New RockCategory will not cause any changes in any existing strategy

&#x20;   but will be contained in it own strategy

&#x20; \~ open for extension, closed for modification.



Global Error Handling:

\----------------------

\- Let all exceptions come through a central place.

\- No try-catches all over the show hiding system errors.

\- Standard exception handling and logging

\- The fallback 500 is the part that completes the requirement.

&#x20; When something genuinely unexpected happens the client must not receive

&#x20; stack trace, assembly names, method names, internal paths, SQL details

&#x20; Instead they receive the standard problem detail

&#x20; {

&#x20; "title": "An unexpected error occurred",

&#x20; "status": 500,

&#x20; "detail": "An unexpected error occurred while processing the request.",

&#x20; "instance": "/members/007CTE001/rocks",

&#x20; "traceId": "..."

&#x20; }

&#x20; while the exception is written in the internal log for support developers to fix.

\- Final Changes to program.cs.

&#x20; 

Structured Logging Correlation:

\-------------------------------

\- Five Logging Rules

&#x20; \~ use incoming X-Correlation-Id(or generate one)

&#x20;   and include it in every log for that request.

&#x20; \~ sensible log levels,

&#x20; \~ emit structured JSON logs

&#x20; \~ log every request outcome including status code and duration

\- I chose to use the built-in Microsoft.Extensions.Logging

\- Everything logged while that HTTP request is being processed 

&#x20; inherits the CorrelationId.

\- Each controller, service, validation strategy and 

&#x20; the global exception handler do not each have to 

&#x20; manually pass the correlation ID around.

\- The Correlation Id can be given to Support engineers

&#x20; and used find all logs belonging to that particular request.

\- Structured JSON logging

&#x20; \~ Correlation ID per request

&#x20; \~ Correlation ID returned to the caller

&#x20; \~ Correlation ID available to every log entry in the request scope

&#x20; \~ Status-code-aware log levels

&#x20; \~ Request duration logging

&#x20; 



