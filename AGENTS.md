# hotel-pms: Agent Working Contract

## 응답 언어

사용자에게 응답은 항상 한국어로 제공한다.

## Project Intent

이 프로젝트는 소형 게스트하우스/펜션(객실 5~20개)을 위한 Hotel PMS 백엔드 포트폴리오이자 학습 프로젝트다.

최적화 대상:

- 완성 가능한 Backend API Core MVP
- 명확한 호텔 도메인 모델링
- OpenAPI, 통합 테스트, demo seed data, HTTP scenario 기반 데모 가능성
- 사용자가 리뷰하며 배울 수 있는 production-style 코드
- 해외 포트폴리오 관점에서 자연스러운 영어 식별자, 테스트명, 커밋 메시지

최적화하지 않을 것:

- 프론트엔드 중심 구현
- 트렌디하지만 불필요한 아키텍처
- 미래 가능성만을 위한 추상화
- 도메인 요구가 없는 대규모 인프라 도입

## Working Model

기본 작업 방식은 **Codex-implemented, user-reviewed**다. 사용자가 설명만 원한다고 명시하지 않았다면, Codex는 요청을 완료하기 위해 필요한 backend API, domain, infrastructure, tests, docs 파일을 직접 수정할 수 있다.

먼저 질문해야 하는 경우:

- 요구사항이 여러 도메인 모델로 크게 갈라질 수 있는 경우
- migration, 데이터 삭제, 대규모 파일 이동, public API contract 변경처럼 되돌리기 어렵거나 영향 범위가 큰 경우
- 외부 비용, secret, auth, deployment, security 설정에 영향을 주는 경우
- 사용자가 명시적으로 "코드 작성하지 말고 설명만", "가이드만", "리뷰만"을 요청한 경우

의미 있는 구현 후에는 다음을 수행한다:

- 무엇이 바뀌었는지 설명한다.
- 왜 이 설계가 현재 프로젝트에 맞는지 설명한다.
- 결정이 비자명하면 tradeoff와 대안을 함께 설명한다.
- 가능한 build/test/format 검증을 직접 실행한다.
- 변경 파일 기준으로 correctness, DDD boundary, layering, async/null handling, security, testability, 영어 naming을 self-review한다.

작은 기계적 변경은 간결하게 보고한다. Domain, API contract, persistence, test strategy를 건드린 변경은 reasoning과 verification을 명시한다.

Codex는 작업 단위가 명확하고 검증 가능한 상태라고 판단하면 직접 커밋할 수 있다. 커밋에는 현재 요청 범위의 변경만 포함해야 하며, 관련 없는 사용자 변경을 함께 staging하지 않는다. 변경 범위가 애매하거나 되돌리기 어려운 작업이면 커밋 전에 먼저 확인한다.

## Technology Baseline

현재 기준 기술 스택:

- .NET 10 + ASP.NET Core
- EF Core 10 + PostgreSQL
- FluentValidation
- Serilog + Seq
- OpenAPI
- xUnit, Testcontainers for .NET, FluentAssertions, Bogus
- Docker Compose 기반 local PostgreSQL and Seq

새 framework, infrastructure package, architectural pattern은 현재 use case를 직접 해결할 때만 도입한다.

ASP.NET Core Identity는 `IdentityDbContext` 기반 테이블과 `Features/Identity` 영역으로 존재하지만, full authentication/authorization flow는 사용자가 명시적으로 요청하기 전까지 Core MVP 범위로 확장하지 않는다. Authenticated tenant context가 도입되기 전까지 tenant-scoped API는 `TenantId`를 명시적으로 받는다.

## Architecture Rules

단일 ASP.NET Core backend project 안에서 **use-case vertical slice** 구조를 사용한다. Clean Architecture식 multi-project 분리는 도입하지 않는다.

기본 feature layout:

```text
src/HotelPms/Features/{FeatureName}/
  {UseCaseName}/
    {UseCaseName}Endpoint.cs
    {UseCaseName}Request.cs
    {UseCaseName}Command.cs 또는 {UseCaseName}Query.cs
    {UseCaseName}CommandValidator.cs 또는 {UseCaseName}QueryValidator.cs
    {UseCaseName}Handler.cs
  Domain/
  Infrastructure/
  EventHandlers/
  {SharedDto}.cs
  {FeatureName}Endpoints.cs
```

규칙:

- Feature는 호텔 도메인 영역 또는 aggregate 묶음이다. 예: `Guests`, `RoomTypes`, `Rooms`, `Reservations`, `Housekeeping`, `Pricing`, `Identity`.
- Use-case folder는 하나의 사용자 행동 또는 API capability를 나타낸다. 예: `RegisterGuest`, `ListRooms`, `CreateReservation`, `CheckInReservation`.
- Endpoint는 HTTP mapping만 담당한다: route, binding, response/status mapping.
- Handler는 validation, data loading, domain call, persistence를 orchestration한다.
- Business rule은 endpoint에 두지 않는다. Handler 또는 Domain으로 보낸다.
- Handler는 API response DTO나 `IResult`를 반환하지 않는다. Application result/query model을 반환하고 endpoint가 response DTO로 매핑한다.
- Domain은 ASP.NET Core, EF Core attribute, JSON serialization, API DTO에 의존하지 않는다.
- Infrastructure는 EF Core mapping, converter, persistence helper를 담당한다.
- Feature route group 조립은 feature root의 `Map{FeatureName}Endpoints()` 확장 메서드가 담당한다.
- 특정 use case에서 시작한 개념이라도 둘 이상의 use case가 공유하면 feature root의 `Domain` 또는 `Infrastructure`로 승격한다. 단, 실제 중복이나 명확한 cross-use-case 필요가 생겼을 때만 승격한다.

의도적으로 도입하지 않는 것:

| 도입하지 않음 | 대신 사용 |
| --- | --- |
| Repository pattern | `HotelDbContext` 직접 사용 + feature별 persistence helper |
| AutoMapper / Mapster | 수동 매핑 |
| MediatR | 평범한 Handler class |
| Full CQRS | 가벼운 Command/Query 구분 |
| Event Sourcing | 전통적 CRUD + Domain Event |
| Microservices / bounded context 분리 | 단일 backend project |
| Message Queue / Kafka | 실제 필요가 생길 때 검토 |
| Anemic Domain Model | Rich Domain Model |
| Primitive Obsession | Value Object + strongly typed ID |

## Domain Modeling Rules

도메인 용어는 `CONTEXT.md`를 우선한다. 사용자의 표현이나 코드 이름이 `CONTEXT.md`의 language와 충돌하면 먼저 용어를 확인한다.

규칙:

- Domain model은 스스로 invariant를 보호해야 한다. Business rule을 endpoint나 handler에만 두지 않는다.
- Domain entity는 행위를 캡슐화하고 setter를 가능한 한 private으로 둔다.
- 생성에 invariant가 있으면 factory method를 사용한다.
- Value Object는 의미 있는 도메인 개념에 사용한다. 예: `Money`, `DateRange`, `RoomNumber`.
- Aggregate는 다른 aggregate를 객체 navigation이 아니라 strongly typed ID로 참조한다. 의도적 예외가 필요하면 먼저 이유를 설명한다.
- Aggregate root ID는 `readonly record struct` 기반 strongly typed ID를 사용한다.
- 새 strongly typed ID를 추가하면 EF Core converter와 `ConfigureConventions` 등록을 함께 추가한다.
- Domain Event는 aggregate에서 수집할 수 있지만, dispatch는 실제 use case가 필요할 때 in-process 방식으로 도입한다.

현재 주요 feature/aggregate:

- `Guest`
- `RoomType`
- `Room`
- `Reservation`
- `Housekeeping`
- `Pricing`
- `Identity`

## Reservation Domain Rules

예약 도메인에서 이미 확정된 규칙:

- Reservations are sold against a `RoomType`, not a physical `Room`.
- Physical room assignment happens during check-in.
- Pending and Confirmed reservations consume availability.
- Cancelled reservations do not consume availability.
- Check-in requires a clean assignable room.
- Check-out ends the in-house stay and makes the assigned room require cleaning.
- Reservation totals are captured at creation time and are not live recalculations.
- Pricing currently calculates the reservation total from the room type's base nightly rate and stay period. Do not model advanced rate plans or promotions until a use case requires them.
- Housekeeping is a separate operational feature connected to reservation check-in/check-out through physical room readiness.

## API Contract Rules

Backend API surface는 포트폴리오 산출물이다. Route, request DTO, response DTO, status code, validation error는 public API contract로 취급한다.

규칙:

- Endpoint는 HTTP boundary다. Application result를 response DTO와 status code로 매핑한다.
- Domain entity를 request/response로 직접 노출하지 않는다.
- Request/response DTO는 외부 계약이며 domain model과 분리한다.
- API behavior 변경 시 OpenAPI metadata, integration tests, demo scenario를 함께 확인한다.
- `docs/api/hotel-pms.http`는 canonical local demo scenario file이다. API route, request/response body, seed data, demo flow가 바뀌면 이 파일을 수정하거나 수정이 필요 없는 이유를 설명한다.

## Multi-Tenancy Rules

Tenant data isolation은 중요한 correctness 요구사항이다.

규칙:

- Tenant-scoped entity는 `TenantId`를 가진다.
- Tenant-scoped command/query는 `TenantId`를 명시적으로 받는다.
- Handler는 tenant-scoped read/write에 항상 `TenantId` 조건을 적용한다.
- Authenticated tenant context가 생기기 전까지 EF Core global query filter를 도입하지 않는다.
- Placeholder auth state에서 tenant identity를 추론하지 않는다.

## Persistence Rules

규칙:

- EF Core mapping은 feature `Infrastructure` folder에 둔다.
- Domain model에 EF Core attribute를 붙이지 않는다.
- Persisted strongly typed ID 또는 value object를 추가하면 converter/configuration을 의도적으로 추가한다.
- Generated migration과 `HotelDbContextModelSnapshot`에서 의도하지 않은 table/column 변경이 없는지 확인한다.
- Demo seed data는 `src/HotelPms/Infrastructure/Database/Seed/`에 둔다.
- Demo seed data는 migration `HasData`에 넣지 않는다.
- Demo seed data는 domain factory와 value object를 통과해 생성하고 idempotent해야 한다.
- 기본 실행에는 seed를 자동 적용하지 않는다. 로컬 데모가 필요할 때만 `--seed-demo-data` 옵션을 사용한다.

Demo seed tenant ID:

```text
11111111-1111-1111-1111-111111111111
```

## Testing Rules

테스트 전략은 테스트 트로피를 따른다. 이 프로젝트에서는 handler, EF Core query, HTTP endpoint behavior에 대한 integration test의 가치가 크다.

규칙:

- Value Object, domain entity, pure domain service, state transition은 unit test로 검증한다.
- Handler, EF Core query, HTTP endpoint behavior는 integration test를 선호한다.
- End-to-end flow test는 핵심 demo flow에 한해 소수만 둔다.
- Mock은 payment provider, email, OTA API, clock 같은 외부 시스템에만 사용한다.
- `HotelDbContext` 또는 내부 service를 mock하지 않는다.
- Use-case 변경마다 가장 좁지만 회귀를 막을 수 있는 테스트를 추가하거나 갱신한다.

테스트 위치:

```text
tests/HotelPms.UnitTests/Features/{FeatureName}/Domain/
tests/HotelPms.UnitTests/Features/{FeatureName}/{UseCaseName}/
tests/HotelPms.IntegrationTests/Features/{FeatureName}/
tests/HotelPms.IntegrationTests/Features/{FeatureName}/{UseCaseName}/
```

## Verification

기본 검증 후보:

```bash
dotnet build HotelPms.slnx --no-restore
dotnet test HotelPms.slnx --no-restore --no-build
dotnet format HotelPms.slnx --verify-no-changes
```

추가 검증:

- EF mapping 또는 migration 변경 시 generated migration과 model snapshot을 확인한다.
- API contract 변경 시 integration tests와 `docs/api/hotel-pms.http`를 확인한다.
- Demo behavior 변경 시 demo seed data와 HTTP scenario를 확인한다.
- Build/test 명령에서 약 30초 동안 새 출력이 없으면 무한 대기하지 말고 중단한 뒤 상황을 보고한다.
- Source를 실제로 수정하는 `dotnet format`은 사용자가 요청했거나 Codex가 직접 구현한 변경의 정리 범위가 명확할 때만 실행한다.

## Review Rules

리뷰 요청을 받으면 기본적으로 변경 파일 기준 diff-first review를 한다. Findings를 먼저 제시하고, 요약은 뒤에 둔다.

우선순위:

- correctness bug
- DDD boundary 위반
- tenant isolation 누락
- API contract regression
- persistence/migration risk
- async/null handling
- security issue
- missing or weak tests
- 어색한 영어 naming

문제가 있으면 정확한 위치, 왜 문제인지, 수정 방향을 설명한다. 필요한 경우 직접 수정한다.

## Naming Rules

해외 포트폴리오 프로젝트이므로 자연스러운 영어 식별자를 중요하게 본다.

규칙:

- Test name은 `Method_Scenario_ExpectedBehavior` 형식을 사용한다.
- 모호한 단어를 피한다: `Correctly`, `Properly`, `Works`, `ReturnsCorrectString`.
- Test target은 이름 맨 앞에 둔다. 예: `ToString_Krw_ReturnsWonFormat`.
- Operator overload operand는 .NET 관용대로 `left`/`right`를 사용한다.
- 약어보다 의도가 드러나는 이름을 사용한다. 예: `culture` over `ci`.
- 어색한 영어는 더 관용적인 표현과 이유를 함께 제안한다.

좋은 예:

```text
Add_DifferentCurrency_ThrowsException
Multiply_ByScalar_ReturnsScaledAmount
ToString_Krw_ReturnsWonFormat
```

나쁜 예:

```text
Add_Money_Correctly
Krw_ToString_Works
ReturnsCorrectString
```

## Commit Rules

이 저장소의 commit message는 영어 Conventional Commits를 사용한다.

규칙:

- Codex may create commits when the completed work is coherent, scoped, and verified enough for the current request.
- Do not commit unrelated user changes.
- Ask before committing when the work includes migrations, broad file moves, public API contract changes, security/auth/deployment settings, or any scope that is hard to reverse.
- 형식: `type: short description`
- 대표 type: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`, `ci`
- 제목은 imperative mood로 쓴다. 예: `feat: add reservation availability endpoint`
- 한국어 commit message를 사용하지 않는다.

왜 이 형식인가:

- 해외 채용 담당자나 클라이언트가 git history를 바로 이해할 수 있다.
- 변경 성격이 commit log에서 드러난다.
- Git과 오픈소스 관행에 맞는다.

## Agent Skills

### Issue tracker

Issues and PRDs are tracked in GitHub Issues for `hoonjungdev/hotel-pms`. See `docs/agents/issue-tracker.md`.

### Triage labels

The repo uses the default five-label triage vocabulary. See `docs/agents/triage-labels.md`.

### Domain docs

This is a single-context repo with root-level domain docs. See `docs/agents/domain.md`.
