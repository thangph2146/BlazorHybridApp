using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorHybridApp.Domain.Entities
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
} 