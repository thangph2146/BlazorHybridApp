using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorHybridApp.Domain.Entities
{
    public class UserPermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }

        [Required]
        public int PermissionId { get; set; }
        public Permission? Permission { get; set; }

        [Required]
        public PermissionType Type { get; set; } = PermissionType.View;

        // Phạm vi: null = toàn bộ, nếu có giá trị thì áp dụng cho department cụ thể
        public int? ScopeDepartmentId { get; set; }
        public Department? ScopeDepartment { get; set; }

        // Nếu true, chỉ áp dụng với dữ liệu của chính mình
        public bool IsSelfOnly { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }
} 