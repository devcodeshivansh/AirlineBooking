using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineBooking.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AirlineBooking.Flights.Commands
{
    public class CreateFlightCommandHandler : IRequestHandler<CreateFlightCommand, Guid>
    {
        private readonly IFlightCreateService _flightCreateService;
        private readonly ILogger<CreateFlightCommandHandler> _logger;


        public CreateFlightCommandHandler(IFlightCreateService flightService, ILogger<CreateFlightCommandHandler> logger)
        {
            _flightCreateService = flightService;
            _logger = logger;
        }


        public async Task<Guid> Handle(CreateFlightCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CreateFlightCommand for flight {FlightNumber}", request.FlightNumber);
            var id = await _flightCreateService.CreateFlightAsync(request, cancellationToken);
            _logger.LogInformation("Flight {FlightNumber} created with Id {FlightId}", request.FlightNumber, id);
            return id;
        }
    }
}
