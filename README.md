# hotel-pms

hotel-pms is a backend API portfolio project for small guesthouses and pensions with roughly 5 to 20 rooms.

The current goal is a **Backend API Core MVP**. The project intentionally focuses on domain modeling, API contracts, PostgreSQL persistence, and integration tests instead of a frontend UI.

## Tech Stack

- .NET 10 + ASP.NET Core Backend API
- EF Core 10 + PostgreSQL 17
- FluentValidation
- Serilog + Seq
- OpenAPI
- xUnit, Testcontainers for .NET, FluentAssertions, Bogus
- Docker Compose for local PostgreSQL and Seq

## Architecture

The application uses a single ASP.NET Core project with use-case vertical slice folders:

```text
src/HotelPms/Features/{FeatureName}/
  {UseCaseName}/   Endpoint, request/response DTOs, command/query, handler, validator
  Domain/          Rich domain model, value objects, strongly typed IDs
  Infrastructure/  EF Core mapping and persistence details
  EventHandlers/   Domain event handlers, introduced only when needed
  {SharedDto}.cs    Small response DTO shared by multiple use cases, only when needed
  {FeatureName}Endpoints.cs
```

`Domain`, `Infrastructure`, and `EventHandlers` stay at the feature root because they are shared by multiple use cases. API contracts and orchestration code stay inside the use-case folder that owns them. A small response DTO can stay at the feature root only when multiple use cases share the same API representation.

Features are split by independent aggregate boundaries. For example, `Rooms` and `RoomTypes` are separate features because a physical room and a sellable room type have different lifecycles and invariants.

Handlers return application result/query models such as `CreateRoomTypeResult`, `GuestDetails`, or `RoomListItem`. Endpoints map those models to HTTP status codes and API response DTOs.

The project intentionally avoids Repository, AutoMapper/Mapster, MediatR, full CQRS, and frontend-first implementation until they solve a concrete problem.

## Current API Surface

All tenant-scoped endpoints currently require the `X-Tenant-Id` header.

| Method | Path | Purpose |
| --- | --- | --- |
| `GET` | `/` | API metadata |
| `GET` | `/openapi/v1.json` | OpenAPI document |
| `GET` | `/api/guests` | List guests |
| `GET` | `/api/guests/{guestId}` | Get one guest |
| `POST` | `/api/guests` | Register guest |
| `GET` | `/api/rooms` | List rooms |
| `GET` | `/api/rooms/{roomId}` | Get one room |
| `POST` | `/api/rooms` | Add physical room for a room type |
| `PATCH` | `/api/rooms/{roomId}/condition` | Update room condition |
| `GET` | `/api/room-types` | List room types |
| `GET` | `/api/room-types/{roomTypeId}` | Get one room type |
| `POST` | `/api/room-types` | Create room type |
| `GET` | `/api/reservations` | List reservations |
| `GET` | `/api/reservations/{reservationId}` | Get one reservation |
| `POST` | `/api/reservations` | Create reservation |

## Local Development

Start PostgreSQL and Seq:

```bash
docker compose up -d
```

Apply EF Core migrations:

```bash
dotnet ef database update --project src/HotelPms/HotelPms.csproj
```

Run the API:

```bash
dotnet run --project src/HotelPms/HotelPms.csproj
```

The default HTTP launch URL is `http://localhost:5216`.

OpenAPI is available at:

```text
http://localhost:5216/openapi/v1.json
```

Optional demo seed data can be inserted when the API starts:

```bash
dotnet run --project src/HotelPms/HotelPms.csproj -- --seed-demo-data
```

The seed is idempotent and uses this tenant ID:

```text
11111111-1111-1111-1111-111111111111
```

## Example Requests

Create a room type:

```bash
curl -X POST http://localhost:5216/api/room-types \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -d '{"code":"DBL","name":"Double","baseOccupancy":2,"maxOccupancy":3}'
```

Add a physical room for a room type:

```bash
curl -X POST http://localhost:5216/api/rooms \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -d '{"roomTypeId":"replace-with-room-type-id","number":"101"}'
```

Register a guest:

```bash
curl -X POST http://localhost:5216/api/guests \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -d '{"name":"Jane Doe","email":"jane@example.com","phoneNumber":"+821012345678"}'
```

Create a reservation:

```bash
curl -X POST http://localhost:5216/api/reservations \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -d '{"primaryGuestId":"replace-with-guest-id","roomTypeId":"replace-with-room-type-id","checkInDate":"2026-07-01","checkOutDate":"2026-07-03","guestCount":2}'
```

More runnable examples are in `docs/api/hotel-pms.http`.

## Verification

```bash
dotnet restore HotelPms.slnx
dotnet format --verify-no-changes
dotnet build HotelPms.slnx --no-restore
dotnet test HotelPms.slnx --no-restore --no-build
```
