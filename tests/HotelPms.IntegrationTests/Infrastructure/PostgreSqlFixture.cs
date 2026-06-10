using HotelPms.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace HotelPms.IntegrationTests.Infrastructure;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using HotelDbContext context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public HotelDbContext CreateDbContext()
    {
        DbContextOptions<HotelDbContext> options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new HotelDbContext(options);
    }
}
