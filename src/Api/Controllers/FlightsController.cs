using AirlineBooking.Application.Flights.Queries;
using AirlineBooking.Flights.Commands;
using AirlineBooking.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [HttpPost]
    public async Task<IActionResult> CreateFlight([FromBody] CreateFlightCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetFlightById), new { id }, null);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFlightById([FromServices] AppDbContext db, Guid id)
    {
        var flight = await db.Flights.FindAsync(id);
        return flight is null ? NotFound() : Ok(flight);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFlights([FromServices] AppDbContext db)
    {
        var flights = await db.Flights.ToListAsync();
        return Ok(flights);
    }
}
