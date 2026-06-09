using FluentValidation;
using HotelPms.Components;
using HotelPms.Features.Guests.Application;
using HotelPms.Features.Rooms.Application;
using HotelPms.Infrastructure.Database;
using HotelPms.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddDbContext<HotelDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddMudServices();

    builder.Services.AddScoped<CurrentTenant>();

    builder.Services.AddScoped<RegisterGuestHandler>();
    builder.Services.AddScoped<IValidator<RegisterGuestCommand>, RegisterGuestCommandValidator>();
    builder.Services.AddScoped<ListGuestsHandler>();
    builder.Services.AddScoped<GetGuestHandler>();
    builder.Services.AddScoped<AddRoomHandler>();
    builder.Services.AddScoped<IValidator<AddRoomCommand>, AddRoomCommandValidator>();
    builder.Services.AddScoped<ListRoomsHandler>();
    builder.Services.AddScoped<CreateRoomTypeHandler>();
    builder.Services.AddScoped<IValidator<CreateRoomTypeCommand>, CreateRoomTypeCommandValidator>();
    builder.Services.AddScoped<ListRoomTypesHandler>();
    builder.Services.AddScoped<UpdateRoomConditionHandler>();
    builder.Services.AddScoped<IValidator<UpdateRoomConditionCommand>, UpdateRoomConditionCommandValidator>();

    WebApplication app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseAntiforgery();
    app.UseSerilogRequestLogging();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (HostAbortedException)
{
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
