using AirlineBooking.Domain.Flights;
using AirlineBooking.Flights.Commands;
using AirlineBooking.Infrastructure.Persistence;
using System;
using AirlineBooking.Application.Common.Interfaces;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Infrastructure.Services
{

    public class FlightCreateService : IFlightCreateService
    {
        private readonly AppDbContext _context;


        public FlightCreateService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<Guid> CreateFlightAsync(CreateFlightCommand command, CancellationToken cancellationToken)
        {
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
            return flight.Id;
        }
    }
}
