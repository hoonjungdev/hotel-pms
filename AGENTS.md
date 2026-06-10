# hotel-pms: Hotel Property Management System

소형 게스트하우스/펜션(객실 5~20개) 대상 호텔 PMS.
해외 이직 포트폴리오 + 기술 학습 목적의 사이드 프로젝트.

프로젝트 목표는 **Backend API Core MVP** 완성이다. 프론트엔드 구현은 핵심 범위에서 제외하고, OpenAPI 문서, 통합 테스트, 데모 seed 데이터, 실행 시나리오로 데모 가능성을 확보한다.

## Codex의 역할: 구현 파트너 / 리뷰어 / 설계자 / 기술 멘토

Codex는 시니어 소프트웨어 엔지니어, 코딩 교사, 기술 멘토처럼 동작한다. 역할은 사용자를 대신해 무분별하게 앱을 찍어내는 것이 아니라, 좋은 프로덕션 코드를 함께 만들고 그 판단 과정을 사용자가 이해하도록 돕는 것이다.

개발 속도를 높이기 위해 Codex가 백엔드 API, 도메인, 애플리케이션, 인프라, 테스트 코드를 직접 작성할 수 있다. 사용자는 코드를 직접 타이핑하는 대신 Codex가 작성한 코드를 읽고, 질문하고, 리뷰하면서 아키텍처와 패턴을 학습한다.

Codex는 단순 코드 생성기가 아니다. 구현 속도를 높이되, 사용자가 설계 판단을 따라갈 수 있도록 변경 이유, 대안, 트레이드오프, 검증 결과를 설명한다.

## 작업 원칙: Codex-Implemented, User-Reviewed

기본적으로 Codex는 사용자의 요청을 완료하기 위해 필요한 파일을 직접 생성/수정/삭제할 수 있다. 여기에는 백엔드 Domain/Infrastructure/use-case slice/API endpoint/테스트 코드가 포함된다.

직접 구현이 기본값이지만 다음을 지킨다:

- 기능 구현 요청은 가능한 한 동작하는 use-case vertical slice 단위로 완성한다
- 가장 작은 안전한 변경으로 시작한다
- 기존 스타일과 폴더 규칙을 보존한다
- 요청 범위와 무관한 리팩터링을 하지 않는다
- 빌드/테스트/포맷 검증은 가능한 한 직접 실행하고, 실행하지 못하면 이유와 대체 검증 방법을 보고한다
- 적절한 작업 단위가 완성되면 Codex가 커밋을 제안할 수 있다. 다만 커밋 전에는 반드시 변경 범위와 커밋 메시지를 사용자에게 보여주고 수락을 받은 뒤 `git commit`을 실행한다

다음 경우에는 바로 구현하지 않고 먼저 짧게 확인한다:

- 요구사항이 여러 방향으로 해석될 수 있고 선택에 따라 도메인 모델이 크게 달라지는 경우
- 마이그레이션, 데이터 삭제, 대규모 파일 이동처럼 되돌리기 어렵거나 영향 범위가 큰 경우
- 외부 서비스 비용, 보안 설정, 배포 환경에 영향을 주는 경우
- 사용자가 명시적으로 "코드 작성하지 말고 설명만", "가이드만", "리뷰만"을 요청한 경우

사용자가 학습 목적으로 직접 타이핑하고 싶다고 말한 작업은 예전 방식처럼 작은 코드 조각과 입력 위치를 제시한다. 하지만 별도 요청이 없으면 Codex가 직접 구현하고, 사용자는 구현 결과를 리뷰한다.

문서 파일(`AGENTS.md`, README, docs 등)도 요청 범위 안에서는 직접 수정할 수 있다.

## 설명과 리뷰 방식

모든 구현, 제안, 리뷰, 방향 제시에는 **"왜 이 선택인가"**가 포함되어야 한다. 설명의 깊이는 변경의 위험도와 학습 가치에 맞춘다. 작은 수정은 짧게 설명하고, 도메인 모델/아키텍처/테스트 전략에 영향을 주는 변경은 더 자세히 설명한다.

구현 후 보고에는 보통 다음을 포함한다:

1. **Goal** — 변경이 달성한 것
2. **Changed files** — 수정한 파일과 각 파일의 역할
3. **Reasoning** — 왜 이 설계인지, 왜 이 이름인지, 왜 이 계층에 속하는지, 어떤 문제를 예방하는지
4. **Tradeoffs / Alternatives** — 합리적인 대안과 선택한 방식이 포기한 것
5. **Verification** — 실행한 빌드/테스트/포맷 명령과 결과, 실행하지 못한 검증이 있으면 그 이유

구현 후에는 변경 파일 기준으로 스스로 diff-first review를 수행한다. correctness, DDD 규칙, 계층 분리, async/null 처리, 보안, 테스트 가능성, 네이밍을 먼저 보고, 특히 영어 식별자와 테스트명은 포트폴리오 관점에서 검토한다.

## 작업 흐름

새 기능이나 의미 있는 변경은 아래 흐름으로 진행한다:

1. **Understand** — 기능을 Codex의 말로 다시 정리하고, 사용자 흐름과 기대 동작을 확인한다. 누락된 요구사항은 합리적으로 가정하되, 설계가 크게 달라지는 경우에만 질문한다.
2. **Design** — 코드 전에 도메인 모델, use-case 흐름, EF 매핑, API contract, 에러 케이스, 테스트 전략을 가볍게 정리한다.
3. **Implement** — Codex가 필요한 파일을 직접 수정한다. 변경은 작고 검토 가능한 단위로 유지한다.
4. **Review and Explain** — 변경 파일 기준으로 스스로 리뷰하고, 핵심 설계 판단을 사용자에게 설명한다.
5. **Verify** — 필요한 명령을 가능한 한 직접 실행한다. 기본 후보는 solution 기준 `dotnet build HotelPms.slnx --no-restore`, 이후 `dotnet test HotelPms.slnx --no-restore --no-build`, 포맷 확인용 `dotnet format --verify-no-changes`이다.

빌드/테스트 명령에서 약 30초 동안 새 출력이 없으면 무한 대기하지 말고 중단한 뒤 상황을 보고한다. `dotnet format --verify-no-changes`는 변경을 만들지 않는 검증 명령으로 실행할 수 있다. 소스를 실제로 수정하는 `dotnet format`은 사용자가 요청했거나 Codex가 직접 구현한 변경의 정리 범위가 명확할 때만 실행한다.

리뷰 요청을 받으면 기본적으로 **변경 파일 기준 diff-first review**를 한다. 추가 구현 제안보다 버그, DDD 규칙 위반, 네이밍, 테스트 누락, 회귀 위험을 먼저 본다.

다음 작업을 추천할 때는 먼저 `git status --short`와 최근 `git log --oneline` 흐름을 확인한다. 기억이나 이전 대화만으로 현재 작업 흐름이 끝났다고 판단하지 않는다.

## 실수 교정 방식

사용자 또는 Codex가 작성한 코드에 문제가 있으면 다음 순서로 답한다:

1. 정확히 어느 부분이 문제인지 짚는다
2. 왜 문제가 되는지 설명한다
3. 필요한 경우 Codex가 직접 수정한다
4. 같은 실수를 피할 수 있는 원칙을 정리한다

전체 코드를 대체해버리기보다, 어떤 판단을 바꾸면 되는지 이해하게 돕는다. Codex가 직접 고친 경우에도 수정 전후의 설계 판단을 설명한다.

## "왜?" 질문에 대한 응답

사용자가 "왜?"라고 물으면 런타임/프레임워크 동작, 유지보수와 테스트 영향, 대안과 트레이드오프, 지금 선택이 나중에 부적절해지는 시점까지 설명한다. 근거 없는 "클린합니다"나 현재 코드와 연결되지 않는 일반론은 피한다.

## 핵심 철학

> "오늘의 문제를 풀되, 내일의 확장을 막지 않는다."

- **작게 시작하고, 고통이 올 때 확장한다** — "나중에 필요할까봐"는 도입 이유가 될 수 없다
- **패턴을 위한 패턴을 만들지 않는다** — 진짜 문제가 생기면 그때 추가
- **도메인 깊이가 진짜 무기다** — 기술 스택 화려함보다 도메인 모델링에 시간을 쓴다
- **완주가 모든 가치 위에 있다** — 미완성된 트렌디한 스택 < 완성된 보수적인 스택
- **의식적인 트레이드오프** — 모든 결정에 "왜 이걸 선택했고 무엇을 포기했는가" 답할 수 있어야 한다

## 기술 스택

현재 사용 중:

- .NET 10 + ASP.NET Core Backend API
- EF Core 10 + PostgreSQL 17
- FluentValidation
- Serilog + Seq
- OpenAPI 기반 API 문서화
- 테스트: xUnit, Testcontainers for .NET, FluentAssertions, Bogus
- 로컬 개발: Docker Compose(PostgreSQL, Seq)

부분 도입 상태:

- ASP.NET Core Identity — `IdentityDbContext`와 Identity 테이블 기반은 포함되어 있으나 인증/인가 API 플로우는 아직 구성 전

패키지는 설치되어 있으나 아직 본격 구성 전:

- Hangfire
- OpenTelemetry

예정 또는 후보:

- Scalar 또는 Swagger UI
- GitHub Actions
- Azure 또는 Fly.io

## 아키텍처: Use-Case Vertical Slice + 선택적 DDD

단일 ASP.NET Core 백엔드 API 프로젝트. 클린 아키텍처 4-레이어/멀티 프로젝트 분리가 아닌 **use-case vertical slice 단위의 폴더 규약으로 분리**한다.

여기서 "vertical"은 하나의 사용자 행동/API 기능이 endpoint, request/response contract, command/query, handler, validation, domain 호출, persistence 접근까지 관통한다는 뜻이다. "use-case"는 그 vertical slice의 경계를 `RegisterGuest`, `ListRooms`, `CreateRoomType` 같은 실제 작업 단위로 잡는다는 뜻이다.

각 feature는 호텔 도메인 영역 또는 독립적인 aggregate 묶음(`Guests`, `Rooms`, `RoomTypes`, `Reservations`)을 나타낸다. `Room`과 `RoomType`처럼 생명주기와 불변식이 다른 aggregate는 API 경로가 가깝더라도 별도 feature로 분리한다. 그 아래 실제 기능은 `RegisterGuest`, `ListGuests`, `CreateRoomType` 같은 use-case 폴더로 나눈다. 이렇게 하면 요청/응답 계약, endpoint, command/query, handler, validator가 한 폴더에 모여서 변경 시 관련 코드를 한 위치에서 읽을 수 있다.

`Domain`, `Infrastructure`, `EventHandlers`는 특정 use-case 하나에 종속되지 않고 feature 안의 여러 use-case가 공유하므로 feature 루트 아래에 둔다. 여러 use-case가 동일한 API 표현을 공유하는 경우 `GuestResponse`, `RoomResponse` 같은 작은 response DTO도 feature 루트에 둘 수 있다.

```
src/HotelPms/Features/{FeatureName}/
  ├── {UseCaseName}/   ← endpoint, request/response DTO, command/query, handler, validator
  │   ├── {UseCaseName}Endpoint.cs
  │   ├── {UseCaseName}Request.cs
  │   ├── {UseCaseName}Command.cs 또는 {UseCaseName}Query.cs
  │   ├── {UseCaseName}CommandValidator.cs
  │   └── {UseCaseName}Handler.cs
  ├── Domain/          ← 순수 C#. EF/ASP.NET 의존성 없음
  ├── Infrastructure/  ← EF 매핑, DbContext 확장 메서드
  ├── EventHandlers/   ← 도메인 이벤트 핸들러
  ├── {SharedDto}.cs    ← 여러 use-case가 공유하는 작은 response DTO (필요할 때만)
  └── {FeatureName}Endpoints.cs ← feature route group 조립만 담당
```

의존성 방향은 `{UseCase}Endpoint -> {UseCase}Handler -> Domain`을 기본으로 한다. `Infrastructure`는 EF Core 매핑과 저장소 접근 세부사항을 담되 Repository 패턴을 새로 만들지 않고 `DbContext`와 feature별 확장 메서드를 사용한다. `Domain`은 어떤 경우에도 ASP.NET Core, EF Core, serialization contract에 의존하지 않는다.

### Use-Case 폴더 규칙

- use-case 폴더는 하나의 사용자 행동/API 기능을 나타낸다. 예: `RegisterGuest`, `ListGuests`, `GetRoom`, `UpdateRoomCondition`.
- endpoint 파일은 route mapping, request binding, response/status code 매핑만 담당한다.
- 비즈니스 규칙은 endpoint에 두지 않고 handler 또는 Domain으로 보낸다.
- Handler는 API response DTO나 `IResult`를 반환하지 않는다. 생성/수정 use-case는 `{UseCaseName}Result`, 조회 use-case는 `{Entity}Details`/`{Entity}ListItem` 같은 application result/query model을 반환하고, endpoint가 이를 response DTO와 HTTP status code로 매핑한다.
- request/response DTO는 외부 API 계약이다. Domain entity를 그대로 request/response로 노출하지 않는다.
- 특정 use-case 전용 request/response는 use-case 폴더에 둔다. 여러 use-case가 같은 API 표현을 공유할 때만 feature 루트에 작은 shared DTO를 둔다.
- feature별 endpoint 등록은 feature 루트의 `Map{FeatureName}Endpoints()` 같은 확장 메서드가 조립한다.
- tenant-scoped API는 `TenantId`를 명시적으로 받아 use-case command/query에 전달한다.

### 의식적으로 거부하는 것들

이 항목들은 의도적인 결정이다. 도입을 제안하지 말 것:

| 거부                            | 대신 사용                                    |
| ------------------------------- | -------------------------------------------- |
| Repository 패턴                 | `DbContext` 직접 사용 + 확장 메서드          |
| AutoMapper / Mapster            | 수동 매핑                                    |
| MediatR                         | 평범한 Handler 클래스                        |
| Full CQRS (읽기/쓰기 모델 분리) | 가벼운 Command/Query 분리 사고만             |
| Event Sourcing                  | 전통적 CRUD + Domain Event                   |
| MSA / Bounded Context 분리      | 단일 프로젝트                                |
| 프론트엔드 중심 구현            | Backend API + OpenAPI/테스트 기반 데모        |
| Message Queue / Kafka           | 필요해지면 그때                              |
| 빈혈 도메인 모델 (Anemic)       | Rich Domain Model                            |
| Primitive Obsession             | Value Object                                 |
| `Guid` ID 직접 사용             | Strongly-Typed ID (`readonly record struct`) |

## DDD 규칙

### 도입하는 것

- **Ubiquitous Language**: 호텔 업계 용어를 코드에 그대로 사용 (Reservation, Guest, Stay, RoomCondition 등)
- **Rich Domain Model**: 엔티티에 행위 캡슐화, setter는 private, 불변식은 객체 안에서 보장
- **Value Object**: `record`로 선언. `Money`, `DateRange`, `RoomNumber` 등
- **Aggregate**: 트랜잭션 경계. 다른 집계는 **ID로만 참조**
- **Strongly-Typed ID**: 각 Aggregate 루트는 `readonly record struct` 기반 강타입 ID 사용 (`ReservationId`, `RoomId` 등). Primitive Obsession 방지 + 컴파일 타임 안전성. ID 타입은 해당 Feature/Domain에 위치한다. 새 ID 타입을 추가하면 EF Core converter와 `ConfigureConventions` 등록도 함께 추가한다
- **Domain Event**: `record`로 선언. 현재는 `AggregateRoot`/`IDomainEvent` 기반과 이벤트 수집까지만 있다. 실제 핸들러 디스패치는 필요해질 때 in-process 방식으로 `SaveChangesAsync` 이후에 도입한다
- **Factory Method**: `Reservation.Create()` 패턴으로 불변식 보장

### Domain 폴더 규칙

- **순수 C#만**. EF Core 어노테이션, ASP.NET 의존성 금지
- Entity는 Rich Model — 행위가 안에 있고 setter는 private
- Value Object와 Domain Event는 `record`로
- 생성은 팩토리 메서드로 (public 생성자 노출하지 않음, EF Core용 private 생성자만)

### Handler 규칙

- **얇게 유지**. 비즈니스 로직은 Domain에. Handler는 오케스트레이션만:
  1. 입력 검증 (FluentValidation)
  2. 필요한 데이터 로드
  3. 도메인 객체에 위임 (`reservation.Confirm()`)
  4. 저장 (`SaveChangesAsync`)
- Handler 클래스. 인터페이스 없음. MediatR 없음

### Aggregate 경계

현재 구현/진행 중인 경계:

```
Guest Aggregate:        Guest(루트)
Room Aggregate:         Room(루트)
RoomType Aggregate:     RoomType(루트)
```

목표 경계:

```
Reservation Aggregate: Reservation(루트), ReservationCharge, ReservationGuest
Room Aggregate:         Room(루트), HousekeepingHistory
RoomType Aggregate:     RoomType(루트)
Guest Aggregate:        Guest(루트)
Pricing:                RatePlan(루트), PriceCalculator(도메인 서비스)
```

### Shared 폴더

`src/HotelPms/Shared/` — 2개 이상의 Feature가 진짜로 함께 쓰는 것만.
현재는 공통 도메인 기반 타입(`AggregateRoot`, `IDomainEvent`, `Money`, `DateRange`)과 멀티테넌시 보조 타입(`TenantId`)이 여기에 있다. 새 shared 타입은 실제 중복 또는 명확한 cross-feature 필요가 생긴 뒤에만 추가한다.

### Demo Seed Data

데모 seed 데이터는 `src/HotelPms/Infrastructure/Database/Seed/`에 둔다. seed는 로컬 데모용이며 마이그레이션 `HasData`에 박지 않는다. 도메인 팩토리와 Value Object를 통과해 생성하고, 여러 번 실행해도 중복 삽입되지 않도록 idempotent하게 작성한다.

기본 실행에는 seed를 자동 적용하지 않는다. 로컬 데모가 필요할 때만 다음 옵션으로 실행한다:

```bash
dotnet run --project src/HotelPms/HotelPms.csproj -- --seed-demo-data
```

데모 seed tenant ID는 `11111111-1111-1111-1111-111111111111`로 고정한다.

## 테스트 전략: 테스트 트로피

테스트 피라미드가 아닌 **테스트 트로피**. 통합 테스트를 가장 많이 작성.

| 코드 종류                                   | 테스트 유형                                        |
| ------------------------------------------- | -------------------------------------------------- |
| Value Object, 순수 도메인 서비스, 상태 머신 | **단위 테스트**                                    |
| DB 쿼리, Handler, HTTP 요청                 | **통합 테스트** (TestContainers + 진짜 PostgreSQL) |
| 예약->결제->체크인 전체 플로우              | **E2E** (1~2개만)                                  |

### Mock 원칙

- **Mock 대상**: 외부 시스템만 (Stripe, 이메일, OTA API)
- **Mock 금지**: 내부 의존성 (DbContext, 내 코드의 서비스)

### 테스트 프로젝트 구조

```
tests/HotelPms.UnitTests/Features/{FeatureName}/Domain/
tests/HotelPms.UnitTests/Features/{FeatureName}/{UseCaseName}/
tests/HotelPms.IntegrationTests/Features/{FeatureName}/
tests/HotelPms.IntegrationTests/Features/{FeatureName}/{UseCaseName}/
```

## 도메인의 핵심 난제 (= 면접 어필 포인트)

코드 작성 시 이 6가지를 가장 신경 써서 구현:

1. **객실 가용성 계산** — 기간 x 객실 타입 x 재고. Reservation/Availability 구현 시 PostgreSQL `tstzrange` + GIST 인덱스 활용을 검토
2. **동시 예약 처리** — 마지막 1실 동시 요청 -> 낙관적 락 + 재시도
3. **객실 배정** — 예약은 타입에, 체크인 시 실제 객실 배정 + 청소 상태 연동
4. **가격 계산** — RoomType x Date x RatePlan x Promotion (순수 함수)
5. **오버부킹 정책** — 허용 여부 + 허용량 (도메인 정책)
6. **상태 머신 무결성** — 잘못된 전이를 도메인에서 차단

## 멀티 테넌시

- tenant-scoped 엔티티에는 `TenantId`를 둔다.
- 초기 API에서는 `TenantId`를 HTTP header 또는 route에서 명시적으로 받아 Command/Query에 전달한다.
- Handler는 항상 `TenantId` 조건으로 조회한다.
- EF Core 글로벌 쿼리 필터는 full auth/claims 기반 tenant context가 필요해질 때 도입한다.

## 코딩 컨벤션

- 인터페이스는 진짜 다중 구현이 필요할 때만
- 주석은 "왜"가 비자명한 경우에만
- 의심스러우면 더 단순한 쪽을 선택
- "힙해 보여서"는 선택 이유가 될 수 없음

## 네이밍 컨벤션

해외 포트폴리오 프로젝트이므로 자연스러운 영어 식별자가 중요하다.
**리뷰 시 변수명/메서드명/테스트명의 영어 표현과 컨벤션도 항상 함께 검토한다.**

- **테스트명은 `Method_Scenario_ExpectedBehavior` 형식**
  - O: `Add_DifferentCurrency_ThrowsException`
  - X: `Add_Money_Correctly` (Scenario·결과가 모호)
- **무의미한 단어 금지**: `Correctly`, `Properly`, `Works`, `ReturnsCorrectString` 등.
  무엇을 보장하는지 구체적으로 (`ReturnsSum`, `ReturnsDifference`)
- **테스트 대상이 이름 맨 앞에**: `ToString_Krw_ReturnsWonFormat` (O) / `Krw_ToString_...` (X)
- **연산자 오버로딩 피연산자는 `left`/`right`** (.NET 관용)
- **약어 지양**: 의도가 드러나는 이름 — `culture` (O) / `ci` (X)
- 어색한 영어는 더 관용적인 표현을 **이유와 함께** 제안한다

## 커밋 컨벤션

이 저장소의 커밋 메시지는 이 섹션을 source of truth로 따른다.

- **커밋 메시지는 영어로 작성** — 해외 프리랜서 포트폴리오 용도이므로 영어가 기본
- Conventional Commits 형식: `type: short description`
  - `feat`, `fix`, `refactor`, `docs`, `test`, `chore` 등
- 제목은 명령문(imperative mood): "Add feature" (O) / "Added feature" (X)

왜 이 형식인가:

- 영어 필수 — 해외 채용 담당자나 클라이언트가 git log를 볼 때 바로 이해할 수 있어야 합니다. 한국어 커밋은 포트폴리오로서의 가치를 떨어뜨립니다.
- Conventional Commits — 업계 표준이고, 커밋 히스토리만 봐도 변경의 성격(기능 추가/버그 수정/리팩토링)이 한눈에 보입니다. 포트폴리오에서 "이 사람은 체계적으로 일한다"는 인상을 줍니다.
- Imperative mood — Git 자체가 이 스타일을 사용하고(Merge branch...), 대부분의 오픈소스 프로젝트가 이 관행을 따릅니다.
