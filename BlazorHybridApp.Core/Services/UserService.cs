using System.Security.Cryptography;
using System.Text;
using BlazorHybridApp.Core.Data;
using BlazorHybridApp.Core.Interfaces;
using BlazorHybridApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorHybridApp.Core.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng");
                return Enumerable.Empty<User>();
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _context.Users.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy người dùng id={UserId}", id);
                return null;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy người dùng theo email={Email}", email);
                return null;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                // Kiểm tra xem email đã tồn tại chưa
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == user.Email);
                
                if (existingUser != null)
                {
                    throw new InvalidOperationException("Email đã tồn tại");
                }

                // Hash mật khẩu trước khi lưu
                user.Password = HashPassword(user.Password);
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo người dùng mới");
                throw;
            }
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            try
            {
                var existingUser = await _context.Users.FindAsync(user.Id);
                if (existingUser == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy người dùng với ID {user.Id}");
                }

                // Cập nhật thông tin người dùng
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                
                // Chỉ cập nhật mật khẩu nếu nó được thay đổi
                if (!string.IsNullOrEmpty(user.Password) && user.Password != existingUser.Password)
                {
                    existingUser.Password = HashPassword(user.Password);
                }
                
                // Cập nhật các thông tin khác
                existingUser.Address = user.Address;
                existingUser.Phone = user.Phone;
                existingUser.Role = user.Role;

                await _context.SaveChangesAsync();
                return existingUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật người dùng id={UserId}", user.Id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return false;
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
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
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return false;
                }

                // Kiểm tra mật khẩu
                return VerifyPassword(password, user.Password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác thực người dùng email={Email}", email);
                return false;
            }
        }

        // Phương thức hash mật khẩu sử dụng SHA256 + salt
        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            // Tạo salt ngẫu nhiên (trong thực tế, salt nên được lưu cùng password)
            string salt = "BlazorHybridAppSalt"; // Đây chỉ là ví dụ, nên sử dụng salt ngẫu nhiên và lưu cùng password
            
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = string.Concat(password, salt);
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Phương thức xác thực mật khẩu
        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            string salt = "BlazorHybridAppSalt"; // Salt giống với phương thức HashPassword
            
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = string.Concat(enteredPassword, salt);
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                var hashedPassword = Convert.ToBase64String(hashedBytes);
                
                return hashedPassword == storedHash;
            }
        }
    }
} 