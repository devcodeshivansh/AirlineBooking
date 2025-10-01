using AirlineBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Bookings.Queries;

public sealed class GetBookingByPnrHandler : IRequestHandler<GetBookingByPnrQuery, BookingDto?>
{
    private readonly IBookingService _svc;
    private readonly ILogger<GetBookingByPnrHandler> _logger;
    public GetBookingByPnrHandler(IBookingService svc, ILogger<GetBookingByPnrHandler> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    public Task<BookingDto?> Handle(GetBookingByPnrQuery request, CancellationToken ct)
        => HandleInternalAsync(request, ct);

    private async Task<BookingDto?> HandleInternalAsync(GetBookingByPnrQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Handling GetBookingByPnrQuery for PNR {Pnr}", request.Pnr);
        var result = await _svc.GetByPnrAsync(request.Pnr, ct);
        if (result is null)
        {
            _logger.LogWarning("No booking found for PNR {Pnr}", request.Pnr);
        }
        else
        {
            _logger.LogInformation("Booking found for PNR {Pnr}", request.Pnr);
        }

        return result;
    }
}
