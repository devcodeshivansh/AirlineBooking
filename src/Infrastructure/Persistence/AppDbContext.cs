using Microsoft.EntityFrameworkCore;
using AirlineBooking.Domain.Flights;
using AirlineBooking.Domain.Bookings;
using AirlineBooking.Domain.Inventory;
using System;

namespace AirlineBooking.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<SeatInventory> SeatInventories => Set<SeatInventory>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Flight>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.FlightNumber).IsRequired();
            b.Property(x => x.FromAirport).IsRequired().HasMaxLength(3);
            b.Property(x => x.ToAirport).IsRequired().HasMaxLength(3);
            b.Property(x => x.Capacity).IsRequired();
            b.Property(x => x.BaseFare).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Passenger>(b => { b.HasKey(x => x.Id); });

        modelBuilder.Entity<Booking>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Pnr).IsUnique();
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.Status).IsRequired();
        });

        modelBuilder.Entity<SeatInventory>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.FlightId).IsUnique();
        });

        modelBuilder.Entity<IdempotencyRecord>(b =>
        {
            b.HasKey(x => x.Key);
            b.Property(x => x.Key).HasMaxLength(100);
            b.Property(x => x.CreatedAtUtc);
        });
    }
}

public sealed class IdempotencyRecord
{
    public string Key { get; set; } = default!;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}