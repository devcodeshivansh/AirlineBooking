using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Application.Flights.Queries;
using AirlineBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Infrastructure.Services;

public sealed class FlightQueryService : IFlightQueryService
{
    private readonly AppDbContext _db;
    public FlightQueryService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<FlightDto>> SearchAsync(string from, string to, DateTime date, CancellationToken ct)
    {
        var dateStart = new DateTimeOffset(date, TimeSpan.Zero);
        var dateEnd = dateStart.AddDays(1);
        
        var fromUpper = from.Trim().ToUpperInvariant();
        var toUpper = to.Trim().ToUpperInvariant();
        
        return await _db.Flights
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

       
    }
}
