using System;

namespace AirlineBooking.Domain.Bookings;

public sealed class Passenger
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;

    private Passenger() {}

    public Passenger(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name required");
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name required");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email required");
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
    }
}