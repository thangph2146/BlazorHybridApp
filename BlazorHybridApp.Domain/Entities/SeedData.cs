using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorHybridApp.Domain.Entities
{
    public static class SeedData
    {
        // Interface để làm trung gian, tránh phụ thuộc trực tiếp
        public interface IAppDbContext
        {
            DbSet<Department> Departments { get; }
            DbSet<Permission> Permissions { get; }
            DbSet<UserPermission> UserPermissions { get; }
            DbSet<AppUser> Users { get; }
            DbSet<AppRole> Roles { get; }
            DbSet<IdentityUserRole<string>> UserRoles { get; }
            Task<int> SaveChangesAsync();
        }

        public static async Task InitializeAsync(
            IAppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            // Kiểm tra đã có dữ liệu roles chưa
            if (!await roleManager.Roles.AnyAsync())
            {
                // Tạo các roles
                await CreateRolesAsync(roleManager);
            }

            // Kiểm tra đã có dữ liệu department chưa
            if (!await context.Departments.AnyAsync())
            {
                // Tạo các department
                await CreateDepartmentsAsync(context);
            }

            // Kiểm tra đã có dữ liệu permission chưa
            if (!await context.Permissions.AnyAsync())
            {
                // Tạo các permission
                await CreatePermissionsAsync(context);
            }

            // Kiểm tra đã có dữ liệu users chưa
            if (!await userManager.Users.AnyAsync())
            {
                // Tạo admin user và các user mẫu
                await CreateUsersAsync(context, userManager, roleManager);
            }

            // Đảm bảo các thay đổi được lưu
            await context.SaveChangesAsync();
        }

        private static async Task CreateRolesAsync(RoleManager<AppRole> roleManager)
        {
            var roles = new List<AppRole>
            {
                new AppRole { Name = "Admin", Description = "Quản trị viên hệ thống", NormalizedName = "ADMIN" },
                new AppRole { Name = "Manager", Description = "Quản lý phòng ban", NormalizedName = "MANAGER" },
                new AppRole { Name = "Staff", Description = "Nhân viên", NormalizedName = "STAFF" }
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }
        }

        private static async Task CreateDepartmentsAsync(IAppDbContext context)
        {
            var departments = new List<Department>
            {
                new Department { Name = "Ban Giám đốc", Description = "Ban lãnh đạo công ty" },
                new Department { Name = "Phòng Nhân sự", Description = "Phòng quản lý nhân sự" },
                new Department { Name = "Phòng Kế toán", Description = "Phòng kế toán" },
                new Department { Name = "Phòng IT", Description = "Phòng công nghệ thông tin" },
                new Department { Name = "Phòng Kinh doanh", Description = "Phòng kinh doanh và bán hàng" }
            };

            foreach (var department in departments)
            {
                context.Departments.Add(department);
            }
            await context.SaveChangesAsync();
        }

        private static async Task CreatePermissionsAsync(IAppDbContext context)
        {
            var permissions = new List<Permission>
            {
                // Quyền hệ thống
                new Permission { Code = "system.admin", Name = "Quản trị hệ thống", Description = "Quyền quản trị toàn bộ hệ thống" },
                
                // Quyền quản lý user
                new Permission { Code = "users.view", Name = "Xem danh sách người dùng", Description = "Xem danh sách tất cả người dùng" },
                new Permission { Code = "users.create", Name = "Tạo người dùng", Description = "Tạo người dùng mới" },
                new Permission { Code = "users.edit", Name = "Sửa thông tin người dùng", Description = "Sửa thông tin người dùng" },
                new Permission { Code = "users.delete", Name = "Xóa người dùng", Description = "Xóa người dùng" },
                
                // Quyền quản lý phòng ban
                new Permission { Code = "departments.view", Name = "Xem danh sách phòng ban", Description = "Xem danh sách phòng ban" },
                new Permission { Code = "departments.create", Name = "Tạo phòng ban", Description = "Tạo phòng ban mới" },
                new Permission { Code = "departments.edit", Name = "Sửa thông tin phòng ban", Description = "Sửa thông tin phòng ban" },
                new Permission { Code = "departments.delete", Name = "Xóa phòng ban", Description = "Xóa phòng ban" },
                
                // Quyền quản lý sản phẩm
                new Permission { Code = "products.view", Name = "Xem danh sách sản phẩm", Description = "Xem danh sách sản phẩm" },
                new Permission { Code = "products.create", Name = "Tạo sản phẩm", Description = "Tạo sản phẩm mới" },
                new Permission { Code = "products.edit", Name = "Sửa thông tin sản phẩm", Description = "Sửa thông tin sản phẩm" },
                new Permission { Code = "products.delete", Name = "Xóa sản phẩm", Description = "Xóa sản phẩm" },
                
                // Quyền quản lý danh mục
                new Permission { Code = "categories.view", Name = "Xem danh sách danh mục", Description = "Xem danh sách danh mục" },
                new Permission { Code = "categories.create", Name = "Tạo danh mục", Description = "Tạo danh mục mới" },
                new Permission { Code = "categories.edit", Name = "Sửa thông tin danh mục", Description = "Sửa thông tin danh mục" },
                new Permission { Code = "categories.delete", Name = "Xóa danh mục", Description = "Xóa danh mục" },
                
                // Quyền quản lý đơn hàng
                new Permission { Code = "orders.view", Name = "Xem danh sách đơn hàng", Description = "Xem danh sách đơn hàng" },
                new Permission { Code = "orders.create", Name = "Tạo đơn hàng", Description = "Tạo đơn hàng mới" },
                new Permission { Code = "orders.edit", Name = "Sửa thông tin đơn hàng", Description = "Sửa thông tin đơn hàng" },
                new Permission { Code = "orders.delete", Name = "Xóa đơn hàng", Description = "Xóa đơn hàng" },
                
                // Quyền tự quản lý
                new Permission { Code = "self.view", Name = "Xem thông tin cá nhân", Description = "Xem thông tin cá nhân của mình" },
                new Permission { Code = "self.edit", Name = "Sửa thông tin cá nhân", Description = "Sửa thông tin cá nhân của mình" }
            };

            foreach (var permission in permissions)
            {
                context.Permissions.Add(permission);
            }
            await context.SaveChangesAsync();
        }

        private static async Task CreateUsersAsync(
            IAppDbContext context, 
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            // Lấy danh sách department
            var itDepartment = await context.Departments
                .FirstOrDefaultAsync(d => d.Name == "Phòng IT");
            var hrDepartment = await context.Departments
                .FirstOrDefaultAsync(d => d.Name == "Phòng Nhân sự");
            var managementDepartment = await context.Departments
                .FirstOrDefaultAsync(d => d.Name == "Ban Giám đốc");

            if (itDepartment == null || hrDepartment == null || managementDepartment == null)
            {
                throw new InvalidOperationException("Không tìm thấy phòng ban cần thiết");
            }

            // Tạo admin user
            var adminUser = new AppUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                PhoneNumber = "0123456789",
                EmailConfirmed = true,
                DepartmentId = managementDepartment.Id
            };

            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Admin");

            // Tạo manager user
            var managerUser = new AppUser
            {
                UserName = "manager@example.com",
                Email = "manager@example.com",
                FirstName = "Manager",
                LastName = "User",
                PhoneNumber = "0123456788",
                EmailConfirmed = true,
                DepartmentId = itDepartment.Id
            };

            await userManager.CreateAsync(managerUser, "Manager@123");
            await userManager.AddToRoleAsync(managerUser, "Manager");

            // Tạo staff user
            var staffUser = new AppUser
            {
                UserName = "staff@example.com",
                Email = "staff@example.com",
                FirstName = "Staff",
                LastName = "User",
                PhoneNumber = "0123456787",
                EmailConfirmed = true,
                DepartmentId = hrDepartment.Id
            };

            await userManager.CreateAsync(staffUser, "Staff@123");
            await userManager.AddToRoleAsync(staffUser, "Staff");

            // Lấy các permission
            var allPermissions = await context.Permissions.ToListAsync();
            var systemAdminPermission = allPermissions.FirstOrDefault(p => p.Code == "system.admin");
            var viewUserPermission = allPermissions.FirstOrDefault(p => p.Code == "users.view");
            var selfViewPermission = allPermissions.FirstOrDefault(p => p.Code == "self.view");
            var selfEditPermission = allPermissions.FirstOrDefault(p => p.Code == "self.edit");

            if (systemAdminPermission == null || viewUserPermission == null || 
                selfViewPermission == null || selfEditPermission == null)
            {
                throw new InvalidOperationException("Không tìm thấy quyền cần thiết");
            }

            // Gán quyền cho users
            context.UserPermissions.Add(new UserPermission { UserId = adminUser.Id, PermissionId = systemAdminPermission.Id });
            context.UserPermissions.Add(new UserPermission { UserId = managerUser.Id, PermissionId = viewUserPermission.Id });
            context.UserPermissions.Add(new UserPermission { UserId = managerUser.Id, PermissionId = selfViewPermission.Id });
            context.UserPermissions.Add(new UserPermission { UserId = managerUser.Id, PermissionId = selfEditPermission.Id });
            context.UserPermissions.Add(new UserPermission { UserId = staffUser.Id, PermissionId = selfViewPermission.Id });
            context.UserPermissions.Add(new UserPermission { UserId = staffUser.Id, PermissionId = selfEditPermission.Id });

            await context.SaveChangesAsync();
        }
    }
} 