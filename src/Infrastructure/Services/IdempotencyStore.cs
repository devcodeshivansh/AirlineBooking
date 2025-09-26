using AirlineBooking.Application.Common.Interfaces;
using AirlineBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Infrastructure.Services;

public sealed class IdempotencyStore : IIdempotencyStore
{
    private readonly AppDbContext _db;
    public IdempotencyStore(AppDbContext db) => _db = db;

    public async Task<bool> ExistsAsync(string key, CancellationToken ct)
        => await _db.IdempotencyRecords.AnyAsync(x => x.Key == key, ct);

    public async Task MarkAsync(string key, CancellationToken ct)
    {
        if (await ExistsAsync(key, ct)) return;
        _db.IdempotencyRecords.Add(new IdempotencyRecord { Key = key });
        await _db.SaveChangesAsync(ct);
    }
}