using System;

namespace AirlineBooking.Domain.Inventory;

public sealed class SeatInventory
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid FlightId { get; private set; }
    public int TotalSeats { get; private set; }
    public int ReservedSeats { get; private set; }
    public int ConfirmedSeats { get; private set; }

    private SeatInventory() { }

    public SeatInventory(Guid flightId, int totalSeats)
    {
        if (totalSeats <= 0) throw new ArgumentOutOfRangeException(nameof(totalSeats));
        FlightId = flightId;
        TotalSeats = totalSeats;
    }

    public int AvailableSeats() => TotalSeats - ReservedSeats - ConfirmedSeats;

    public void Reserve(int seats)
    {
        if (seats <= 0) throw new ArgumentOutOfRangeException(nameof(seats));
        if (AvailableSeats() < seats) throw new InvalidOperationException("Not enough seats.");
        ReservedSeats += seats;
    }

    public void Confirm(int seats)
    {
        if (seats <= 0) throw new ArgumentOutOfRangeException(nameof(seats));
        if (ReservedSeats < seats) throw new InvalidOperationException("Not enough reserved seats.");
        ReservedSeats -= seats;
        ConfirmedSeats += seats;
    }

    public void Release(int seats)
    {
        if (seats <= 0) throw new ArgumentOutOfRangeException(nameof(seats));
        if (ReservedSeats < seats) throw new InvalidOperationException("Not enough reserved seats.");
        ReservedSeats -= seats;
    }
}