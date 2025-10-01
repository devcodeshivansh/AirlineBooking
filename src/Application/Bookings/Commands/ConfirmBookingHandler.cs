using AirlineBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Bookings.Commands;

public sealed class ConfirmBookingHandler : IRequestHandler<ConfirmBookingCommand, bool>
{
    private readonly IBookingService _svc;
    private readonly ILogger<ConfirmBookingHandler> _logger;
    public ConfirmBookingHandler(IBookingService svc, ILogger<ConfirmBookingHandler> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    public Task<bool> Handle(ConfirmBookingCommand request, CancellationToken ct)
        => HandleInternalAsync(request, ct);

    private async Task<bool> HandleInternalAsync(ConfirmBookingCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Handling ConfirmBookingCommand for PNR {Pnr}", request.Pnr);
        var success = await _svc.ConfirmBookingAsync(request.Pnr, ct);
        if (success)
        {
            _logger.LogInformation("Booking with PNR {Pnr} confirmed successfully", request.Pnr);
        }
        else
        {
            _logger.LogWarning("Booking with PNR {Pnr} could not be confirmed", request.Pnr);
        }

        return success;
    }
}
