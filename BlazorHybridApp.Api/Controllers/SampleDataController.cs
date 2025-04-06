using BlazorHybridApp.Core.Data;
using BlazorHybridApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorHybridApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleDataController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SampleDataController> _logger;

        public SampleDataController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<SampleDataController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateSampleData()
        {
            try
            {
                // Kiểm tra xem đã có dữ liệu mẫu chưa
                if (await _context.Users.AnyAsync())
                {
                    // Xóa dữ liệu cũ
                    await ClearExistingData();
                }

                // Tạo vai trò
                await CreateRoles();

                // Tạo phòng ban
                var departments = await CreateDepartments();

                // Tạo người dùng
                var users = await CreateUsers(departments);

                // Tạo quyền
                var permissions = await CreatePermissions();

                // Gán quyền cho người dùng
                await AssignPermissionsToUsers(users, permissions);

                return Ok(new { message = "Dữ liệu mẫu đã được tạo thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo dữ liệu mẫu");
                return StatusCode(500, new { message = $"Lỗi khi tạo dữ liệu mẫu: {ex.Message}" });
            }
        }

        private async Task ClearExistingData()
        {
            // Xóa user permissions
            _context.UserPermissions.RemoveRange(await _context.UserPermissions.ToListAsync());
            
            // Xóa users (không phải admin)
            var nonAdminUsers = await _userManager.Users
                .Where(u => u.UserName != "admin@example.com")
                .ToListAsync();
                
            foreach (var user in nonAdminUsers)
            {
                await _userManager.DeleteAsync(user);
            }
            
            // Xóa permissions
            _context.Permissions.RemoveRange(await _context.Permissions.ToListAsync());
            
            // Xóa departments
            _context.Departments.RemoveRange(await _context.Departments.ToListAsync());
            
            await _context.SaveChangesAsync();
        }

        private async Task CreateRoles()
        {
            string[] roleNames = { "Admin", "Manager", "User" };
            
            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private async Task<List<Department>> CreateDepartments()
        {
            var departments = new List<Department>
            {
                new Department { 
                    Name = "Ban Giám đốc", 
                    Description = "Ban lãnh đạo công ty", 
                    IsActive = true, 
                    CreatedAt = DateTime.Now 
                },
                new Department { 
                    Name = "Phòng Nhân sự", 
                    Description = "Quản lý nhân sự và tuyển dụng", 
                    IsActive = true, 
                    CreatedAt = DateTime.Now 
                },
                new Department { 
                    Name = "Phòng Kế toán", 
                    Description = "Quản lý tài chính và kế toán", 
                    IsActive = true, 
                    CreatedAt = DateTime.Now 
                },
                new Department { 
                    Name = "Phòng IT", 
                    Description = "Quản lý hệ thống công nghệ thông tin", 
                    IsActive = true, 
                    CreatedAt = DateTime.Now 
                },
                new Department { 
                    Name = "Phòng Marketing", 
                    Description = "Quản lý marketing và truyền thông", 
                    IsActive = true, 
                    CreatedAt = DateTime.Now 
                }
            };

            _context.Departments.AddRange(departments);
            await _context.SaveChangesAsync();
            
            return departments;
        }

        private async Task<List<AppUser>> CreateUsers(List<Department> departments)
        {
            // Tạo danh sách người dùng mẫu
            var users = new List<(AppUser User, string Password, string[] Roles)>
            {
                (
                    new AppUser { 
                        UserName = "admin@example.com", 
                        Email = "admin@example.com",
                        FirstName = "Admin",
                        LastName = "User",
                        IsActive = true,
                        DepartmentId = departments[0].Id,
                        CreatedAt = DateTime.Now
                    },
                    "Admin@123",
                    new[] { "Admin" }
                ),
                (
                    new AppUser { 
                        UserName = "manager@example.com", 
                        Email = "manager@example.com",
                        FirstName = "Manager",
                        LastName = "User",
                        IsActive = true,
                        DepartmentId = departments[1].Id,
                        CreatedAt = DateTime.Now
                    },
                    "Manager@123",
                    new[] { "Manager" }
                ),
                (
                    new AppUser { 
                        UserName = "user1@example.com", 
                        Email = "user1@example.com",
                        FirstName = "User",
                        LastName = "One",
                        IsActive = true,
                        DepartmentId = departments[2].Id,
                        CreatedAt = DateTime.Now
                    },
                    "User@123",
                    new[] { "User" }
                ),
                (
                    new AppUser { 
                        UserName = "user2@example.com", 
                        Email = "user2@example.com",
                        FirstName = "User",
                        LastName = "Two",
                        IsActive = true,
                        DepartmentId = departments[3].Id,
                        CreatedAt = DateTime.Now
                    },
                    "User@123",
                    new[] { "User" }
                ),
                (
                    new AppUser { 
                        UserName = "user3@example.com", 
                        Email = "user3@example.com",
                        FirstName = "User",
                        LastName = "Three",
                        IsActive = true,
                        DepartmentId = departments[4].Id,
                        CreatedAt = DateTime.Now
                    },
                    "User@123",
                    new[] { "User" }
                ),
                (
                    new AppUser { 
                        UserName = "user4@example.com", 
                        Email = "user4@example.com",
                        FirstName = "User",
                        LastName = "Four",
                        IsActive = false,
                        DepartmentId = departments[4].Id,
                        CreatedAt = DateTime.Now
                    },
                    "User@123",
                    new[] { "User" }
                )
            };

            var createdUsers = new List<AppUser>();

            foreach (var (user, password, roles) in users)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser == null)
                {
                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        foreach (var role in roles)
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                        createdUsers.Add(user);
                    }
                }
                else
                {
                    createdUsers.Add(existingUser);
                }
            }

            return createdUsers;
        }

        private async Task<List<Permission>> CreatePermissions()
        {
            var permissions = new List<Permission>
            {
                // Quyền phòng ban
                new Permission { 
                    Name = "Xem danh sách phòng ban", 
                    Code = "departments.view", 
                    Description = "Quyền xem danh sách phòng ban",
                    IsActive = true
                },
                new Permission { 
                    Name = "Tạo phòng ban", 
                    Code = "departments.create", 
                    Description = "Quyền tạo phòng ban mới",
                    IsActive = true
                },
                new Permission { 
                    Name = "Chỉnh sửa phòng ban", 
                    Code = "departments.edit", 
                    Description = "Quyền chỉnh sửa thông tin phòng ban",
                    IsActive = true
                },
                new Permission { 
                    Name = "Xóa phòng ban", 
                    Code = "departments.delete", 
                    Description = "Quyền xóa phòng ban",
                    IsActive = true
                },
                
                // Quyền người dùng
                new Permission { 
                    Name = "Xem danh sách người dùng", 
                    Code = "users.view", 
                    Description = "Quyền xem danh sách người dùng",
                    IsActive = true
                },
                new Permission { 
                    Name = "Tạo người dùng", 
                    Code = "users.create", 
                    Description = "Quyền tạo người dùng mới",
                    IsActive = true
                },
                new Permission { 
                    Name = "Chỉnh sửa người dùng", 
                    Code = "users.edit", 
                    Description = "Quyền chỉnh sửa thông tin người dùng",
                    IsActive = true
                },
                new Permission { 
                    Name = "Xóa người dùng", 
                    Code = "users.delete", 
                    Description = "Quyền xóa người dùng",
                    IsActive = true
                },
                
                // Quyền phân quyền
                new Permission { 
                    Name = "Xem danh sách quyền", 
                    Code = "permissions.view", 
                    Description = "Quyền xem danh sách quyền trong hệ thống",
                    IsActive = true
                },
                new Permission { 
                    Name = "Tạo quyền", 
                    Code = "permissions.create", 
                    Description = "Quyền tạo quyền mới",
                    IsActive = true
                },
                new Permission { 
                    Name = "Chỉnh sửa quyền", 
                    Code = "permissions.edit", 
                    Description = "Quyền chỉnh sửa thông tin quyền",
                    IsActive = true
                },
                new Permission { 
                    Name = "Xóa quyền", 
                    Code = "permissions.delete", 
                    Description = "Quyền xóa quyền",
                    IsActive = true
                },
                
                // Quyền báo cáo
                new Permission { 
                    Name = "Xem báo cáo", 
                    Code = "reports.view", 
                    Description = "Quyền xem báo cáo",
                    IsActive = true
                },
                new Permission { 
                    Name = "Xuất báo cáo", 
                    Code = "reports.export", 
                    Description = "Quyền xuất báo cáo",
                    IsActive = true
                }
            };

            _context.Permissions.AddRange(permissions);
            await _context.SaveChangesAsync();
            
            return permissions;
        }

        private async Task AssignPermissionsToUsers(List<AppUser> users, List<Permission> permissions)
        {
            // Tìm người dùng theo email
            var admin = users.FirstOrDefault(u => u.Email == "admin@example.com");
            var manager = users.FirstOrDefault(u => u.Email == "manager@example.com");
            var user1 = users.FirstOrDefault(u => u.Email == "user1@example.com");
            var user2 = users.FirstOrDefault(u => u.Email == "user2@example.com");
            
            if (admin != null)
            {
                // Admin có tất cả các quyền với phạm vi toàn bộ
                foreach (var permission in permissions)
                {
                    var userPermission = new UserPermission
                    {
                        UserId = admin.Id,
                        PermissionId = permission.Id,
                        Type = PermissionType.Admin,
                        IsSelfOnly = false,
                        ScopeDepartmentId = null,
                        CreatedAt = DateTime.Now
                    };
                    _context.UserPermissions.Add(userPermission);
                }
            }
            
            if (manager != null)
            {
                // Manager có quyền xem với tất cả các đối tượng
                foreach (var permission in permissions.Where(p => p.Code.EndsWith(".view")))
                {
                    var userPermission = new UserPermission
                    {
                        UserId = manager.Id,
                        PermissionId = permission.Id,
                        Type = PermissionType.View,
                        IsSelfOnly = false,
                        ScopeDepartmentId = null,
                        CreatedAt = DateTime.Now
                    };
                    _context.UserPermissions.Add(userPermission);
                }
                
                // Manager có quyền chỉnh sửa trong phòng ban của mình
                foreach (var permission in permissions.Where(p => p.Code.EndsWith(".edit")))
                {
                    var userPermission = new UserPermission
                    {
                        UserId = manager.Id,
                        PermissionId = permission.Id,
                        Type = PermissionType.Edit,
                        IsSelfOnly = false,
                        ScopeDepartmentId = manager.DepartmentId,
                        CreatedAt = DateTime.Now
                    };
                    _context.UserPermissions.Add(userPermission);
                }
                
                // Manager có quyền xem báo cáo
                var reportViewPermission = permissions.FirstOrDefault(p => p.Code == "reports.view");
                if (reportViewPermission != null)
                {
                    var userPermission = new UserPermission
                    {
                        UserId = manager.Id,
                        PermissionId = reportViewPermission.Id,
                        Type = PermissionType.View,
                        IsSelfOnly = false,
                        ScopeDepartmentId = null,
                        CreatedAt = DateTime.Now
                    };
                    _context.UserPermissions.Add(userPermission);
                }
            }
            
            if (user1 != null)
            {
                // User1 có quyền xem người dùng trong phòng ban của mình
                var userViewPermission = permissions.FirstOrDefault(p => p.Code == "users.view");
                if (userViewPermission != null)
                {
                    var userPermission = new UserPermission
                    {
                        UserId = user1.Id,
                        PermissionId = userViewPermission.Id,
                        Type = PermissionType.View,
                        IsSelfOnly = false,
                        ScopeDepartmentId = user1.DepartmentId,
                        CreatedAt = DateTime.Now
                    };
                    _context.UserPermissions.Add(userPermission);
                }
                
                // User1 có quyền xem danh sách phòng ban
                var deptViewPermission = permissions.FirstOrDefault(p => p.Code == "departments.view");
                if (deptViewPermission != null)
                {
                    var userPermission = new UserPermission
                    {
                        UserId = user1.Id,
                        PermissionId = deptViewPermission.Id,
                        Type = PermissionType.View,
                        IsSelfOnly = false,
                        ScopeDepartmentId = null,
                        CreatedAt = DateTime.Now
                    };
                    _context.UserPermissions.Add(userPermission);
                }
            }
            
            if (user2 != null)
            {
                // User2 có quyền xem người dùng và chỉnh sửa chính mình
                var userViewPermission = permissions.FirstOrDefault(p => p.Code == "users.view");
                if (userViewPermission != null)
                {
                    var userPermission = new UserPermission
                    {
                        UserId = user2.Id,
                        PermissionId = userViewPermission.Id,
                        Type = PermissionType.View,
                        IsSelfOnly = false,
                        ScopeDepartmentId = null,
                        CreatedAt = DateTime.Now
                    };
                    _context.UserPermissions.Add(userPermission);
                }
                
                var userEditPermission = permissions.FirstOrDefault(p => p.Code == "users.edit");
                if (userEditPermission != null)
                {
                    var userPermission = new UserPermission
                    {
                        UserId = user2.Id,
                        PermissionId = userEditPermission.Id,
                        Type = PermissionType.Edit,
                        IsSelfOnly = true,
                        ScopeDepartmentId = null,
                        CreatedAt = DateTime.Now
                    };
                    _context.UserPermissions.Add(userPermission);
                }
            }
            
            await _context.SaveChangesAsync();
        }
    }
} 