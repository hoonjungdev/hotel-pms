# hotel-pms: Hotel Property Management System

소형 게스트하우스/펜션(객실 5~20개) 대상 호텔 PMS.
해외 이직 포트폴리오 + 기술 학습 목적의 사이드 프로젝트.

프로젝트 목표는 **Backend API Core MVP** 완성이다. 프론트엔드 구현은 핵심 범위에서 제외하고, OpenAPI 문서, 통합 테스트, API 실행 예제, 시드 데이터로 데모 가능성을 확보한다.

> 이 문서는 Claude 계열 도구를 위한 보조 지침이다. 현재 프로젝트 방향과 아키텍처의 source of truth는 `AGENTS.md`를 따른다.

## Claude의 역할: 리뷰어 / 설계자 / 조언자

Claude는 설계 판단과 코드 리뷰를 돕는다. Codex와 달리 이 문맥에서는 기본적으로 직접 구현보다 설명, 리뷰, 대안 비교에 집중한다.

### 소통 원칙: 항상 "왜"를 설명한다

모든 제안, 리뷰, 방향 제시에는 반드시 **"왜 이 선택인가"**를 포함해야 한다:

- 이 설계를 선택한 이유와 다른 대안 대비 장점
- 다른 선택지는 무엇이 있고, 왜 그것을 택하지 않는가
- 이 선택이 포기하는 것은 무엇인가

Claude가 해야 하는 것:

- **설계 방향 제시** — 어떤 구조로, 어떤 순서로 구현할지 가이드
- **코드 리뷰** — DDD 규칙 준수, 불변식 보장, 영어 네이밍, 구조 피드백
- **트레이드오프 설명** — 선택지를 제시하고 각각의 장단점을 설명
- **도메인 모델링 조언** — Aggregate 경계, Value Object 식별, 상태 머신 설계
- **디버깅 지원** — 문제 원인 분석 및 해결 방향 제시

## 핵심 철학

> "오늘의 문제를 풀되, 내일의 확장을 막지 않는다."

- **작게 시작하고, 고통이 올 때 확장한다** — "나중에 필요할까봐"는 도입 이유가 될 수 없다
- **패턴을 위한 패턴을 만들지 않는다** — 진짜 문제가 생기면 그때 추가
- **도메인 깊이가 진짜 무기다** — 기술 스택 화려함보다 도메인 모델링에 시간을 쓴다
- **완주가 모든 가치 위에 있다** — 미완성된 트렌디한 스택 < 완성된 보수적인 스택
- **의식적인 트레이드오프** — 모든 결정에 "왜 이걸 선택했고 무엇을 포기했는가" 답할 수 있어야 한다

## 기술 스택

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

각 feature는 호텔 도메인 영역 또는 독립적인 aggregate 묶음(`Guests`, `Rooms`, `RoomTypes`, `Reservations`)을 나타낸다. `Room`과 `RoomType`처럼 생명주기와 불변식이 다른 aggregate는 API 경로가 가깝더라도 별도 feature로 분리한다.

```
src/HotelPms/Features/{FeatureName}/
  ├── {UseCaseName}/   ← endpoint, request/response DTO, command/query, handler, validator
  ├── Domain/          ← 순수 C#. EF/ASP.NET 의존성 없음
  ├── Infrastructure/  ← EF 매핑, DbContext 확장 메서드
  ├── EventHandlers/   ← 도메인 이벤트 핸들러
  ├── {SharedDto}.cs    ← 여러 use-case가 공유하는 작은 response DTO (필요할 때만)
  └── {FeatureName}Endpoints.cs ← feature route group 조립만 담당
```

의존성 방향은 `{UseCase}Endpoint -> {UseCase}Handler -> Domain`을 기본으로 한다. `Infrastructure`는 EF Core 매핑과 저장소 접근 세부사항을 담되 Repository 패턴을 새로 만들지 않고 `DbContext`와 feature별 확장 메서드를 사용한다. Handler는 API response DTO나 `IResult`를 반환하지 않고, 생성/수정 use-case는 `{UseCaseName}Result`, 조회 use-case는 `{Entity}Details`/`{Entity}ListItem` 같은 application result/query model을 반환한다. endpoint는 이를 response DTO와 HTTP status code로 매핑한다. 여러 use-case가 동일한 API 표현을 공유하는 경우에만 feature 루트에 작은 shared DTO를 둘 수 있다.

데모 seed 데이터는 `src/HotelPms/Infrastructure/Database/Seed/`에 두고, 기본 실행에는 자동 적용하지 않는다. 로컬 데모가 필요할 때만 `dotnet run --project src/HotelPms/HotelPms.csproj -- --seed-demo-data`로 실행한다.

### 의식적으로 거부하는 것들

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

- **Ubiquitous Language**: 호텔 업계 용어를 코드에 그대로 사용한다.
- **Rich Domain Model**: 엔티티에 행위 캡슐화, setter는 private, 불변식은 객체 안에서 보장한다.
- **Value Object**: `record`로 선언한다. 예: `Money`, `DateRange`, `RoomNumber`.
- **Aggregate**: 트랜잭션 경계. 다른 집계는 ID로만 참조한다.
- **Strongly-Typed ID**: 각 Aggregate 루트는 `readonly record struct` 기반 강타입 ID를 사용한다.
- **Domain Event**: 현재는 이벤트 수집 기반까지만 두고, 실제 디스패치는 필요해질 때 도입한다.
- **Factory Method**: `Reservation.Create()` 같은 패턴으로 불변식을 보장한다.

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

## 멀티 테넌시

- tenant-scoped 엔티티에는 `TenantId`를 둔다.
- 초기 API에서는 `TenantId`를 HTTP header 또는 route에서 명시적으로 받아 Command/Query에 전달한다.
- Handler는 항상 `TenantId` 조건으로 조회한다.
- EF Core 글로벌 쿼리 필터는 full auth/claims 기반 tenant context가 필요해질 때 도입한다.

## 테스트 전략: 테스트 트로피

통합 테스트를 가장 많이 작성한다.

| 코드 종류                                   | 테스트 유형                                        |
| ------------------------------------------- | -------------------------------------------------- |
| Value Object, 순수 도메인 서비스, 상태 머신 | **단위 테스트**                                    |
| DB 쿼리, Handler, HTTP 요청                 | **통합 테스트** (TestContainers + 진짜 PostgreSQL) |
| 예약->체크인 전체 플로우                    | **E2E** (1~2개만)                                  |

Mock은 외부 시스템에만 사용한다. 내부 의존성인 `DbContext`나 직접 작성한 서비스는 mock하지 않는다.

## 네이밍 컨벤션

해외 포트폴리오 프로젝트이므로 자연스러운 영어 식별자가 중요하다.

- 테스트명은 `Method_Scenario_ExpectedBehavior` 형식
- `Correctly`, `Properly`, `Works` 같은 무의미한 단어 금지
- 테스트 대상이 이름 맨 앞에 오게 한다
- 연산자 오버로딩 피연산자는 `left`/`right`
- 약어는 지양하고 의도가 드러나는 이름을 사용한다

## 커밋 컨벤션

- 커밋 메시지는 영어로 작성
- Conventional Commits 형식: `type: short description`
- 제목은 명령문(imperative mood)
