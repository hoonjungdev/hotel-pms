# Hotel PMS

This context describes the domain language for a small guesthouse or pension property management system.

## Language

**Room Type**:
A sellable category of room, such as Single, Double, or Family, used for inventory and reservation availability before a physical room is assigned.
_Avoid_: Room class, room category

**Base Nightly Rate**:
The default price for one night of stay in a room type before more advanced rate plans or promotions are applied.
_Avoid_: Price, fee, tariff

**Reservation Total**:
The monetary amount captured for a reservation's stay at creation time. It represents the quoted stay total, not a live recalculation.
_Avoid_: Current price, running total

**Room Assignment**:
The link between a reservation and the physical room the guest will occupy. Reservations are sold by room type first, then assigned to a room at check-in.
_Avoid_: Room booking, room allocation

**Check-in**:
The arrival action that marks a confirmed reservation as in-house and assigns a clean physical room.
_Avoid_: Arrival, admission

**Check-out**:
The departure action that ends an in-house reservation and makes the assigned room require cleaning.
_Avoid_: Departure, closing a stay

**Reservation Availability**:
The inventory state that answers whether a room type can accept another reservation for a stay period. It is based on sellable room inventory and overlapping active reservations; Pending and Confirmed reservations consume availability, while Cancelled reservations do not.
_Avoid_: Vacancy, free room check
