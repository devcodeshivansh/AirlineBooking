using AirlineBooking.Application.Common.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Flights.Queries;

public sealed class SearchFlightsHandler : IRequestHandler<SearchFlightsQuery, IReadOnlyList<FlightDto>>
{
    private readonly IFlightQueryService _service;
    public SearchFlightsHandler(IFlightQueryService service) => _service = service;

    public Task<IReadOnlyList<FlightDto>> Handle(SearchFlightsQuery request, CancellationToken ct)
        => _service.SearchAsync(request.From, request.To, request.Date, ct);
}
