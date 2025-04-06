using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorHybridApp.Domain.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        public decimal TotalAmount { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";
        
        [MaxLength(500)]
        public string? ShippingAddress { get; set; }
        
        public DateTime? DeliveryDate { get; set; }
        
        // Navigation property
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
} 