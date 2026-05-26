# Initial Project Setup — Design Spec

## Goal

빌드 성공 + Docker Compose 기동 가능한 프로젝트 뼈대.
도메인 코드 없이 구조와 의존성만 잡는다.

## Solution Structure

```
HotelPms.sln
├── src/HotelPms/                      Blazor Interactive Server (.NET 10)
├── tests/HotelPms.UnitTests/          xUnit
└── tests/HotelPms.IntegrationTests/   xUnit + TestContainers
```

## NuGet Packages

### src/HotelPms

| Category | Package |
|----------|---------|
| ORM | Microsoft.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL |
| Auth | Microsoft.AspNetCore.Identity.EntityFrameworkCore |
| Background | Hangfire.Core, Hangfire.PostgreSql |
| Validation | FluentValidation, FluentValidation.DependencyInjectionExtensions |
| Logging | Serilog.AspNetCore, Serilog.Sinks.Seq |
| UI | MudBlazor |
| Observability | OpenTelemetry.Extensions.Hosting, OpenTelemetry.Exporter.Console |

### tests/HotelPms.UnitTests

xUnit, FluentAssertions, Bogus, bUnit

### tests/HotelPms.IntegrationTests

xUnit, FluentAssertions, Bogus, Testcontainers.PostgreSql, Microsoft.AspNetCore.Mvc.Testing

## Folder Structure (src/HotelPms)

Features 폴더에 8개 Feature 슬라이스, 각각 Domain/Application/Infrastructure/Pages 하위 폴더.
Shared 폴더에 Domain/Components/MultiTenancy.
Infrastructure 폴더에 Database/BackgroundJobs/Authorization.
빈 폴더는 `.gitkeep`으로 유지.

## Docker Compose

- PostgreSQL 17: port 5432, db=hotelpms, user=hotelpms, pw=hotelpms_dev
- Seq: port 5341(ingest)/8081(ui)

## Config Files

- `Program.cs`: EF Core, Identity, MudBlazor, Serilog DI 등록
- `appsettings.json` / `appsettings.Development.json`
- `Dockerfile` (multi-stage build)
- `docker-compose.yml`
- `.gitignore` (.NET template)

## Success Criteria

1. `dotnet build` 성공 (warning 0)
2. `dotnet test` 실행 가능 (테스트 0개, 실패 0개)
3. `docker compose up -d` 로 PostgreSQL + Seq 기동
