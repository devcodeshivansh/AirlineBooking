using MediatR;

namespace AirlineBooking.Application.Bookings.Queries;

public sealed record GetBookingByPnrQuery(string Pnr) : IRequest<BookingDto?>;

public sealed record BookingDto(string Pnr, string Status, string FlightNumber, string Passenger, int Seats, decimal Amount);