using AirlineBooking.Application.Bookings.Commands;
using AirlineBooking.Application.Bookings.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Common.Interfaces;

public interface IBookingService
{
    Task<CreateBookingResult> CreateBookingAsync(CreateBookingCommand cmd, CancellationToken ct);
    Task<bool> ConfirmBookingAsync(string pnr, CancellationToken ct);
    Task<BookingDto?> GetByPnrAsync(string pnr, CancellationToken ct);
}
