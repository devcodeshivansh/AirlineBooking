using System;

namespace AirlineBooking.Domain.Bookings;

public enum BookingStatus { Reserved = 1, Confirmed = 2, Cancelled = 3 }

public sealed class Booking
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Pnr { get; private set; } = default!;
    public Guid FlightId { get; private set; }
    public Guid PassengerId { get; private set; }
    public int Seats { get; private set; }
    public decimal Amount { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Reserved;
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ConfirmedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }

    private Booking() { }

    public Booking(Guid flightId, Guid passengerId, int seats, decimal amount, string pnr)
    {
        if (seats <= 0) throw new ArgumentOutOfRangeException(nameof(seats));
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        FlightId = flightId;
        PassengerId = passengerId;
        Seats = seats;
        Amount = amount;
        Pnr = pnr;
    }

    public void Confirm()
    {
        if (Status == BookingStatus.Cancelled) throw new InvalidOperationException("Cannot confirm a cancelled booking.");
        Status = BookingStatus.Confirmed;
        ConfirmedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled) return;
        Status = BookingStatus.Cancelled;
        CancelledAtUtc = DateTimeOffset.UtcNow;
    }
}