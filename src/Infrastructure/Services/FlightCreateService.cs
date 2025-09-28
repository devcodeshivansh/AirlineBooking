using AirlineBooking.Domain.Flights;
using AirlineBooking.Flights.Commands;
using AirlineBooking.Infrastructure.Persistence;
using System;
using AirlineBooking.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AirlineBooking.Infrastructure.Services
{
    public class FlightCreateService : IFlightCreateService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FlightCreateService> _logger;

        public FlightCreateService(AppDbContext context, ILogger<FlightCreateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Guid> CreateFlightAsync(CreateFlightCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Persisting new flight {FlightNumber} from {From} to {To}", command.FlightNumber, command.FromAirport, command.ToAirport);
            var flight = new Flight(
                command.FlightNumber,
                command.FromAirport,
                command.ToAirport,
                command.DepartureUtc,
                command.ArrivalUtc,
                command.Capacity,
                command.BaseFare);

            _context.Flights.Add(flight);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Flight {FlightNumber} persisted with Id {FlightId}", flight.FlightNumber, flight.Id);
            return flight.Id;
        }
    }
}
