using Microsoft.EntityFrameworkCore;
using Profile.Domain;

namespace Profile.Infrastructure;

public class ProfileDbContext : DbContext
{
    public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("profile");
        base.OnModelCreating(modelBuilder);
    }
}