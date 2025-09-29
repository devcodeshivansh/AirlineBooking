using AirlineBooking.Application.Bookings.Commands;
using AirlineBooking.Application.Bookings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace AirlineBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IMediator mediator, ILogger<BookingsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _logger.LogInformation("Received booking creation request for flight {FlightId} with {Seats} seats", command.FlightId, command.Seats);
        var result = await _mediator.Send(command);
        _logger.LogInformation("Booking created with PNR {Pnr} and BookingId {BookingId}", result.Pnr, result.BookingId);
        return Ok(result);
    }

    [HttpPost("confirm/{pnr}")]
    public async Task<IActionResult> ConfirmBooking([FromRoute][Required][RegularExpression("^[A-Z0-9]{6}$")] string pnr)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _logger.LogInformation("Received booking confirmation request for PNR {Pnr}", pnr);
        var success = await _mediator.Send(new ConfirmBookingCommand(pnr));
        if (success)
        {
            _logger.LogInformation("Booking with PNR {Pnr} confirmed", pnr);
            return NoContent();
        }

        _logger.LogWarning("Booking with PNR {Pnr} could not be confirmed", pnr);
        return NotFound();
    }

    [HttpGet("{pnr}")]
    public async Task<IActionResult> GetBooking([FromRoute][Required][RegularExpression("^[A-Z0-9]{6}$")] string pnr)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _logger.LogInformation("Retrieving booking details for PNR {Pnr}", pnr);
        var result = await _mediator.Send(new GetBookingByPnrQuery(pnr));
        if (result is null)
        {
            _logger.LogWarning("Booking with PNR {Pnr} was not found", pnr);
            return NotFound();
        }

        _logger.LogInformation("Booking with PNR {Pnr} retrieved successfully", pnr);
        return Ok(result);
    }
}
