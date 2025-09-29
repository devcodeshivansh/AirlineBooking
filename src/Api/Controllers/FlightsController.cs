using AirlineBooking.Application.Flights.Queries;
using AirlineBooking.Flights.Commands;
using AirlineBooking.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace AirlineBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(IMediator mediator, ILogger<FlightsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchFlights(
        [FromQuery][Required][StringLength(3, MinimumLength = 3)] string from,
        [FromQuery][Required][StringLength(3, MinimumLength = 3)] string to,
        [FromQuery][Required] DateTime date)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _logger.LogInformation("Searching flights from {From} to {To} on {Date}", from, to, date);
        var result = await _mediator.Send(new SearchFlightsQuery(from, to, date));
        _logger.LogInformation("Found {Count} flights from {From} to {To} on {Date}", result.Count, from, to, date);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFlight([FromBody] CreateFlightCommand command)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _logger.LogInformation("Creating flight {FlightNumber} from {From} to {To} on {DepartureUtc}", command.FlightNumber, command.FromAirport, command.ToAirport, command.DepartureUtc);
        var id = await _mediator.Send(command);
        _logger.LogInformation("Flight {FlightNumber} created with Id {FlightId}", command.FlightNumber, id);
        return CreatedAtAction(nameof(GetFlightById), new { id }, null);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFlightById([FromServices] AppDbContext db, Guid id)
    {
        if (id == Guid.Empty)
        {
            return ValidationProblem(new ValidationProblemDetails
            {
                Errors = { [nameof(id)] = new[] { "Flight identifier cannot be empty." } }
            });
        }

        _logger.LogInformation("Retrieving flight with Id {FlightId}", id);
        var flight = await db.Flights.FindAsync(id);
        if (flight is null)
        {
            _logger.LogWarning("Flight with Id {FlightId} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Flight with Id {FlightId} retrieved successfully", id);
        return Ok(flight);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFlights([FromServices] AppDbContext db)
    {
        _logger.LogInformation("Retrieving all flights");
        var flights = await db.Flights.ToListAsync();
        _logger.LogInformation("Retrieved {Count} flights", flights.Count);
        return Ok(flights);
    }
}
