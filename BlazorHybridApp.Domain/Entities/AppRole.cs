using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorHybridApp.Domain.Entities
{
    public class AppRole : IdentityRole
    {
        [MaxLength(200)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        public string? CreatedBy { get; set; }
    }
} 