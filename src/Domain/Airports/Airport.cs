using System;

namespace AirlineBooking.Domain.Airports;

public sealed class Airport
{
    public string Code { get; } // IATA: e.g., DEL, BOM
    public string Name { get; }
    public string City { get; }
    public string Country { get; }

    public Airport(string code, string name, string city, string country)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
            throw new ArgumentException("Airport code must be 3 letters", nameof(code));
        Code = code.ToUpperInvariant();
        Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
        City = city?.Trim() ?? throw new ArgumentNullException(nameof(city));
        Country = country?.Trim() ?? throw new ArgumentNullException(nameof(country));
    }
}