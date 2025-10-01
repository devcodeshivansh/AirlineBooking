using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Infrastructure.Services;

public sealed class IdempotencyStore : IIdempotencyStore
{
    private readonly AppDbContext _db;
    private readonly ILogger<IdempotencyStore> _logger;

    public IdempotencyStore(AppDbContext db, ILogger<IdempotencyStore> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct)
    {
        var exists = await _db.IdempotencyRecords.AnyAsync(x => x.Key == key, ct);
        _logger.LogInformation("Checked idempotency key {Key}. Exists: {Exists}", key, exists);
        return exists;
    }

    public async Task MarkAsync(string key, CancellationToken ct)
    {
        if (await ExistsAsync(key, ct))
        {
            _logger.LogInformation("Idempotency key {Key} already marked", key);
            return;
        }

        _db.IdempotencyRecords.Add(new IdempotencyRecord { Key = key });
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Marked idempotency key {Key}", key);
    }
}
