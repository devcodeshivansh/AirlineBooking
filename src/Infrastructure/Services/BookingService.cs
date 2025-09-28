using AirlineBooking.Application.Bookings.Commands;
using AirlineBooking.Application.Bookings.Queries;
using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Domain.Bookings;
using AirlineBooking.Domain.Inventory;
using AirlineBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Infrastructure.Services;

public sealed class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private readonly IIdempotencyStore _idem;
    private readonly ILogger<BookingService> _logger;
    public BookingService(AppDbContext db, IIdempotencyStore idem, ILogger<BookingService> logger)
    {
        _db = db;
        _idem = idem;
        _logger = logger;
    }

    public async Task<CreateBookingResult> CreateBookingAsync(CreateBookingCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Creating booking for flight {FlightId} with {Seats} seats", request.FlightId, request.Seats);
        if (await _idem.ExistsAsync(request.IdempotencyKey, ct))
        {
            _logger.LogInformation("Idempotency key {Key} already processed, returning existing booking", request.IdempotencyKey);
            var recent = await _db.Bookings
                .OrderByDescending(b => b.CreatedAtUtc)
                .FirstOrDefaultAsync(b => b.FlightId == request.FlightId && b.Seats == request.Seats, ct);
            if (recent is not null) return new CreateBookingResult(recent.Pnr, recent.Id, recent.Amount);
        }

        var flight = await _db.Flights.AsTracking().FirstOrDefaultAsync(f => f.Id == request.FlightId, ct)
            ?? throw LogAndThrow(request.FlightId, "Flight not found");

        var inventory = await _db.SeatInventories.AsTracking().FirstOrDefaultAsync(i => i.FlightId == flight.Id, ct)
            ?? throw LogAndThrow(flight.Id, "Inventory not found");

        var pax = new Passenger(request.FirstName, request.LastName, request.Email);
        _db.Passengers.Add(pax);

        inventory.Reserve(request.Seats);

        var amount = flight.BaseFare * request.Seats;
        var pnr = GeneratePnr();
        var booking = new Booking(flight.Id, pax.Id, request.Seats, amount, pnr);
        _db.Bookings.Add(booking);

        await _db.SaveChangesAsync(ct);
        await _idem.MarkAsync(request.IdempotencyKey, ct);

        _logger.LogInformation("Booking created with BookingId {BookingId} and PNR {Pnr}", booking.Id, booking.Pnr);
        return new CreateBookingResult(pnr, booking.Id, amount);
    }

    public async Task<bool> ConfirmBookingAsync(string pnr, CancellationToken ct)
    {
        _logger.LogInformation("Confirming booking with PNR {Pnr}", pnr);
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Pnr == pnr, ct);
        if (booking is null)
        {
            _logger.LogWarning("Booking with PNR {Pnr} not found", pnr);
            return false;
        }
        if (booking.Status == BookingStatus.Confirmed)
        {
            _logger.LogInformation("Booking with PNR {Pnr} already confirmed", pnr);
            return true;
        }
        if (booking.Status == BookingStatus.Cancelled)
        {
            _logger.LogWarning("Booking with PNR {Pnr} is cancelled and cannot be confirmed", pnr);
            return false;
        }

        var inventory = await _db.SeatInventories.FirstAsync(i => i.FlightId == booking.FlightId, ct);
        inventory.Confirm(booking.Seats);
        booking.Confirm();

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Booking with PNR {Pnr} confirmed", pnr);
        return true;
    }

    public async Task<BookingDto?> GetByPnrAsync(string pnr, CancellationToken ct)
    {
        _logger.LogInformation("Retrieving booking information for PNR {Pnr}", pnr);
        var dto = await _db.Bookings
            .Where(b => b.Pnr == pnr)
            .Select(b => new BookingDto(
                b.Pnr,
                b.Status.ToString(),
                _db.Flights.Where(f => f.Id == b.FlightId).Select(f => f.FlightNumber).First(),
                _db.Passengers.Where(p => p.Id == b.PassengerId).Select(p => p.FirstName + " " + p.LastName).First(),
                b.Seats, b.Amount))
            .FirstOrDefaultAsync(ct);

        if (dto is null)
        {
            _logger.LogWarning("No booking found for PNR {Pnr}", pnr);
        }
        else
        {
            _logger.LogInformation("Booking for PNR {Pnr} retrieved successfully", pnr);
        }

        return dto;
    }

    private static string GeneratePnr()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rnd = Random.Shared;
        return new string(Enumerable.Range(0, 6).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
    }

    private InvalidOperationException LogAndThrow(Guid flightId, string message)
    {
        _logger.LogError("{Message} for flight {FlightId}", message, flightId);
        return new InvalidOperationException(message);
    }
}
