using AirlineBooking.Application.Flights.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AirlineBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FlightsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchFlights([FromQuery] string from, [FromQuery] string to, [FromQuery] DateTime date)
    {
        var result = await _mediator.Send(new SearchFlightsQuery(from, to, date));
        return Ok(result);
    }
}
