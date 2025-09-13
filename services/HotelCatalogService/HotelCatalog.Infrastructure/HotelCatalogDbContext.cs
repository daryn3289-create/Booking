using HotelCatalog.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelCatalog.Infrastructure;

public class HotelCatalogDbContext : DbContext
{
    public HotelCatalogDbContext(DbContextOptions<HotelCatalogDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>()
            .ComplexProperty(x => x.Address, c =>
            {
                c.Property(a => a.Street).HasMaxLength(100);
                c.Property(a => a.City).HasMaxLength(50);
                c.Property(a => a.Country).HasMaxLength(50);
            });
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<HotelPhoto> HotelPhotos { get; set; }
    public DbSet<HotelDetailsReadModel> HotelDetailsReadModels { get; set; }
    public DbSet<RoomReadModel> RoomReadModels { get; set; }
}