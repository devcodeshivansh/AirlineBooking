using System;

namespace AirlineBooking.Domain.Flights;

public sealed class Flight
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FlightNumber { get; private set; } = default!; // e.g., AI-101
    public string FromAirport { get; private set; } = default!;  // IATA
    public string ToAirport { get; private set; } = default!;    // IATA
    public DateTimeOffset DepartureUtc { get; private set; }
    public DateTimeOffset ArrivalUtc { get; private set; }
    public int Capacity { get; private set; } // total seats
    public decimal BaseFare { get; private set; } // base fare per seat

    private Flight() { }

    public Flight(string flightNumber, string fromAirport, string toAirport, DateTimeOffset depUtc, DateTimeOffset arrUtc, int capacity, decimal baseFare)
    {
        if (string.IsNullOrWhiteSpace(flightNumber)) throw new ArgumentException("Flight number required");
        if (fromAirport == toAirport) throw new ArgumentException("From and To cannot be same");
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (arrUtc <= depUtc) throw new ArgumentException("Arrival must be after departure");
        if (baseFare < 0) throw new ArgumentOutOfRangeException(nameof(baseFare));

        FlightNumber = flightNumber.Trim().ToUpperInvariant();
        FromAirport = fromAirport.Trim().ToUpperInvariant();
        ToAirport = toAirport.Trim().ToUpperInvariant();
        DepartureUtc = depUtc;
        ArrivalUtc = arrUtc;
        Capacity = capacity;
        BaseFare = baseFare;
    }
}