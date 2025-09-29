using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirlineBooking.Application.Flights.Queries;

public sealed record SearchFlightsQuery(
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "From airport must be a 3 letter IATA code.")]
    string From,

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "To airport must be a 3 letter IATA code.")]
    string To,

    [Required]
    DateTime Date
) : IRequest<IReadOnlyList<FlightDto>>, IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(From))
        {
            yield return new ValidationResult("Origin airport is required.", new[] { nameof(From) });
        }

        if (string.IsNullOrWhiteSpace(To))
        {
            yield return new ValidationResult("Destination airport is required.", new[] { nameof(To) });
        }

        if (string.Equals(From, To, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Origin and destination airports must differ.", new[] { nameof(From), nameof(To) });
        }
    }
}

public sealed record FlightDto(Guid Id, string FlightNumber, string From, string To, DateTimeOffset DepartureUtc, DateTimeOffset ArrivalUtc, decimal BaseFare);
