# HJ-PMS: Hotel Property Management System

소형 게스트하우스/펜션(객실 5~20개) 대상 호텔 PMS.
해외 이직 포트폴리오 + 기술 학습 목적의 사이드 프로젝트.

## 핵심 철학

> "오늘의 문제를 풀되, 내일의 확장을 막지 않는다."

- **작게 시작하고, 고통이 올 때 확장한다** — "나중에 필요할까봐"는 도입 이유가 될 수 없다
- **패턴을 위한 패턴을 만들지 않는다** — 진짜 문제가 생기면 그때 추가
- **도메인 깊이가 진짜 무기다** — 기술 스택 화려함보다 도메인 모델링에 시간을 쓴다
- **완주가 모든 가치 위에 있다** — 미완성된 트렌디한 스택 < 완성된 보수적인 스택
- **의식적인 트레이드오프** — 모든 결정에 "왜 이걸 선택했고 무엇을 포기했는가" 답할 수 있어야 한다

## 기술 스택

- .NET 10 + Blazor (Interactive Server)
- EF Core 10 + PostgreSQL 17 (`tstzrange` 활용)
- ASP.NET Core Identity, Hangfire, FluentValidation
- Serilog + Seq, OpenTelemetry
- MudBlazor + FullCalendar (JS interop)
- 테스트: xUnit, TestContainers, FluentAssertions, Bogus, bUnit
- 배포: Docker + GitHub Actions + Azure/Fly.io

## 아키텍처: Vertical Slice + 선택적 DDD

단일 Blazor 프로젝트. 클린 아키텍처 4-레이어 분리가 아닌 **폴더 규약으로 분리**.

```
src/HotelPms/Features/{FeatureName}/
  ├── Pages/           ← Blazor 페이지
  ├── Components/      ← 기능 전용 컴포넌트
  ├── Domain/          ← 순수 C#. EF/ASP.NET 의존성 없음
  ├── Application/     ← 얇은 오케스트레이션 (Handler 클래스)
  ├── Infrastructure/  ← EF 매핑, DbContext 확장 메서드
  └── EventHandlers/   ← 도메인 이벤트 핸들러
```

### 의식적으로 거부하는 것들

이 항목들은 의도적인 결정이다. 도입을 제안하지 말 것:

| 거부 | 대신 사용 |
|------|----------|
| Repository 패턴 | `DbContext` 직접 사용 + 확장 메서드 |
| AutoMapper / Mapster | 수동 매핑 |
| MediatR | 평범한 Handler 클래스 |
| Full CQRS (읽기/쓰기 모델 분리) | 가벼운 Command/Query 분리 사고만 |
| Event Sourcing | 전통적 CRUD + Domain Event |
| MSA / Bounded Context 분리 | 단일 프로젝트 (1단계) |
| React + 별도 API | Blazor 풀스택 |
| Message Queue / Kafka | 필요해지면 그때 (1단계 없음) |
| 빈혈 도메인 모델 (Anemic) | Rich Domain Model |
| Primitive Obsession | Value Object |

## DDD 규칙

### 도입하는 것

- **Ubiquitous Language**: 호텔 업계 용어를 코드에 그대로 사용 (Reservation, Guest, Stay, RoomCondition 등)
- **Rich Domain Model**: 엔티티에 행위 캡슐화, setter는 private, 불변식은 객체 안에서 보장
- **Value Object**: `record`로 선언. `Money`, `DateRange`, `RoomNumber` 등
- **Aggregate**: 트랜잭션 경계. 다른 집계는 **ID로만 참조**
- **Domain Event**: `record`로 선언. 메모리 내 처리(in-process). `SaveChangesAsync` 후 디스패치
- **Factory Method**: `Reservation.Create()` 패턴으로 불변식 보장

### Domain 폴더 규칙

- **순수 C#만**. EF Core 어노테이션, ASP.NET 의존성 금지
- Entity는 Rich Model — 행위가 안에 있고 setter는 private
- Value Object와 Domain Event는 `record`로
- 생성은 팩토리 메서드로 (public 생성자 노출하지 않음, EF Core용 private 생성자만)

### Application 폴더 규칙

- **얇게 유지**. 비즈니스 로직은 Domain에. 여기는 오케스트레이션만:
  1. 입력 검증 (FluentValidation)
  2. 필요한 데이터 로드
  3. 도메인 객체에 위임 (`reservation.Confirm()`)
  4. 저장 (`SaveChangesAsync`)
- Handler 클래스. 인터페이스 없음. MediatR 없음

### Aggregate 경계

```
Reservation Aggregate: Reservation(루트), ReservationCharge, ReservationGuest
Room Aggregate:         Room(루트), HousekeepingHistory
Guest Aggregate:        Guest(루트)
Payment Aggregate:      Payment(루트)
Pricing:                RatePlan(루트), PriceCalculator(도메인 서비스)
```

### Shared 폴더

`src/HotelPms/Shared/` — 2개 이상의 Feature가 진짜로 함께 쓰는 것만.
처음에는 비어있어도 됨. 중복이 생긴 다음에 추출.

## 테스트 전략: 테스트 트로피

테스트 피라미드가 아닌 **테스트 트로피**. 통합 테스트를 가장 많이 작성.

| 코드 종류 | 테스트 유형 |
|-----------|------------|
| Value Object, 순수 도메인 서비스, 상태 머신 | **단위 테스트** |
| DB 쿼리, Handler, HTTP 요청 | **통합 테스트** (TestContainers + 진짜 PostgreSQL) |
| 예약->결제->체크인 전체 플로우 | **E2E** (1~2개만) |

### Mock 원칙

- **Mock 대상**: 외부 시스템만 (Stripe, 이메일, OTA API)
- **Mock 금지**: 내부 의존성 (DbContext, 내 코드의 서비스)

### 테스트 프로젝트 구조

```
tests/HotelPms.UnitTests/Features/{FeatureName}/Domain/
tests/HotelPms.IntegrationTests/Features/{FeatureName}/
```

## 도메인의 핵심 난제 (= 면접 어필 포인트)

코드 작성 시 이 6가지를 가장 신경 써서 구현:

1. **객실 가용성 계산** — 기간 x 객실 타입 x 재고 (PostgreSQL `tstzrange` + GIST 인덱스)
2. **동시 예약 처리** — 마지막 1실 동시 요청 -> 낙관적 락 + 재시도
3. **객실 배정** — 예약은 타입에, 체크인 시 실제 객실 배정 + 청소 상태 연동
4. **가격 계산** — RoomType x Date x RatePlan x Promotion (순수 함수)
5. **오버부킹 정책** — 허용 여부 + 허용량 (도메인 정책)
6. **상태 머신 무결성** — 잘못된 전이를 도메인에서 차단

## 멀티 테넌시

모든 엔티티에 `TenantId`. EF Core 글로벌 쿼리 필터로 자동 적용.

## 코딩 컨벤션

- 인터페이스는 진짜 다중 구현이 필요할 때만
- 주석은 "왜"가 비자명한 경우에만
- 의심스러우면 더 단순한 쪽을 선택
- "힙해 보여서"는 선택 이유가 될 수 없음

## 현재 단계

**1단계 (코어 MVP)** 진행 중. 2단계(Web API 분리, 대시보드), 3단계(OTA, 모바일)는 아직.
