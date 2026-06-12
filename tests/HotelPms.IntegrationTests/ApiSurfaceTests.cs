using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelPms.IntegrationTests;

public class ApiSurfaceTests
{
    [Fact]
    public async Task Get_Root_ReturnsApiMetadata()
    {
        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = factory.CreateClient();

        JsonDocument? response = await client.GetFromJsonAsync<JsonDocument>("/");

        Assert.NotNull(response);
        Assert.Equal("hotel-pms API", response.RootElement.GetProperty("name").GetString());
        Assert.Equal("Running", response.RootElement.GetProperty("status").GetString());
        Assert.Equal("/openapi/v1.json", response.RootElement.GetProperty("openApi").GetString());
    }

    [Fact]
    public async Task Get_OpenApiDocument_ReturnsOpenApiJson()
    {
        await using WebApplicationFactory<Program> factory = CreateFactory();
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        JsonDocument document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        Assert.Equal("3.1.1", document.RootElement.GetProperty("openapi").GetString());
        Assert.True(document.RootElement.GetProperty("paths").TryGetProperty("/api/guests", out _));
        Assert.True(document.RootElement.GetProperty("paths").TryGetProperty("/api/rooms", out _));
        Assert.True(document.RootElement.GetProperty("paths").TryGetProperty("/api/room-types", out _));
        Assert.True(document.RootElement.GetProperty("paths").TryGetProperty("/api/reservations", out _));
        Assert.True(document.RootElement.GetProperty("paths").TryGetProperty("/api/reservations/{reservationId}/confirm", out _));
        Assert.True(document.RootElement.GetProperty("paths").TryGetProperty("/api/reservations/{reservationId}/check-in", out _));
        Assert.True(document.RootElement.GetProperty("paths").TryGetProperty("/api/reservations/{reservationId}/check-out", out _));
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
    }
}
