using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorHybridApp.Domain.Entities
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }

    public enum PermissionType
    {
        View = 1,   // Chỉ xem
        Create = 2, // Thêm mới
        Edit = 3,   // Chỉnh sửa
        Delete = 4, // Xóa
        Admin = 5   // Toàn quyền
    }
} 