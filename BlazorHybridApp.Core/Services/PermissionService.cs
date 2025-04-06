using BlazorHybridApp.Core.Data;
using BlazorHybridApp.Core.Interfaces;
using BlazorHybridApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorHybridApp.Core.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(AppDbContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permissionCode, PermissionType type = PermissionType.View)
        {
            try
            {
                // Tìm Permission dựa trên code
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Code == permissionCode && p.IsActive);

                if (permission == null)
                    return false;

                // Kiểm tra quyền của người dùng
                var user = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

                if (user == null)
                    return false;

                // Kiểm tra nếu người dùng có quyền trực tiếp
                var hasDirectPermission = await _context.UserPermissions
                    .AnyAsync(up => 
                        up.UserId == userId && 
                        up.PermissionId == permission.Id && 
                        (int)up.Type >= (int)type);

                if (hasDirectPermission)
                    return true;

                // Kiểm tra nếu người dùng có quyền thông qua role
                var userRoleIds = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (!userRoleIds.Any())
                    return false;

                var isAdmin = await _context.Roles
                    .AnyAsync(r => userRoleIds.Contains(r.Id) && r.Name == "Administrator");

                if (isAdmin)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra quyền cho user {UserId} với permission {PermissionCode}", userId, permissionCode);
                return false;
            }
        }

        public async Task<bool> HasDepartmentPermissionAsync(string userId, string permissionCode, PermissionType type, int departmentId)
        {
            try
            {
                // Tìm Permission dựa trên code
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Code == permissionCode && p.IsActive);

                if (permission == null)
                    return false;

                // Kiểm tra quyền của người dùng
                var user = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

                if (user == null)
                    return false;

                // Kiểm tra nếu người dùng có quyền trực tiếp với scope department
                var hasDirectPermission = await _context.UserPermissions
                    .AnyAsync(up => 
                        up.UserId == userId && 
                        up.PermissionId == permission.Id && 
                        (int)up.Type >= (int)type &&
                        (up.ScopeDepartmentId == null || up.ScopeDepartmentId == departmentId));

                if (hasDirectPermission)
                    return true;

                // Kiểm tra nếu người dùng là Manager trong department này
                var userRoleIds = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (!userRoleIds.Any())
                    return false;

                var isAdmin = await _context.Roles
                    .AnyAsync(r => userRoleIds.Contains(r.Id) && r.Name == "Administrator");

                if (isAdmin)
                    return true;

                var isManager = await _context.Roles
                    .AnyAsync(r => userRoleIds.Contains(r.Id) && r.Name == "Manager");

                if (isManager && user.DepartmentId == departmentId)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra quyền phòng ban cho user {UserId} với permission {PermissionCode} và departmentId {DepartmentId}", 
                    userId, permissionCode, departmentId);
                return false;
            }
        }

        public async Task<bool> HasDepartmentPermissionAsync(string userId, int departmentId, string permissionCode)
        {
            // Gọi phương thức đầy đủ với giá trị mặc định cho type
            return await HasDepartmentPermissionAsync(userId, permissionCode, PermissionType.View, departmentId);
        }

        public async Task<bool> HasSelfPermissionAsync(string userId, string permissionCode, PermissionType type, string targetUserId)
        {
            if (userId == targetUserId)
                return true;

            try
            {
                // Tìm Permission dựa trên code
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Code == permissionCode && p.IsActive);

                if (permission == null)
                    return false;

                // Kiểm tra quyền của người dùng
                var user = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

                if (user == null)
                    return false;

                // Kiểm tra target user
                var targetUser = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == targetUserId && u.IsActive);

                if (targetUser == null)
                    return false;

                // Nếu cả hai là cùng phòng ban và người dùng là Manager
                var userRoleIds = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                var isAdmin = await _context.Roles
                    .AnyAsync(r => userRoleIds.Contains(r.Id) && r.Name == "Administrator");

                if (isAdmin)
                    return true;

                var isManager = await _context.Roles
                    .AnyAsync(r => userRoleIds.Contains(r.Id) && r.Name == "Manager");

                if (isManager && user.DepartmentId.HasValue && user.DepartmentId == targetUser.DepartmentId)
                    return true;

                // Kiểm tra quyền cụ thể
                var hasDirectPermission = await _context.UserPermissions
                    .AnyAsync(up => 
                        up.UserId == userId && 
                        up.PermissionId == permission.Id && 
                        (int)up.Type >= (int)type &&
                        (up.IsSelfOnly == false || userId == targetUserId));

                return hasDirectPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra quyền self cho user {UserId} với permission {PermissionCode} và targetUserId {TargetUserId}", 
                    userId, permissionCode, targetUserId);
                return false;
            }
        }

        public async Task<bool> HasSelfPermissionAsync(string userId, string targetUserId, string permissionCode)
        {
            // Gọi phương thức đầy đủ với giá trị mặc định cho type
            return await HasSelfPermissionAsync(userId, permissionCode, PermissionType.View, targetUserId);
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            try
            {
                return await _context.Permissions
                    .Where(p => p.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách permissions");
                return Enumerable.Empty<Permission>();
            }
        }

        public async Task<UserPermission> AddUserPermissionAsync(UserPermission userPermission)
        {
            try
            {
                _context.UserPermissions.Add(userPermission);
                await _context.SaveChangesAsync();
                return userPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm quyền cho user {UserId}", userPermission.UserId);
                throw;
            }
        }

        public async Task<UserPermission> AddUserPermissionAsync(string userId, string permissionCode, PermissionType type = PermissionType.View)
        {
            try
            {
                // Tìm Permission dựa trên code
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Code == permissionCode && p.IsActive);

                if (permission == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy quyền với mã: {permissionCode}");
                }

                // Kiểm tra xem quyền đã tồn tại chưa
                var existingPermission = await _context.UserPermissions
                    .FirstOrDefaultAsync(up => 
                        up.UserId == userId && 
                        up.PermissionId == permission.Id &&
                        up.Type == type);

                if (existingPermission != null)
                {
                    // Cập nhật loại quyền nếu đã tồn tại
                    existingPermission.Type = type;
                    await _context.SaveChangesAsync();
                    return existingPermission;
                }

                // Tạo quyền mới nếu chưa tồn tại
                var userPermission = new UserPermission
                {
                    UserId = userId,
                    PermissionId = permission.Id,
                    Type = type,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserPermissions.Add(userPermission);
                await _context.SaveChangesAsync();
                return userPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm quyền {PermissionCode} cho user {UserId}", permissionCode, userId);
                throw;
            }
        }

        public async Task<bool> RemoveUserPermissionAsync(int userPermissionId)
        {
            try
            {
                var permission = await _context.UserPermissions.FindAsync(userPermissionId);
                if (permission == null)
                    return false;

                _context.UserPermissions.Remove(permission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa quyền ID {PermissionId}", userPermissionId);
                return false;
            }
        }

        public async Task<IEnumerable<UserPermission>> GetUserPermissionsAsync(string userId)
        {
            try
            {
                return await _context.UserPermissions
                    .Include(up => up.Permission)
                    .Include(up => up.ScopeDepartment)
                    .Where(up => up.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách quyền của user {UserId}", userId);
                return Enumerable.Empty<UserPermission>();
            }
        }
    }
} 