using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorHybridApp.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

        // Full name property that returns first and last name combined
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
} 