using Microsoft.EntityFrameworkCore;

namespace HotelPms.Infrastructure.Database.Seed;

public static class DemoSeedDataExtensions
{
    public static async Task SeedDemoDataAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = services.CreateScope();
        HotelDbContext context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

        await context.Database.MigrateAsync(cancellationToken);
        await DemoSeedData.SeedAsync(context, cancellationToken);
    }
}
