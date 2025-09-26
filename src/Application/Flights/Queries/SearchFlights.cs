using MediatR;
using System;
using System.Collections.Generic;

namespace AirlineBooking.Application.Flights.Queries;

public sealed record SearchFlightsQuery(string From, string To, DateTime Date) : IRequest<IReadOnlyList<FlightDto>>;

public sealed record FlightDto(Guid Id, string FlightNumber, string From, string To, DateTimeOffset DepartureUtc, DateTimeOffset ArrivalUtc, decimal BaseFare);