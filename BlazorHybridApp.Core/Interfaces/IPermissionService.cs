using BlazorHybridApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorHybridApp.Core.Interfaces
{
    public interface IPermissionService
    {
        /// <summary>
        /// Kiểm tra nếu người dùng có quyền cụ thể
        /// </summary>
        Task<bool> HasPermissionAsync(string userId, string permissionCode, PermissionType type = PermissionType.View);
        
        /// <summary>
        /// Kiểm tra nếu người dùng có quyền trong phạm vi phòng ban
        /// </summary>
        Task<bool> HasDepartmentPermissionAsync(string userId, string permissionCode, PermissionType type, int departmentId);
        
        /// <summary>
        /// Kiểm tra nếu người dùng có quyền trong phạm vi phòng ban - phiên bản rút gọn
        /// </summary>
        Task<bool> HasDepartmentPermissionAsync(string userId, int departmentId, string permissionCode);
        
        /// <summary>
        /// Kiểm tra nếu người dùng có quyền với dữ liệu của người dùng cụ thể
        /// </summary>
        Task<bool> HasSelfPermissionAsync(string userId, string permissionCode, PermissionType type, string targetUserId);
        
        /// <summary>
        /// Kiểm tra nếu người dùng có quyền với dữ liệu của người dùng cụ thể - phiên bản rút gọn
        /// </summary>
        Task<bool> HasSelfPermissionAsync(string userId, string targetUserId, string permissionCode);
        
        /// <summary>
        /// Lấy danh sách tất cả quyền
        /// </summary>
        Task<IEnumerable<Permission>> GetAllPermissionsAsync();
        
        /// <summary>
        /// Thêm quyền cho người dùng
        /// </summary>
        Task<UserPermission> AddUserPermissionAsync(UserPermission userPermission);
        
        /// <summary>
        /// Thêm quyền cho người dùng bằng Id và mã quyền
        /// </summary>
        Task<UserPermission> AddUserPermissionAsync(string userId, string permissionCode, PermissionType type = PermissionType.View);
        
        /// <summary>
        /// Xóa quyền người dùng
        /// </summary>
        Task<bool> RemoveUserPermissionAsync(int userPermissionId);
        
        /// <summary>
        /// Lấy danh sách quyền của người dùng
        /// </summary>
        Task<IEnumerable<UserPermission>> GetUserPermissionsAsync(string userId);
    }
} 