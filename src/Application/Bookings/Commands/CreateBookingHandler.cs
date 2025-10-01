using AirlineBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Bookings.Commands;

public sealed class CreateBookingHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
{
    private readonly IBookingService _svc;
    private readonly ILogger<CreateBookingHandler> _logger;
    public CreateBookingHandler(IBookingService svc, ILogger<CreateBookingHandler> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    public Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Handling CreateBookingCommand for flight {FlightId} with {Seats} seats", request.FlightId, request.Seats);
        return HandleInternalAsync(request, ct);
    }

    private async Task<CreateBookingResult> HandleInternalAsync(CreateBookingCommand request, CancellationToken ct)
    {
        var result = await _svc.CreateBookingAsync(request, ct);
        _logger.LogInformation("Booking created with BookingId {BookingId} and PNR {Pnr}", result.BookingId, result.Pnr);
        return result;
    }
}
