using HotelPms.Features.Guests.Domain;
using HotelPms.Features.Guests.Infrastructure.Converters;
using HotelPms.Infrastructure.Database.Converters;
using HotelPms.Shared.MultiTenancy;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelPms.Infrastructure.Database;

public class HotelDbContext : IdentityDbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<GuestId>()
            .HaveConversion<GuestIdConverter>();

        configurationBuilder
            .Properties<TenantId>()
            .HaveConversion<TenantIdConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
    }
}
