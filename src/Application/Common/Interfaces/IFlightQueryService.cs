using AirlineBooking.Application.Flights.Queries;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Common.Interfaces;

public interface IFlightQueryService
{
    Task<IReadOnlyList<FlightDto>> SearchAsync(string from, string to, DateOnly date, CancellationToken ct);
}
