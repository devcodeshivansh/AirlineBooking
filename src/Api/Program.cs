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
using Hellang.Middleware.ProblemDetails;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext());

// DbContext: SQLite or PostgreSQL
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

builder.Services.AddControllers(); // ✅ Add Controllers

// MediatR + Validation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SearchFlightsQuery).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateBookingCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// App Services
builder.Services.AddScoped<IIdempotencyStore, IdempotencyStore>();
builder.Services.AddScoped<IFlightQueryService, FlightQueryService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Swagger + Rate Limiting + Problem Details
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails(opt =>
{
    opt.IncludeExceptionDetails = (ctx, ex) => true;
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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // ✅ Enable controllers

app.Run();
