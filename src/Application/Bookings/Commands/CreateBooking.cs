using MediatR;
using System;

namespace AirlineBooking.Application.Bookings.Commands;

public sealed record CreateBookingCommand(Guid FlightId, string FirstName, string LastName, string Email, int Seats, string IdempotencyKey) : IRequest<CreateBookingResult>;
public sealed record CreateBookingResult(string Pnr, Guid BookingId, decimal Amount);