using MediatR;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AirlineBooking.Application.Bookings.Queries;

public sealed record GetBookingByPnrQuery(
    [Required]
    [RegularExpression("^[A-Z0-9]{6}$", ErrorMessage = "PNR must be exactly 6 alphanumeric characters.")]
    string Pnr
) : IRequest<BookingDto?>, IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Pnr))
        {
            yield return new ValidationResult("PNR cannot be empty.", new[] { nameof(Pnr) });
            yield break;
        }

        if (Pnr.Length != 6)
        {
            yield return new ValidationResult("PNR must contain exactly six characters.", new[] { nameof(Pnr) });
        }

        if (Pnr.Length == 6 && Pnr.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            yield return new ValidationResult("PNR must contain only letters and digits.", new[] { nameof(Pnr) });
        }
    }
}

public sealed record BookingDto(string Pnr, string Status, string FlightNumber, string Passenger, int Seats, decimal Amount);
