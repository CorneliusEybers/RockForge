# RockForge

A place where commitments are forged, tested, and completed.

Technical Assessment Doctorly VirginActive
TechAssessVirginActive

# Nuget Packages Installed:

Swashbuckle.AspNetCore for Swagger...

# Scrum Tasks:

000-Create-GitRepository-VsSolution-AppArchitecture
001-Rock-Management
002-Input-Validation
003-Category-Validation-Strategies
004-Global-Error-Handling
005-Structured-Logging-Correlation
006-Api-Key-Authentication
007-Profile-Integration-Resilience
008-Tests-and-Readme

# Scrum Task Cycle:

main -> branch -> implement -> test -> commit(PR) -> merge into main -> pull main



NB! NB! NB! 
- Please also view the Test Data.txt file for TestEvidence

\- For normal work tasks I create screenshots and place then in the story

NB! NB! NB! 



# GIT:

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

Branch:
Commits:
Pull Request:

Branch:
Commits:
Pull Request:

# Notes and Decisions:

## Getting Started:

* My SOP to jump in that I follow every day in my work is the same as
what I share in this readme file. Every task on my board I do in
the way displayed in this readme.
* I start every task in its own folder on my local
all task folders are held together in the Workitems folder.
* Every task starts with a text file like this called Investigate.txt
* Before every task/story I do a small technical assessment to
investigate technically what is already there and how does the new
requirement fit into the existing system.
* I also create a SQL file called Investigate.sql where I do the queries
to see how the data looks on the database before the changes

## Application Creation:

* I chose the name for the application as RockForge.
It is a short easy name to for people to use and remember.
A place where commitments are forged, tested, and completed.
* Create the Repository in GIT.
* Visual Studio project RockForge.API inside solution RockForge
to be scalable we can later add multiple projects
as well as the Unit test project.
* All projects created inside the Solution to facilitate the
Application Architecture

## Application Architecture:

RockForge.API - Exposes the REST API endpoints and handles incoming HTTP requests/responses.
RockForge.Application - Contains application use cases, business workflows, and service orchestration.
RockForge.Domain - Contains the core business entities, enums, rules, and domain behaviour.
RockForge.Infrastructure - Contains external integrations, in-memory persistence, and technical implementations.
RockForge.Tests - Contains automated tests for critical business rules and application behaviour.

## Rock Management

* Get the application to work!!
* Start at the endpoint, create the endpoint and then
create all the components/methods in the subsequent layers.
* Be careful to stick to the original Architecture decisions made.
* For storage I used a Dictionary, to keep the dictionary
alive for one run I scoped the service as Singleton in Program.cs
* All Endpoints working, checked with test data as per attached.

## Input Validation:

* I created one central validator that exists in the Application layer.
* It throws excpetions for validation errors that bubbles up through
the system and by using the excpetions to also cater for validation'
I set everythin up for Central Excpetion handling later
* No separate message to the user for validation
Let it all run via the Exception syste,
* API to return only the friendly error to the subscriber
while the technical(vulnerable) Error data remains internal.
* Foundation laid for Centralised Exception Handling

## Category-Validation:

* Each category to have its own validation strategy
* Future category should require adding a new class
and not modifying existing validation logic,
* Implement Open/Closed Principle
\~ For a new RockCategory we will create a new strategy
with it's own interface.
\~ Register in Program.cs usinf the same Interface(IRockValidationStrategy)
because the controller request DI to inject using the interface
DI will inject all the services of that interface...
\~ New RockCategory will not cause any changes in any existing strategy
but will be contained in it own strategy
\~ open for extension, closed for modification.

## Global Error Handling:

* Let all exceptions come through a central place.
* No try-catches all over the show hiding system errors.
* Standard exception handling and logging
* The fallback 500 is the part that completes the requirement.
When something genuinely unexpected happens the client must not receive
stack trace, assembly names, method names, internal paths, SQL details
Instead they receive the standard problem detail
{
"title": "An unexpected error occurred",
"status": 500,
"detail": "An unexpected error occurred while processing the request.",
"instance": "/members/007CTE001/rocks",
"traceId": "..."
}
while the exception is written in the internal log for support developers to fix.
* Final Changes to program.cs.

