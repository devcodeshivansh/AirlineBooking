using System.Threading;
using System.Threading.Tasks;

namespace AirlineBooking.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}