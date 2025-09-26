using FluentValidation;

namespace AirlineBooking.Application.Bookings.Commands;

public sealed class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.FlightId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Seats).GreaterThan(0).LessThanOrEqualTo(9);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(100);
    }
}