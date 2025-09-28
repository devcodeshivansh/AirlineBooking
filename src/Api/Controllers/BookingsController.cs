using AirlineBooking.Application.Bookings.Commands;
using AirlineBooking.Application.Bookings.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AirlineBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("confirm/{pnr}")]
    public async Task<IActionResult> ConfirmBooking(string pnr)
    {
        var success = await _mediator.Send(new ConfirmBookingCommand(pnr));
        return success ? NoContent() : NotFound();
    }

    [HttpGet("{pnr}")]
    public async Task<IActionResult> GetBooking(string pnr)
    {
        var result = await _mediator.Send(new GetBookingByPnrQuery(pnr));
        return result is null ? NotFound() : Ok(result);
    }
}
