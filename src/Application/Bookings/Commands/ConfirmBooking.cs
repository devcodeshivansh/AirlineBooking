using MediatR;

namespace AirlineBooking.Application.Bookings.Commands;

public sealed record ConfirmBookingCommand(string Pnr) : IRequest<bool>;