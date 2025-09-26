using AirlineBooking.Application.Common.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Bookings.Queries;

public sealed class GetBookingByPnrHandler : IRequestHandler<GetBookingByPnrQuery, BookingDto?>
{
    private readonly IBookingService _svc;
    public GetBookingByPnrHandler(IBookingService svc) => _svc = svc;

    public Task<BookingDto?> Handle(GetBookingByPnrQuery request, CancellationToken ct)
        => _svc.GetByPnrAsync(request.Pnr, ct);
}
