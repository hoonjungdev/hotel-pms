# Hotel PMS

This context describes the domain language for a small guesthouse or pension property management system.

## Language

**Reservation Availability**:
The inventory state that answers whether a room type can accept another reservation for a stay period. It is based on sellable room inventory and overlapping active reservations; Pending and Confirmed reservations consume availability, while Cancelled reservations do not.
_Avoid_: Vacancy, free room check
