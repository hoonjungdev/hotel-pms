# 0002. Capture Reservation Total at Creation

## Status

Accepted

## Context

Reservations need a monetary total that represents the amount quoted for the stay. In the Core MVP, this total is calculated from the room type's base nightly rate and the stay period.

Room type pricing can change over time. Future pricing features may introduce rate plans, promotions, seasonal prices, manual adjustments, or channel-specific pricing. If reservation totals were always recalculated from the current pricing rules, historical reservations could change when pricing configuration changes later.

The project needs a simple pricing model now, but it also needs a clear boundary between a quoted reservation amount and future pricing rules.

## Decision

Capture the reservation total when the reservation is created.

The reservation stores this captured monetary amount as its reservation total. It represents the quoted stay total at creation time, not a live recalculation from the current room type rate.

For the Core MVP, the total is calculated from:

- the selected room type's base nightly rate
- the stay period length in nights

Do not update existing reservation totals automatically when room type pricing changes later.

## Consequences

Positive consequences:

- Historical reservations keep the amount that was quoted when they were created.
- Future pricing changes do not silently mutate existing reservations.
- Reservation list/detail responses can show a stable amount without recalculating pricing rules.
- The MVP pricing model stays simple while leaving room for future pricing features.

Tradeoffs:

- The system needs an explicit workflow later if a reservation total must be re-quoted or adjusted.
- The stored total may differ from current room type pricing after rates change.
- Future pricing features will need to decide which pricing inputs are captured for auditability.

## Alternatives Considered

### Recalculate reservation totals on every read

Rejected. This would make historical reservations dependent on current pricing configuration and could change past quoted amounts without a user action.

### Store only nightly rate and nights

Rejected for now. It captures more detail than the Core MVP needs but still leaves unclear how future discounts, promotions, or manual adjustments should be represented.

### Introduce full pricing snapshots now

Rejected for now. A full pricing snapshot may become useful when rate plans and promotions exist, but introducing it before those use cases would add premature structure.
