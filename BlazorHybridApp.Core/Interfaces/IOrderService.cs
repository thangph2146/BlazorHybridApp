using BlazorHybridApp.Domain.Entities;

namespace BlazorHybridApp.Core.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int id);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<Order> GetOrderWithItemsAsync(int id);
    }
} 