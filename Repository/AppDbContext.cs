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

        public DbSet<Category> Categories { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductsInWarehouse> ProductsInWarehouses { get; set; }

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

            //add order user relation
            modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            // Other entity configurations (if any)
            modelBuilder.Entity<Warehouse>()
                .ToTable("Warehouse")
                .HasOne<User>(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.userId)
                .OnDelete(DeleteBehavior.NoAction);

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

            //prodsinwarehouse

            modelBuilder.Entity<ProductsInWarehouse>()
                .HasKey(wp => new { wp.WarehouseId, wp.ProductId });

            modelBuilder.Entity<ProductsInWarehouse>()
                .HasOne(wp => wp.Warehouse)
                .WithMany(w => w.WarehouseProducts)
                .HasForeignKey(wp => wp.WarehouseId);

            modelBuilder.Entity<ProductsInWarehouse>()
                .HasOne(wp => wp.Product)
                .WithMany(p => p.WarehouseProducts)
                .HasForeignKey(wp => wp.ProductId);
            
            //category
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            //testing

            modelBuilder.Entity<Order>()
            .HasOne(o => o.ShoppingCart)
            .WithMany(sc => sc.Orders)
            .HasForeignKey(o => o.ShoppingCartId)
            .OnDelete(DeleteBehavior.Restrict);


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
