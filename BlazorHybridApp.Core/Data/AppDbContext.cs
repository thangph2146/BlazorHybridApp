using BlazorHybridApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorHybridApp.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed some data
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories", IsActive = true },
                new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion items", IsActive = true },
                new Category { Id = 3, Name = "Books", Description = "Books and publications", IsActive = true }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 1200.00m, Stock = 10, CategoryId = 1, IsActive = true },
                new Product { Id = 2, Name = "Smartphone", Description = "Latest smartphone model", Price = 800.00m, Stock = 15, CategoryId = 1, IsActive = true },
                new Product { Id = 3, Name = "T-Shirt", Description = "Cotton t-shirt", Price = 25.00m, Stock = 50, CategoryId = 2, IsActive = true },
                new Product { Id = 4, Name = "Programming Book", Description = "Learn programming", Price = 35.00m, Stock = 20, CategoryId = 3, IsActive = true }
            );
        }
    }
} 