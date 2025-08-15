using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.Models;

namespace MyApp.Persistence
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Orders> Orders {get;set;}
        public DbSet<OrderDetails> OrderDetails {get;set;}
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItems> CartItems { get; set; }
        public DbSet<WishList> WishLists { get; set; }
        public DbSet<WishlistItems> WishlistItems { get; set; }
          public DbSet<Home> Home { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public AppDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    modelBuilder.Entity<User>(entity =>
    {
        entity.Property(u => u.FirstName).HasMaxLength(50);
        entity.Property(u => u.LastName).HasMaxLength(50);
        entity.Property(u => u.PhoneNumber).HasMaxLength(15);
        entity.Property(u => u.Country).HasMaxLength(50);
        entity.Property(u => u.City).HasMaxLength(50);
    });

    modelBuilder.Entity<Product>()
        .Property(p => p.Price)
        .HasColumnType("decimal(18,2)");

    modelBuilder.Entity<OrderDetails>()
        .Property(o => o.Price)
        .HasColumnType("decimal(18,2)");

    modelBuilder.Entity<Orders>()
        .Property(o => o.TotalAmount)
        .HasColumnType("decimal(18,2)");
}

    }
}
