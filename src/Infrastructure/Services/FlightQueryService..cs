using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Application.Flights.Queries;
using AirlineBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Infrastructure.Services;

public sealed class FlightQueryService : IFlightQueryService
{
    private readonly AppDbContext _db;
    private readonly ILogger<FlightQueryService> _logger;

    public FlightQueryService(AppDbContext db, ILogger<FlightQueryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FlightDto>> SearchAsync(string from, string to, DateTime date, CancellationToken ct)
    {
        _logger.LogInformation("Searching flights in persistence from {From} to {To} on {Date}", from, to, date);
        var dateStart = new DateTimeOffset(date, TimeSpan.Zero);
        var dateEnd = dateStart.AddDays(1);

        var fromUpper = from.Trim().ToUpperInvariant();
        var toUpper = to.Trim().ToUpperInvariant();

        var results = await _db.Flights
            .Where(f => f.FromAirport == fromUpper
                        && f.ToAirport == toUpper
                        && f.DepartureUtc >= dateStart
                        && f.DepartureUtc < dateEnd)
            .Select(f => new FlightDto(
                f.Id,
                f.FlightNumber,
                f.FromAirport,
                f.ToAirport,
                f.DepartureUtc,
                f.ArrivalUtc,
                f.BaseFare))
            .ToListAsync(ct);

        _logger.LogInformation("Flight search returned {Count} records", results.Count);
        return results;
    }
}
