using DominionWarehouseAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DominionWarehouseAPI.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Warehouse> Warehouse { get; set; }
        public DbSet<Roles> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity to map to a specific table (if required)
            modelBuilder.Entity<User>().ToTable("Users")
                .HasOne(u => u.Role)
                .WithOne()
                .HasForeignKey<User>(u => u.RoleId);

            // Other entity configurations (if any)
            modelBuilder.Entity<Warehouse>()
                .ToTable("Warehouse")
                .HasOne<User>(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.userId)
                .OnDelete(DeleteBehavior.Cascade);

        }

    }
}
