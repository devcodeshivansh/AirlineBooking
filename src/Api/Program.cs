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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

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
builder.Services.AddScoped<IFlightCreateService, FlightCreateService>();

// Swagger + Rate Limiting + Problem Details
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Airline Booking API",
        Version = "v1"
    });

    // 🔐 Add JWT Authentication scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\r\n\r\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddProblemDetails(opt =>
{
    opt.IncludeExceptionDetails = (ctx, _) => builder.Environment.IsDevelopment();

    opt.OnBeforeWriteDetails = (ctx, problem) =>
    {
        if (!problem.Extensions.ContainsKey("traceId"))
        {
            problem.Extensions["traceId"] = ctx.TraceIdentifier;
        }
    };

    opt.Map<ValidationException>((ctx, exception) =>
    {
        var errors = exception.Errors
            .GroupBy(error => error.PropertyName ?? string.Empty)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Detail = "See the errors property for details.",
            Instance = ctx.Request.Path,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };
    });

    opt.MapToStatusCode<ArgumentException>(StatusCodes.Status400BadRequest);
    opt.MapToStatusCode<ArgumentOutOfRangeException>(StatusCodes.Status400BadRequest);
    opt.MapToStatusCode<InvalidOperationException>(StatusCodes.Status409Conflict);
    opt.MapToStatusCode<DbUpdateException>(StatusCodes.Status409Conflict);
    opt.MapToStatusCode<DbUpdateConcurrencyException>(StatusCodes.Status409Conflict);
    opt.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers(); // ✅ Enable controllers

app.Run();
