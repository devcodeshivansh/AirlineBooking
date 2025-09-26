using AirlineBooking.Application.Common.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Bookings.Commands;

public sealed class ConfirmBookingHandler : IRequestHandler<ConfirmBookingCommand, bool>
{
    private readonly IBookingService _svc;
    public ConfirmBookingHandler(IBookingService svc) => _svc = svc;

    public Task<bool> Handle(ConfirmBookingCommand request, CancellationToken ct)
        => _svc.ConfirmBookingAsync(request.Pnr, ct);
}
