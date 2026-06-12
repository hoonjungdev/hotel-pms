using FluentValidation;
using HotelPms.Features.Guests;
using HotelPms.Features.Guests.GetGuest;
using HotelPms.Features.Guests.ListGuests;
using HotelPms.Features.Guests.RegisterGuest;
using HotelPms.Features.Reservations;
using HotelPms.Features.Reservations.CancelReservation;
using HotelPms.Features.Reservations.CheckReservationAvailability;
using HotelPms.Features.Reservations.ConfirmReservation;
using HotelPms.Features.Reservations.CreateReservation;
using HotelPms.Features.Reservations.GetReservation;
using HotelPms.Features.Reservations.ListReservations;
using HotelPms.Features.Rooms;
using HotelPms.Features.Rooms.AddRoom;
using HotelPms.Features.Rooms.GetRoom;
using HotelPms.Features.Rooms.ListRooms;
using HotelPms.Features.Rooms.UpdateRoomCondition;
using HotelPms.Features.RoomTypes;
using HotelPms.Features.RoomTypes.CreateRoomType;
using HotelPms.Features.RoomTypes.GetRoomType;
using HotelPms.Features.RoomTypes.ListRoomTypes;
using HotelPms.Infrastructure.Database;
using HotelPms.Infrastructure.Database.Seed;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddProblemDetails();
    builder.Services.AddOpenApi();

    builder.Services.AddDbContext<HotelDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<RegisterGuestHandler>();
    builder.Services.AddScoped<IValidator<RegisterGuestCommand>, RegisterGuestCommandValidator>();
    builder.Services.AddScoped<ListGuestsHandler>();
    builder.Services.AddScoped<GetGuestHandler>();
    builder.Services.AddScoped<AddRoomHandler>();
    builder.Services.AddScoped<IValidator<AddRoomCommand>, AddRoomCommandValidator>();
    builder.Services.AddScoped<ListRoomsHandler>();
    builder.Services.AddScoped<GetRoomHandler>();
    builder.Services.AddScoped<CreateRoomTypeHandler>();
    builder.Services.AddScoped<IValidator<CreateRoomTypeCommand>, CreateRoomTypeCommandValidator>();
    builder.Services.AddScoped<ListRoomTypesHandler>();
    builder.Services.AddScoped<GetRoomTypeHandler>();
    builder.Services.AddScoped<UpdateRoomConditionHandler>();
    builder.Services.AddScoped<IValidator<UpdateRoomConditionCommand>, UpdateRoomConditionCommandValidator>();
    builder.Services.AddScoped<CreateReservationHandler>();
    builder.Services.AddScoped<IValidator<CreateReservationCommand>, CreateReservationCommandValidator>();
    builder.Services.AddScoped<ListReservationsHandler>();
    builder.Services.AddScoped<CheckReservationAvailabilityHandler>();
    builder.Services.AddScoped<IValidator<CheckReservationAvailabilityQuery>, CheckReservationAvailabilityQueryValidator>();
    builder.Services.AddScoped<GetReservationHandler>();
    builder.Services.AddScoped<ConfirmReservationHandler>();
    builder.Services.AddScoped<CancelReservationHandler>();

    WebApplication app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler();
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();

    app.MapOpenApi();
    app.MapGet("/", () => Results.Ok(new
    {
        Name = "hotel-pms API",
        Status = "Running",
        OpenApi = "/openapi/v1.json"
    }));

    app.MapGuestEndpoints();
    app.MapRoomEndpoints();
    app.MapRoomTypeEndpoints();
    app.MapReservationEndpoints();

    if (args.Contains("--seed-demo-data", StringComparer.OrdinalIgnoreCase))
    {
        await app.Services.SeedDemoDataAsync();
    }

    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
