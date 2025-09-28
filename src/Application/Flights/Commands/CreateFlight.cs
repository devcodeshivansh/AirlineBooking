using MediatR;
using System;


namespace AirlineBooking.Flights.Commands
{
    public record CreateFlightCommand(
        string FlightNumber,
        string FromAirport,
        string ToAirport,
        DateTimeOffset DepartureUtc,
        DateTimeOffset ArrivalUtc,
        int Capacity,
        decimal BaseFare
    ) : IRequest<Guid>;
}
