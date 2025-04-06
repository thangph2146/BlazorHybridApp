using System.Security.Cryptography;
using System.Text;
using BlazorHybridApp.Core.Data;
using BlazorHybridApp.Core.Interfaces;
using BlazorHybridApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorHybridApp.Core.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<UserService> _logger;

        public UserService(
            AppDbContext context, 
            UserManager<AppUser> userManager,
            ILogger<UserService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
        {
            try
            {
                return await _userManager.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng");
                return Enumerable.Empty<AppUser>();
            }
        }

        public async Task<AppUser?> GetUserByIdAsync(string id)
        {
            try
            {
                return await _userManager.FindByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy người dùng id={UserId}", id);
                return null;
            }
        }

        public async Task<AppUser?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _userManager.FindByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy người dùng theo email={Email}", email);
                return null;
            }
        }

        public async Task<AppUser> CreateUserAsync(AppUser user, string password)
        {
            try
            {
                // Kiểm tra xem email đã tồn tại chưa
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                
                if (existingUser != null)
                {
                    throw new InvalidOperationException("Email đã tồn tại");
                }

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Không thể tạo người dùng: {errors}");
                }
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo người dùng mới");
                throw;
            }
        }

        public async Task<AppUser> UpdateUserAsync(AppUser user, string password = null)
        {
            try
            {
                var existingUser = await _userManager.FindByIdAsync(user.Id);
                if (existingUser == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy người dùng với ID {user.Id}");
                }

                // Cập nhật thông tin người dùng
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;
                existingUser.UserName = user.Email; // Giữ email và username đồng bộ
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Address = user.Address;
                existingUser.DepartmentId = user.DepartmentId;
                
                // Cập nhật mật khẩu nếu được cung cấp
                if (!string.IsNullOrEmpty(password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                    var passwordResult = await _userManager.ResetPasswordAsync(existingUser, token, password);
                    
                    if (!passwordResult.Succeeded)
                    {
                        var errors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Không thể cập nhật mật khẩu: {errors}");
                    }
                }
                
                var result = await _userManager.UpdateAsync(existingUser);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Không thể cập nhật người dùng: {errors}");
                }
                
                return existingUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật người dùng id={UserId}", user.Id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                var result = await _userManager.DeleteAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa người dùng id={UserId}", id);
                return false;
            }
        }

        public async Task<bool> AuthenticateUserAsync(string email, string password)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return false;
                }

                // Kiểm tra mật khẩu
                return await _userManager.CheckPasswordAsync(user, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác thực người dùng email={Email}", email);
                return false;
            }
        }
    }
} 