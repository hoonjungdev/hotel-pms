# 0001. Use Single-Project Vertical Slices

## Status

Accepted

## Context

hotel-pms is a backend-focused portfolio and learning project for a small guesthouse or pension PMS. The current goal is to complete a Backend API Core MVP with clear domain modeling, OpenAPI documentation, integration tests, demo seed data, and runnable HTTP scenarios.

.NET backend projects often adopt multi-project Clean Architecture, MediatR, repository abstractions, and strict layer packages. Those patterns can be useful in larger systems, but they add ceremony and indirection before this project has enough domain pressure to justify them.

The project still needs domain boundaries, persistence boundaries, and testable use cases. The question is whether to enforce those boundaries through multiple projects and framework abstractions, or through a single ASP.NET Core project with explicit folder and dependency rules.

## Decision

Use one ASP.NET Core backend project organized by use-case vertical slices.

Feature folders represent domain areas or aggregate groups, such as `Guests`, `RoomTypes`, `Rooms`, `Reservations`, `Housekeeping`, `Pricing`, and `Identity`.

Each feature can contain:

- use-case folders for endpoint, request/response contract, command/query, validator, handler, and result/query models
- `Domain` for pure domain models and rules
- `Infrastructure` for EF Core mapping, converters, and persistence helpers
- `EventHandlers` for feature-level domain event handling when needed
- a feature endpoint registration file such as `ReservationEndpoints.cs`

Endpoints handle HTTP mapping only. Handlers orchestrate validation, data loading, domain calls, and persistence. Domain models protect business invariants and stay free of ASP.NET Core, EF Core attributes, JSON serialization, and API DTOs.

Do not introduce MediatR, AutoMapper, a repository layer, full CQRS, event sourcing, or multi-project bounded contexts unless a later use case creates concrete pressure for that change.

## Consequences

Positive consequences:

- Related code for one user action stays close together and easy to review.
- The project remains small enough to finish while still preserving domain boundaries.
- New contributors and agents can locate feature behavior without jumping across several projects.
- Integration tests can exercise real handlers, EF Core mappings, and HTTP endpoints without heavy mocking.

Tradeoffs:

- Boundaries are enforced by conventions and review rather than project references.
- It is possible to accidentally couple features if the folder rules are ignored.
- A future move to multiple projects would require deliberate extraction work.
- Some readers may expect more traditional Clean Architecture structure in a .NET portfolio.

## Alternatives Considered

### Multi-project Clean Architecture

Rejected for now. It provides stronger physical boundaries, but it would add ceremony before the MVP needs it. The current priority is completing a coherent backend slice while keeping domain rules visible and testable.

### MediatR-based vertical slices

Rejected for now. MediatR can standardize request dispatch, but plain handler classes are easier to read, debug, and teach in this project. The current code does not need pipeline behaviors enough to justify the dependency.

### Repository pattern over EF Core

Rejected for now. EF Core already provides unit-of-work and query composition. Feature-specific persistence helpers are enough when a query needs a name or reuse point.

### Full CQRS

Rejected for now. Commands and queries are named separately where useful, but read/write models are not split into separate persistence models. The project can revisit this only if query complexity or reporting needs create real pressure.
