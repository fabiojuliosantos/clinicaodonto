# Architecture

## Architecture Style

The project follows **Clean Architecture** with **Domain-Driven Design (DDD)** principles.

The architecture is organized to keep business rules independent from external technologies, allowing the system to evolve with minimal impact across layers.

---

## Solution Structure

```text
Odonto.API
Odonto.Application
Odonto.Domain
Odonto.Infrastructure
Odonto.Tests
```

---

## Layers

### API

**Responsibilities**

- Expose HTTP endpoints.
- Receive and validate HTTP requests.
- Handle authentication and authorization.
- Delegate execution to the Application layer.
- Return HTTP responses.

The API layer contains no business rules.

---

### Application

**Responsibilities**

- Coordinate application use cases.
- Orchestrate domain operations.
- Manage application flow.
- Communicate with repository contracts defined in the Domain.

The Application layer should orchestrate business operations rather than implement business rules whenever possible.

---

### Domain

**Responsibilities**

- Entities.
- Value Objects.
- Domain Services.
- Business Rules.
- Repository Contracts (Interfaces).
- Domain Exceptions.

The Domain layer is the core of the system and must remain independent of external technologies.

---

### Infrastructure

**Responsibilities**

- Implement repository contracts.
- Database persistence.
- External integrations.
- Infrastructure services.

Current implementations include reading and writing Domain entities to the database.

---

### Tests

**Responsibilities**

- Validate business behavior.
- Verify application correctness through automated tests.
- Protect against regressions.

---

## Dependency Direction

Dependencies must always point toward the Domain.

```text
API
        ↓
Application
        ↓
Domain
        ↑
Infrastructure

Tests
        ↓
Application / Domain / API (depending on the test type)
```

### Rules

- Domain must not depend on any other project.
- Application depends on Domain.
- Infrastructure depends on Domain.
- API depends on Application.
- Infrastructure provides implementations for Domain contracts.

---

## Request Flow

A typical request follows this flow:

```text
HTTP Request
        ↓
API
        ↓
Application
        ↓
Repository Contract (Domain)
        ↓
Infrastructure
        ↓
Database
        ↑
Infrastructure
        ↑
Application
        ↑
API
        ↑
HTTP Response
```

---

## Architectural Principles

- Business rules belong to the Domain whenever possible.
- The Application layer coordinates use cases.
- Infrastructure implements external concerns.
- API exposes the application to external clients.
- Dependencies always point toward the Domain.
- External technologies should never dictate business rules.
- Prefer composition over unnecessary abstraction.
- Keep layers independent and responsibilities well defined.