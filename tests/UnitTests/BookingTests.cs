using AirlineBooking.Domain.Bookings;
using FluentAssertions;
using Xunit;
using System;

namespace UnitTests;

public class BookingTests
{
    [Fact]
    public void Booking_CanBeConfirmed()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), 2, 9000, "ABC123");
        booking.Confirm();
        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.ConfirmedAtUtc.Should().NotBeNull();
    }
}