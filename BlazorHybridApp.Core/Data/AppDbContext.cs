using BlazorHybridApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BlazorHybridApp.Core.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>, SeedData.IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        public new async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Identity tables với tên tùy chỉnh
            modelBuilder.Entity<AppUser>().ToTable("AppUsers");
            modelBuilder.Entity<AppRole>().ToTable("AppRoles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("AppUserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("AppUserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("AppUserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("AppRoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("AppUserTokens");

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Cấu hình Order với AppUser
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

            // Cấu hình UserPermission
            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.ScopeDepartment)
                .WithMany()
                .HasForeignKey(up => up.ScopeDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

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

            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "IT", Description = "Information Technology Department", IsActive = true },
                new Department { Id = 2, Name = "HR", Description = "Human Resources Department", IsActive = true },
                new Department { Id = 3, Name = "Sales", Description = "Sales and Marketing Department", IsActive = true },
                new Department { Id = 4, Name = "Finance", Description = "Finance and Accounting Department", IsActive = true }
            );

            // Seed Permissions
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "User Management", Code = "USER", Description = "Manage users", IsActive = true },
                new Permission { Id = 2, Name = "Product Management", Code = "PRODUCT", Description = "Manage products", IsActive = true },
                new Permission { Id = 3, Name = "Order Management", Code = "ORDER", Description = "Manage orders", IsActive = true },
                new Permission { Id = 4, Name = "Report Access", Code = "REPORT", Description = "Access reports", IsActive = true },
                new Permission { Id = 5, Name = "Department Management", Code = "DEPARTMENT", Description = "Manage departments", IsActive = true }
            );

            // Seed Roles
            modelBuilder.Entity<AppRole>().HasData(
                new AppRole 
                { 
                    Id = "1", 
                    Name = "Administrator", 
                    NormalizedName = "ADMINISTRATOR", 
                    Description = "System Administrator with full access", 
                    IsActive = true 
                },
                new AppRole 
                { 
                    Id = "2", 
                    Name = "Manager", 
                    NormalizedName = "MANAGER", 
                    Description = "Department Manager with access to department data", 
                    IsActive = true 
                },
                new AppRole 
                { 
                    Id = "3", 
                    Name = "Employee", 
                    NormalizedName = "EMPLOYEE", 
                    Description = "Regular employee with limited access", 
                    IsActive = true 
                }
            );
        }
    }
} 