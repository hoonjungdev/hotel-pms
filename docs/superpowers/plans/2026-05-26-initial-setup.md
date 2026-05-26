# Initial Project Setup — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `dotnet build` 성공 + `docker compose up`으로 PostgreSQL/Seq 기동 가능한 프로젝트 뼈대 완성

**Architecture:** 단일 Blazor Interactive Server 프로젝트 + xUnit 테스트 프로젝트 2개. Vertical Slice 폴더 규약. EF Core + PostgreSQL + Identity + MudBlazor + Serilog 기본 DI 등록.

**Tech Stack:** .NET 10, Blazor Server, EF Core 10, PostgreSQL 17, MudBlazor, Serilog, Docker Compose

---

## File Map

### Task 1 — Solution + Blazor 프로젝트 생성
- Create: `HotelPms.sln`
- Create: `src/HotelPms/HotelPms.csproj`
- Create: `src/HotelPms/Program.cs`
- Create: `src/HotelPms/Components/App.razor`
- Create: `src/HotelPms/Components/Routes.razor`
- Create: `src/HotelPms/Components/_Imports.razor`
- (dotnet new blazor 템플릿이 자동 생성)

### Task 2 — 테스트 프로젝트 생성
- Create: `tests/HotelPms.UnitTests/HotelPms.UnitTests.csproj`
- Create: `tests/HotelPms.IntegrationTests/HotelPms.IntegrationTests.csproj`

### Task 3 — NuGet 패키지 추가
- Modify: `src/HotelPms/HotelPms.csproj`
- Modify: `tests/HotelPms.UnitTests/HotelPms.UnitTests.csproj`
- Modify: `tests/HotelPms.IntegrationTests/HotelPms.IntegrationTests.csproj`

### Task 4 — Feature 폴더 구조 생성
- Create: `src/HotelPms/Features/` 하위 8개 Feature 폴더 + 서브폴더
- Create: `src/HotelPms/Shared/` 하위 폴더
- Create: `src/HotelPms/Infrastructure/` 하위 폴더
- Create: `tests/HotelPms.UnitTests/Features/` 미러 구조
- Create: `tests/HotelPms.IntegrationTests/Features/` 미러 구조

### Task 5 — DbContext 빈 껍데기 + Program.cs DI 설정
- Create: `src/HotelPms/Infrastructure/Database/HotelDbContext.cs`
- Modify: `src/HotelPms/Program.cs`
- Modify: `src/HotelPms/Components/App.razor`
- Modify: `src/HotelPms/Components/Routes.razor`
- Modify: `src/HotelPms/Components/_Imports.razor`
- Create: `src/HotelPms/Components/Layout/MainLayout.razor`
- Modify: `src/HotelPms/appsettings.json`
- Create: `src/HotelPms/appsettings.Development.json`

### Task 6 — Docker Compose + Dockerfile
- Create: `docker-compose.yml`
- Create: `Dockerfile`

### Task 7 — git init + .gitignore + 빌드 검증 + 첫 커밋

---

## Task 1: Solution + Blazor 프로젝트 생성

**Files:** dotnet new 템플릿이 자동 생성

- [ ] **Step 1: git init**

```bash
cd /Users/muho/hoonjungdev/hj-pms
git init
```

- [ ] **Step 2: .gitignore 생성**

```bash
dotnet new gitignore
```

- [ ] **Step 3: 솔루션 생성**

```bash
dotnet new sln --name HotelPms
```

Expected: `HotelPms.sln` 파일 생성

- [ ] **Step 4: Blazor 프로젝트 생성**

```bash
dotnet new blazor --interactivity Server --all-interactive --empty --no-restore --name HotelPms -o src/HotelPms
```

Expected: `src/HotelPms/` 폴더에 Blazor 프로젝트 파일들 생성

- [ ] **Step 5: 솔루션에 프로젝트 추가**

```bash
dotnet sln add src/HotelPms/HotelPms.csproj
```

- [ ] **Step 6: 빌드 확인**

```bash
dotnet build
```

Expected: Build succeeded. 0 Warning(s). 0 Error(s).

---

## Task 2: 테스트 프로젝트 생성

**Files:**
- Create: `tests/HotelPms.UnitTests/HotelPms.UnitTests.csproj`
- Create: `tests/HotelPms.IntegrationTests/HotelPms.IntegrationTests.csproj`

- [ ] **Step 1: UnitTests 프로젝트 생성**

```bash
dotnet new xunit --no-restore --name HotelPms.UnitTests -o tests/HotelPms.UnitTests
```

- [ ] **Step 2: IntegrationTests 프로젝트 생성**

```bash
dotnet new xunit --no-restore --name HotelPms.IntegrationTests -o tests/HotelPms.IntegrationTests
```

- [ ] **Step 3: 솔루션에 추가**

```bash
dotnet sln add tests/HotelPms.UnitTests/HotelPms.UnitTests.csproj
dotnet sln add tests/HotelPms.IntegrationTests/HotelPms.IntegrationTests.csproj
```

- [ ] **Step 4: 테스트 프로젝트에서 메인 프로젝트 참조 추가**

```bash
dotnet add tests/HotelPms.UnitTests/HotelPms.UnitTests.csproj reference src/HotelPms/HotelPms.csproj
dotnet add tests/HotelPms.IntegrationTests/HotelPms.IntegrationTests.csproj reference src/HotelPms/HotelPms.csproj
```

- [ ] **Step 5: 기본 생성된 테스트 파일 삭제**

```bash
rm tests/HotelPms.UnitTests/UnitTest1.cs
rm tests/HotelPms.IntegrationTests/UnitTest1.cs
```

- [ ] **Step 6: 빌드 확인**

```bash
dotnet build
```

Expected: Build succeeded. 0 Warning(s). 0 Error(s).

---

## Task 3: NuGet 패키지 추가

**Files:**
- Modify: `src/HotelPms/HotelPms.csproj`
- Modify: `tests/HotelPms.UnitTests/HotelPms.UnitTests.csproj`
- Modify: `tests/HotelPms.IntegrationTests/HotelPms.IntegrationTests.csproj`

- [ ] **Step 1: 메인 프로젝트 패키지 추가**

```bash
cd /Users/muho/hoonjungdev/hj-pms

# EF Core + PostgreSQL
dotnet add src/HotelPms package Microsoft.EntityFrameworkCore
dotnet add src/HotelPms package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/HotelPms package Microsoft.EntityFrameworkCore.Tools

# Identity
dotnet add src/HotelPms package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# Hangfire
dotnet add src/HotelPms package Hangfire.Core
dotnet add src/HotelPms package Hangfire.PostgreSql

# Validation
dotnet add src/HotelPms package FluentValidation
dotnet add src/HotelPms package FluentValidation.DependencyInjectionExtensions

# Logging
dotnet add src/HotelPms package Serilog.AspNetCore
dotnet add src/HotelPms package Serilog.Sinks.Seq

# UI
dotnet add src/HotelPms package MudBlazor

# Observability
dotnet add src/HotelPms package OpenTelemetry.Extensions.Hosting
dotnet add src/HotelPms package OpenTelemetry.Exporter.Console
```

- [ ] **Step 2: UnitTests 패키지 추가**

```bash
dotnet add tests/HotelPms.UnitTests package FluentAssertions
dotnet add tests/HotelPms.UnitTests package Bogus
dotnet add tests/HotelPms.UnitTests package bunit
```

- [ ] **Step 3: IntegrationTests 패키지 추가**

```bash
dotnet add tests/HotelPms.IntegrationTests package FluentAssertions
dotnet add tests/HotelPms.IntegrationTests package Bogus
dotnet add tests/HotelPms.IntegrationTests package Testcontainers.PostgreSql
dotnet add tests/HotelPms.IntegrationTests package Microsoft.AspNetCore.Mvc.Testing
```

- [ ] **Step 4: 복원 + 빌드 확인**

```bash
dotnet restore
dotnet build
```

Expected: Build succeeded. 0 Warning(s). 0 Error(s).

---

## Task 4: Feature 폴더 구조 생성

**Files:** 빈 폴더 + `.gitkeep`

- [ ] **Step 1: Feature 폴더 생성**

각 Feature에 동일한 서브폴더 구조 생성. 모든 빈 폴더에 `.gitkeep` 배치.

```bash
cd /Users/muho/hoonjungdev/hj-pms

# Feature 목록과 서브폴더
FEATURES="Reservations Rooms Pricing Guests Payments Housekeeping Dashboard Identity"
SUBDIRS="Pages Components Domain Application Infrastructure EventHandlers"

for f in $FEATURES; do
  for s in $SUBDIRS; do
    mkdir -p "src/HotelPms/Features/$f/$s"
    touch "src/HotelPms/Features/$f/$s/.gitkeep"
  done
done

# Reservations/Domain/Events, Reservations/Domain/Exceptions
mkdir -p src/HotelPms/Features/Reservations/Domain/Events
touch src/HotelPms/Features/Reservations/Domain/Events/.gitkeep
mkdir -p src/HotelPms/Features/Reservations/Domain/Exceptions
touch src/HotelPms/Features/Reservations/Domain/Exceptions/.gitkeep
mkdir -p src/HotelPms/Features/Reservations/Application/Validators
touch src/HotelPms/Features/Reservations/Application/Validators/.gitkeep
```

- [ ] **Step 2: Shared 폴더 생성**

```bash
# Shared 구조
mkdir -p src/HotelPms/Shared/Domain/ValueObjects
touch src/HotelPms/Shared/Domain/.gitkeep
touch src/HotelPms/Shared/Domain/ValueObjects/.gitkeep

mkdir -p src/HotelPms/Shared/Components/Layout
mkdir -p src/HotelPms/Shared/Components/UI
touch src/HotelPms/Shared/Components/UI/.gitkeep

mkdir -p src/HotelPms/Shared/MultiTenancy
touch src/HotelPms/Shared/MultiTenancy/.gitkeep
```

- [ ] **Step 3: Infrastructure 폴더 생성**

```bash
mkdir -p src/HotelPms/Infrastructure/Database/Migrations
touch src/HotelPms/Infrastructure/Database/Migrations/.gitkeep
mkdir -p src/HotelPms/Infrastructure/BackgroundJobs
touch src/HotelPms/Infrastructure/BackgroundJobs/.gitkeep
mkdir -p src/HotelPms/Infrastructure/Authorization
touch src/HotelPms/Infrastructure/Authorization/.gitkeep
```

- [ ] **Step 4: 테스트 폴더 미러 구조 생성**

```bash
# UnitTests 미러
for f in Reservations Rooms Pricing Guests Payments Housekeeping; do
  mkdir -p "tests/HotelPms.UnitTests/Features/$f/Domain"
  touch "tests/HotelPms.UnitTests/Features/$f/Domain/.gitkeep"
done
mkdir -p tests/HotelPms.UnitTests/Features/Shared/Domain
touch tests/HotelPms.UnitTests/Features/Shared/Domain/.gitkeep

# IntegrationTests 미러
for f in Reservations Rooms Pricing Guests Payments Housekeeping; do
  mkdir -p "tests/HotelPms.IntegrationTests/Features/$f"
  touch "tests/HotelPms.IntegrationTests/Features/$f/.gitkeep"
done
```

---

## Task 5: DbContext + Program.cs DI 설정 + Blazor 구성

**Files:**
- Create: `src/HotelPms/Infrastructure/Database/HotelDbContext.cs`
- Modify: `src/HotelPms/Program.cs`
- Modify: `src/HotelPms/Components/App.razor`
- Modify: `src/HotelPms/Components/Routes.razor`
- Modify: `src/HotelPms/Components/_Imports.razor`
- Create: `src/HotelPms/Components/Layout/MainLayout.razor`
- Modify: `src/HotelPms/appsettings.json`
- Create: `src/HotelPms/appsettings.Development.json`

- [ ] **Step 1: HotelDbContext 생성**

`src/HotelPms/Infrastructure/Database/HotelDbContext.cs`:

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Infrastructure.Database;

public class HotelDbContext : IdentityDbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
```

- [ ] **Step 2: appsettings.json 수정**

`src/HotelPms/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hotelpms;Username=hotelpms;Password=hotelpms_dev"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Seq",
        "Args": { "serverUrl": "http://localhost:5341" }
      }
    ]
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 3: appsettings.Development.json 생성**

`src/HotelPms/appsettings.Development.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.AspNetCore": "Information",
        "Microsoft.EntityFrameworkCore": "Information"
      }
    }
  }
}
```

- [ ] **Step 4: Program.cs 재작성**

`src/HotelPms/Program.cs`:

```csharp
using HotelPms.Components;
using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddDbContext<HotelDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddMudServices();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseAntiforgery();
    app.UseSerilogRequestLogging();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

- [ ] **Step 5: App.razor에 MudBlazor 프로바이더 추가**

`src/HotelPms/Components/App.razor`:

```razor
<!DOCTYPE html>
<html lang="ko">

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <ResourcePreloader />
    <link rel="stylesheet" href="@Assets["app.css"]" />
    <link rel="stylesheet" href="@Assets["HotelPms.styles.css"]" />
    <link href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap" rel="stylesheet" />
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
    <ImportMap />
    <HeadOutlet @rendermode="InteractiveServer" />
</head>

<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
    <script src="@Assets["_framework/blazor.web.js"]"></script>
</body>

</html>
```

- [ ] **Step 6: MainLayout.razor에 MudBlazor 레이아웃 적용**

기존 템플릿의 `src/HotelPms/Components/Layout/MainLayout.razor`를 이동하여 `src/HotelPms/Shared/Components/Layout/MainLayout.razor`에 작성:

```razor
@inherits LayoutComponentBase

<MudThemeProvider />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />

<MudLayout>
    <MudAppBar Elevation="1">
        <MudText Typo="Typo.h5">HotelPms</MudText>
    </MudAppBar>
    <MudMainContent>
        <MudContainer MaxWidth="MaxWidth.Large" Class="my-4">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>
```

기존 `src/HotelPms/Components/Layout/` 폴더의 파일들은 삭제.

- [ ] **Step 7: _Imports.razor 업데이트**

`src/HotelPms/Components/_Imports.razor`:

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using MudBlazor
@using HotelPms
@using HotelPms.Components
@using HotelPms.Shared.Components.Layout
```

- [ ] **Step 8: Routes.razor 업데이트**

`src/HotelPms/Components/Routes.razor` (.NET 10은 `NotFoundPage` 속성 사용):

```razor
<Router AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

`src/HotelPms/Components/Pages/NotFound.razor` (유지):

```razor
@page "/not-found"

<PageTitle>Not found</PageTitle>

<MudText Typo="Typo.h6">페이지를 찾을 수 없습니다.</MudText>
```

- [ ] **Step 9: Home.razor를 Features/Dashboard로 이동**

기존 `src/HotelPms/Components/Pages/Home.razor` 삭제.
`src/HotelPms/Features/Dashboard/Pages/DashboardHome.razor` 생성:

```razor
@page "/"

<PageTitle>HotelPms - Dashboard</PageTitle>

<MudText Typo="Typo.h4" Class="mb-4">Dashboard</MudText>
<MudText>HotelPms 프로젝트가 정상 구동 중입니다.</MudText>
```

기존 `src/HotelPms/Components/Pages/Error.razor` 삭제. `NotFound.razor`는 유지 (Step 8에서 수정).

- [ ] **Step 10: 빌드 확인**

```bash
dotnet build
```

Expected: Build succeeded. 0 Warning(s). 0 Error(s).

---

## Task 6: Docker Compose + Dockerfile

**Files:**
- Create: `docker-compose.yml`
- Create: `Dockerfile`

- [ ] **Step 1: docker-compose.yml 생성**

`docker-compose.yml` (프로젝트 루트):

```yaml
services:
  postgres:
    image: postgres:17
    environment:
      POSTGRES_DB: hotelpms
      POSTGRES_USER: hotelpms
      POSTGRES_PASSWORD: hotelpms_dev
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  seq:
    image: datalust/seq:latest
    environment:
      ACCEPT_EULA: "Y"
    ports:
      - "5341:5341"
      - "8081:80"
    volumes:
      - seqdata:/data

volumes:
  pgdata:
  seqdata:
```

- [ ] **Step 2: Dockerfile 생성**

`Dockerfile` (프로젝트 루트):

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/HotelPms/HotelPms.csproj", "src/HotelPms/"]
RUN dotnet restore "src/HotelPms/HotelPms.csproj"
COPY . .
WORKDIR "/src/src/HotelPms"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HotelPms.dll"]
```

- [ ] **Step 3: Docker Compose 기동 확인**

```bash
docker compose up -d
docker compose ps
```

Expected: postgres (healthy), seq (healthy) 2개 컨테이너 실행

- [ ] **Step 4: Docker Compose 중지**

```bash
docker compose down
```

---

## Task 7: 최종 빌드 검증 + 첫 커밋

- [ ] **Step 1: 전체 빌드 확인**

```bash
dotnet build
```

Expected: Build succeeded. 0 Warning(s). 0 Error(s).

- [ ] **Step 2: 테스트 실행 확인**

```bash
dotnet test
```

Expected: 테스트 0개, 실패 0개 (빈 테스트 프로젝트)

- [ ] **Step 3: 커밋**

```bash
git add -A
git commit -m "chore: initial project setup

- Blazor Interactive Server (.NET 10) + solution structure
- NuGet packages: EF Core, Identity, MudBlazor, Serilog, Hangfire, FluentValidation, OpenTelemetry
- Test projects: xUnit + FluentAssertions + Bogus + TestContainers + bUnit
- Docker Compose: PostgreSQL 17 + Seq
- Vertical Slice folder structure with 8 feature slices
- Basic Program.cs DI configuration + HotelDbContext shell"
```
