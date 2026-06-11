using HotelPms.Features.Rooms.Domain.ValueObjects;
using HotelPms.Features.RoomTypes.Domain;
using HotelPms.Shared.Domain;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.Rooms.Domain;

public class Room : AggregateRoot
{
    public RoomId Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public RoomTypeId RoomTypeId { get; private set; }
    public RoomNumber Number { get; private set; }
    public RoomCondition Condition { get; private set; }

    private Room()
    {
        Number = null!;
    }

    private Room(TenantId tenantId, RoomTypeId roomTypeId, RoomNumber number)
    {
        Id = RoomId.New();
        TenantId = tenantId;
        RoomTypeId = roomTypeId;
        Number = number;
        Condition = RoomCondition.Clean;
    }

    public static Room Create(TenantId tenantId, RoomTypeId roomTypeId, RoomNumber number)
    {
        EnsureValidTenantId(tenantId);
        EnsureValidRoomTypeId(roomTypeId);

        return new Room(tenantId, roomTypeId, number);
    }

    public void MarkDirty()
    {
        if (Condition == RoomCondition.OutOfService)
        {
            throw new InvalidOperationException("An out-of-service room cannot be marked dirty.");
        }

        Condition = RoomCondition.Dirty;
    }

    public void MarkClean()
    {
        if (Condition == RoomCondition.OutOfService)
        {
            throw new InvalidOperationException("An out-of-service room cannot be marked clean.");
        }

        Condition = RoomCondition.Clean;
    }

    public void TakeOutOfService()
    {
        Condition = RoomCondition.OutOfService;
    }

    private static void EnsureValidTenantId(TenantId tenantId)
    {
        if (tenantId.Value == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID must be provided.", nameof(tenantId));
        }
    }

    private static void EnsureValidRoomTypeId(RoomTypeId roomTypeId)
    {
        if (roomTypeId.Value == Guid.Empty)
        {
            throw new ArgumentException("Room type ID must be provided.", nameof(roomTypeId));
        }
    }
}
