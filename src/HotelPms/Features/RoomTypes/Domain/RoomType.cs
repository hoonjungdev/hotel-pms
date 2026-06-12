using HotelPms.Features.RoomTypes.Domain.ValueObjects;
using HotelPms.Shared.Domain;
using HotelPms.Shared.Domain.ValueObjects;
using HotelPms.Shared.MultiTenancy;

namespace HotelPms.Features.RoomTypes.Domain;

public class RoomType : AggregateRoot
{
    public RoomTypeId Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public RoomTypeCode Code { get; private set; }
    public string Name { get; private set; }
    public int BaseOccupancy { get; private set; }
    public int MaxOccupancy { get; private set; }
    public Money BaseNightlyRate { get; private set; }

    private RoomType()
    {
        Code = null!;
        Name = null!;
    }

    private RoomType(
        TenantId tenantId,
        RoomTypeCode code,
        string name,
        int baseOccupancy,
        int maxOccupancy,
        Money baseNightlyRate)
    {
        Id = RoomTypeId.New();
        TenantId = tenantId;
        Code = code;
        Name = name;
        BaseOccupancy = baseOccupancy;
        MaxOccupancy = maxOccupancy;
        BaseNightlyRate = baseNightlyRate;
    }

    public static RoomType Create(
        TenantId tenantId,
        RoomTypeCode code,
        string name,
        int baseOccupancy,
        int maxOccupancy,
        Money baseNightlyRate)
    {
        EnsureValidTenantId(tenantId);
        EnsureValidCode(code);
        string normalizedName = name?.Trim() ?? "";
        EnsureValidName(normalizedName);
        EnsureValidOccupancy(baseOccupancy, maxOccupancy);
        EnsureValidBaseNightlyRate(baseNightlyRate);

        return new RoomType(tenantId, code, normalizedName, baseOccupancy, maxOccupancy, baseNightlyRate);
    }

    private static void EnsureValidTenantId(TenantId tenantId)
    {
        if (tenantId.Value == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID must be provided.", nameof(tenantId));
        }
    }

    private static void EnsureValidCode(RoomTypeCode code)
    {
        if (code is null)
        {
            throw new ArgumentException("Code must be provided.", nameof(code));
        }
    }

    private static void EnsureValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Room type name must be provided.", nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException("Room type name is too long.", nameof(name));
        }
    }

    private static void EnsureValidOccupancy(int baseOccupancy, int maxOccupancy)
    {
        if (baseOccupancy < 1)
        {
            throw new ArgumentException("Base occupancy must be at least 1.", nameof(baseOccupancy));
        }

        if (maxOccupancy < baseOccupancy)
        {
            throw new ArgumentException("Max occupancy must be greater than or equal to base occupancy.", nameof(maxOccupancy));
        }
    }

    private static void EnsureValidBaseNightlyRate(Money baseNightlyRate)
    {
        if (!Enum.IsDefined(baseNightlyRate.Currency))
        {
            throw new ArgumentException("Base nightly rate currency must be supported.", nameof(baseNightlyRate));
        }
    }
}
