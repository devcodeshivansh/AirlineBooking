using AirlineBooking.Domain.Flights;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AirlineBooking.Application.Common.Interfaces;

namespace AirlineBooking.Flights.Commands
{
    public class CreateFlightCommandHandler : IRequestHandler<CreateFlightCommand, Guid>
    {
        private readonly IFlightCreateService _flightCreateService;


        public CreateFlightCommandHandler(IFlightCreateService flightService)
        {
            _flightCreateService = flightService;
        }


        public async Task<Guid> Handle(CreateFlightCommand request, CancellationToken cancellationToken)
        {
            return await _flightCreateService.CreateFlightAsync(request, cancellationToken);
        }
    }
}
