using AirlineBooking.Application.Common.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Bookings.Commands;

public sealed class CreateBookingHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
{
    private readonly IBookingService _svc;
    public CreateBookingHandler(IBookingService svc) => _svc = svc;

    public Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken ct)
        => _svc.CreateBookingAsync(request, ct);
}
