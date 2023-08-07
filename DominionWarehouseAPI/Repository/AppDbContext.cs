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

        public DbSet<ShoppingCart> ShoppingCart { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductsInShoppingCart> ProductsInShoppingCarts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity to map to a specific table (if required)
            modelBuilder.Entity<User>().ToTable("Users")
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<User>()
            .HasOne(u => u.ShoppingCart)
            .WithOne(sc => sc.User)
            .HasForeignKey<ShoppingCart>(sc => sc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            // Other entity configurations (if any)
            modelBuilder.Entity<Warehouse>()
                .ToTable("Warehouse")
                .HasOne<User>(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.userId)
                .OnDelete(DeleteBehavior.Cascade);

            //many-to-many with junction table
            modelBuilder.Entity<ProductsInShoppingCart>()
            .HasKey(psc => new { psc.ProductId, psc.ShoppingCartId });

            modelBuilder.Entity<ProductsInShoppingCart>()
                .HasOne(psc => psc.Product)
                .WithMany(p => p.ProductShoppingCarts)
                .HasForeignKey(psc => psc.ProductId);

            modelBuilder.Entity<ProductsInShoppingCart>()
                .HasOne(psc => psc.ShoppingCart)
                .WithMany(sc => sc.ProductShoppingCarts)
                .HasForeignKey(psc => psc.ShoppingCartId);

        }

        public override int SaveChanges()
        {
            // Check for new users being added
            var addedUsers = ChangeTracker.Entries<User>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .ToList();

            foreach (var user in addedUsers)
            {

                if (user.ShoppingCart == null)
                {
                    var newShoppingCart = new ShoppingCart();
                    user.ShoppingCart = newShoppingCart;
                    base.SaveChanges();
                    user.ShoppingCartId = newShoppingCart.Id;
                }
            }
            return base.SaveChanges();
        }

    }

}
