using AirlineBooking.Flights.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Common.Interfaces
{
    public interface IFlightCreateService
    {
        Task<Guid> CreateFlightAsync(CreateFlightCommand command, CancellationToken cancellationToken);
    }
}
