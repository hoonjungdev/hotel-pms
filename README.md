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

Room types carry a base nightly rate for the Core MVP. Reservations snapshot the calculated stay total when they are created so historical reservation amounts do not change if room type pricing changes later.

Reservations are sold by room type first. At check-in, a confirmed reservation can be assigned to a clean physical room of the same room type. At check-out, the assigned room becomes dirty so housekeeping can clean it before the next stay.

Reservation creation serializes availability checks per tenant and room type with a PostgreSQL transaction-level advisory lock. This keeps the "last room" case from accepting two overlapping reservations while still allowing different room types to be booked concurrently.

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
| `GET` | `/api/reservations/availability` | Check reservation availability for a room type and stay period |
| `GET` | `/api/reservations/{reservationId}` | Get one reservation |
| `POST` | `/api/reservations` | Create reservation |
| `POST` | `/api/reservations/{reservationId}/confirm` | Confirm a pending reservation |
| `POST` | `/api/reservations/{reservationId}/cancel` | Cancel a reservation |
| `POST` | `/api/reservations/{reservationId}/check-in` | Check in a confirmed reservation and assign a room |
| `POST` | `/api/reservations/{reservationId}/check-out` | Check out an in-house reservation and mark the room dirty |

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

It creates three room types, four rooms, two guests, and one confirmed Single reservation from `2026-07-10` to `2026-07-12` so reservation listing and availability checks have meaningful demo data immediately.

## Example Requests

Create a room type:

```bash
curl -X POST http://localhost:5216/api/room-types \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -d '{"code":"DBL","name":"Double","baseOccupancy":2,"maxOccupancy":3,"baseNightlyRateAmount":120000,"baseNightlyRateCurrency":"KRW"}'
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

The reservation response includes `totalAmount` and `totalCurrency`, calculated from the room type base nightly rate and stay length.

Confirm a reservation:

```bash
curl -X POST http://localhost:5216/api/reservations/replace-with-reservation-id/confirm \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111"
```

Cancel a reservation:

```bash
curl -X POST http://localhost:5216/api/reservations/replace-with-reservation-id/cancel \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111"
```

Check in a reservation:

```bash
curl -X POST http://localhost:5216/api/reservations/replace-with-reservation-id/check-in \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -d '{"roomId":"replace-with-room-id"}'
```

Check out a reservation:

```bash
curl -X POST http://localhost:5216/api/reservations/replace-with-reservation-id/check-out \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111"
```

More runnable examples are in `docs/api/hotel-pms.http`.

## Verification

The same restore, format, build, and test sequence runs in GitHub Actions for pull requests and pushes to `main`.

```bash
dotnet restore HotelPms.slnx
dotnet format HotelPms.slnx --verify-no-changes --no-restore
dotnet build HotelPms.slnx --no-restore
dotnet test HotelPms.slnx --no-restore --no-build
```
