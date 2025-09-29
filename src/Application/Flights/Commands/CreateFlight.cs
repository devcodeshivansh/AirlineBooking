using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirlineBooking.Flights.Commands;

public sealed record CreateFlightCommand(
    [Required]
    [RegularExpression("^[A-Z0-9-]{3,10}$", ErrorMessage = "Flight number must contain 3 to 10 alphanumeric characters or hyphen.")]
    string FlightNumber,

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "From airport must be a 3 letter IATA code.")]
    string FromAirport,

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "To airport must be a 3 letter IATA code.")]
    string ToAirport,

    [Required]
    DateTimeOffset DepartureUtc,

    [Required]
    DateTimeOffset ArrivalUtc,

    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
    int Capacity,

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Base fare must be non-negative.")]
    decimal BaseFare
) : IRequest<Guid>, IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(FlightNumber))
        {
            yield return new ValidationResult("Flight number is required.", new[] { nameof(FlightNumber) });
        }

        if (string.IsNullOrWhiteSpace(FromAirport))
        {
            yield return new ValidationResult("Origin airport is required.", new[] { nameof(FromAirport) });
        }

        if (string.IsNullOrWhiteSpace(ToAirport))
        {
            yield return new ValidationResult("Destination airport is required.", new[] { nameof(ToAirport) });
        }

        if (string.Equals(FromAirport, ToAirport, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Origin and destination airports must differ.",
                new[] { nameof(FromAirport), nameof(ToAirport) });
        }

        if (ArrivalUtc <= DepartureUtc)
        {
            yield return new ValidationResult(
                "Arrival time must be after departure time.",
                new[] { nameof(ArrivalUtc), nameof(DepartureUtc) });
        }

        if (Capacity <= 0)
        {
            yield return new ValidationResult("Capacity must be greater than zero.", new[] { nameof(Capacity) });
        }

        if (BaseFare < 0)
        {
            yield return new ValidationResult("Base fare must be non-negative.", new[] { nameof(BaseFare) });
        }
    }
}
