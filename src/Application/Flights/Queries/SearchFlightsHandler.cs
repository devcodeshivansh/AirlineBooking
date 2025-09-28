using AirlineBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Flights.Queries;

public sealed class SearchFlightsHandler : IRequestHandler<SearchFlightsQuery, IReadOnlyList<FlightDto>>
{
    private readonly IFlightQueryService _service;
    private readonly ILogger<SearchFlightsHandler> _logger;
    public SearchFlightsHandler(IFlightQueryService service, ILogger<SearchFlightsHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public Task<IReadOnlyList<FlightDto>> Handle(SearchFlightsQuery request, CancellationToken ct)
        => HandleInternalAsync(request, ct);

    private async Task<IReadOnlyList<FlightDto>> HandleInternalAsync(SearchFlightsQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Handling SearchFlightsQuery from {From} to {To} on {Date}", request.From, request.To, request.Date);
        var results = await _service.SearchAsync(request.From, request.To, request.Date, ct);
        _logger.LogInformation("SearchFlightsQuery returned {Count} flights", results.Count);
        return results;
    }
}
