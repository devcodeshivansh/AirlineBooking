using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AirlineBooking.Application.Bookings.Commands;

public sealed record CreateBookingCommand(
    [Required]
    Guid FlightId,

    [Required]
    [StringLength(100, MinimumLength = 1)]
    string FirstName,

    [Required]
    [StringLength(100, MinimumLength = 1)]
    string LastName,

    [Required]
    [EmailAddress]
    string Email,

    [Range(1, 9, ErrorMessage = "Seats must be between 1 and 9.")]
    int Seats,

    [Required]
    [StringLength(100, MinimumLength = 3)]
    string IdempotencyKey
) : IRequest<CreateBookingResult>, IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FlightId == Guid.Empty)
        {
            yield return new ValidationResult("Flight identifier cannot be empty.", new[] { nameof(FlightId) });
        }

        if (Seats <= 0)
        {
            yield return new ValidationResult("At least one seat must be booked.", new[] { nameof(Seats) });
        }

        if (string.IsNullOrWhiteSpace(FirstName))
        {
            yield return new ValidationResult("First name is required.", new[] { nameof(FirstName) });
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            yield return new ValidationResult("Last name is required.", new[] { nameof(LastName) });
        }

        if (string.IsNullOrWhiteSpace(IdempotencyKey))
        {
            yield return new ValidationResult("Idempotency key is required.", new[] { nameof(IdempotencyKey) });
        }
    }
}

public sealed record CreateBookingResult(string Pnr, Guid BookingId, decimal Amount);
