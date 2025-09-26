using AirlineBooking.Application.Bookings.Commands;
using AirlineBooking.Application.Bookings.Queries;
using AirlineBooking.Application.Common.Behaviors;
using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Application.Flights.Queries;
using AirlineBooking.Infrastructure.Persistence;
using AirlineBooking.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Hellang.Middleware.ProblemDetails;;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext());

// Db: SQLite (dev) / PostgreSQL (prod)
var usePostgres = builder.Configuration.GetValue<bool>("Database:UsePostgres");
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    if (usePostgres)
    {
        var cs = builder.Configuration.GetConnectionString("Postgres");
        opt.UseNpgsql(cs);
    }
    else
    {
        opt.UseSqlite("Data Source=airlinebooking.db");
    }
});

builder.Services.AddControllers();

// MediatR + Validators + Pipeline
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SearchFlightsQuery).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateBookingCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Services
builder.Services.AddScoped<IIdempotencyStore, IdempotencyStore>();
builder.Services.AddScoped<IFlightQueryService, FlightQueryService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (ctx, ex) => true;
});
builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter("fixed", opt =>
{
    opt.Window = TimeSpan.FromSeconds(1);
    opt.PermitLimit = 20;
    opt.QueueLimit = 0;
}));

var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseProblemDetails();
app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI();

// Minimal API endpoints
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/api/flights/search", async (string from, string to, DateTime date, IMediator mediator) =>
{
    var res = await mediator.Send(new SearchFlightsQuery(from, to, date));
    return Results.Ok(res);
}).WithName("SearchFlights").WithOpenApi();

app.MapPost("/api/bookings", async (CreateBookingCommand cmd, IMediator mediator) =>
{
    var res = await mediator.Send(cmd);
    return Results.Ok(res);
}).WithName("CreateBooking").WithOpenApi();

app.MapPost("/api/bookings/confirm/{pnr}", async (string pnr, IMediator mediator) =>
{
    var ok = await mediator.Send(new ConfirmBookingCommand(pnr));
    return ok ? Results.NoContent() : Results.NotFound();
}).WithName("ConfirmBooking").WithOpenApi();

app.MapGet("/api/bookings/{pnr}", async (string pnr, IMediator mediator) =>
{
    var res = await mediator.Send(new GetBookingByPnrQuery(pnr));
    return res is null ? Results.NotFound() : Results.Ok(res);
}).WithName("GetBookingByPnr").WithOpenApi();

// Seed dev data (only if DB empty)
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    await db.Database.MigrateAsync();
//    if (!db.Flights.Any())
//    {
//        var delhi = "DEL"; var mumbai = "BOM";
//        db.Flights.Add(new AirlineBooking.Domain.Flights.Flight("AI101", delhi, mumbai,
//            DateTimeOffset.UtcNow.Date.AddHours(6), DateTimeOffset.UtcNow.Date.AddHours(8), 180, 4500));
//        db.Flights.Add(new AirlineBooking.Domain.Flights.Flight("AI102", mumbai, delhi,
//            DateTimeOffset.UtcNow.Date.AddHours(18), DateTimeOffset.UtcNow.Date.AddHours(20), 180, 4700));
//        await db.SaveChangesAsync();

//        var flights = db.Flights.ToList();
//        foreach (var f in flights)
//            db.SeatInventories.Add(new AirlineBooking.Domain.Inventory.SeatInventory(f.Id, f.Capacity));
//        await db.SaveChangesAsync();
//    }
//}

app.Run();