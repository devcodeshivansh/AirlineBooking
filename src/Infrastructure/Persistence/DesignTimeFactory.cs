using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AirlineBooking.Infrastructure.Persistence;

public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        // Default to SQLite for migrations design-time
        builder.UseSqlite("Data Source=airlinebooking.db");
        return new AppDbContext(builder.Options);
    }
}