using BlazorHybridApp.Domain.Entities;

namespace BlazorHybridApp.Core.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<AppUser>> GetAllUsersAsync();
        Task<AppUser?> GetUserByIdAsync(string id);
        Task<AppUser?> GetUserByEmailAsync(string email);
        Task<AppUser> CreateUserAsync(AppUser user, string password);
        Task<AppUser> UpdateUserAsync(AppUser user, string password = null);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> AuthenticateUserAsync(string email, string password);
    }
} 