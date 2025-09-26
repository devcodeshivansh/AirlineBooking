using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Common.Interfaces;

public interface IIdempotencyStore
{
    Task<bool> ExistsAsync(string key, CancellationToken ct);
    Task MarkAsync(string key, CancellationToken ct);
}