using BlazorHybridApp.Core.Interfaces;
using BlazorHybridApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BlazorHybridApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            IPermissionService permissionService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _permissionService = permissionService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
                }

                var token = await GenerateJwtToken(user);
                return Ok(new
                {
                    token = token,
                    expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60")),
                    user = new
                    {
                        id = user.Id,
                        userName = user.UserName,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        departmentId = user.DepartmentId
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng nhập");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    return BadRequest(new { message = "Email đã tồn tại" });
                }

                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    DepartmentId = model.DepartmentId,
                    EmailConfirmed = true // Xác nhận email ngay lập tức cho môi trường dev
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Đăng ký không thành công", errors = result.Errors });
                }

                // Thêm role cho user
                if (!string.IsNullOrEmpty(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
                else
                {
                    // Mặc định là Staff nếu không có role
                    await _userManager.AddToRoleAsync(user, "Staff");
                }

                // Thêm các quyền cơ bản cho người dùng mới
                await _permissionService.AddUserPermissionAsync(user.Id, "self.view");
                await _permissionService.AddUserPermissionAsync(user.Id, "self.edit");

                return Ok(new { message = "Đăng ký thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized(new { message = "Người dùng không tồn tại" });
                }

                var token = await GenerateJwtToken(user);
                return Ok(new
                {
                    token = token,
                    expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60"))
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi làm mới token");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok(new { message = "Đăng xuất thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng xuất");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var permissions = await _permissionService.GetUserPermissionsAsync(userId);

                return Ok(new
                {
                    id = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    phoneNumber = user.PhoneNumber,
                    departmentId = user.DepartmentId,
                    roles = roles,
                    permissions = permissions.Select(p => p.Permission?.Code)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin người dùng hiện tại");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpGet("external-login")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                return BadRequest(new { message = $"Lỗi từ nhà cung cấp bên ngoài: {remoteError}" });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return BadRequest(new { message = "Lỗi khi lấy thông tin đăng nhập bên ngoài" });
            }

            // Đăng nhập người dùng bằng thông tin đăng nhập bên ngoài
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var token = await GenerateJwtToken(user);

                // Chuyển hướng đến client với token
                var targetUrl = returnUrl ?? "/";
                targetUrl = targetUrl + (targetUrl.Contains("?") ? "&" : "?") + "token=" + token;
                return Redirect(targetUrl);
            }

            // Nếu người dùng chưa có tài khoản, yêu cầu tạo tài khoản mới
            string email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return BadRequest(new { message = "Không thể lấy email từ tài khoản bên ngoài" });
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // Nếu người dùng đã tồn tại, liên kết tài khoản bên ngoài với tài khoản hiện có
                await _userManager.AddLoginAsync(existingUser, info);
                var token = await GenerateJwtToken(existingUser);

                // Chuyển hướng đến client với token
                var targetUrl = returnUrl ?? "/";
                targetUrl = targetUrl + (targetUrl.Contains("?") ? "&" : "?") + "token=" + token;
                return Redirect(targetUrl);
            }
            else
            {
                // Tạo người dùng mới từ thông tin đăng nhập bên ngoài
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    // Liên kết tài khoản bên ngoài
                    await _userManager.AddLoginAsync(user, info);
                    
                    // Thêm role Staff mặc định
                    await _userManager.AddToRoleAsync(user, "Staff");

                    // Thêm các quyền cơ bản
                    await _permissionService.AddUserPermissionAsync(user.Id, "self.view");
                    await _permissionService.AddUserPermissionAsync(user.Id, "self.edit");

                    var token = await GenerateJwtToken(user);

                    // Chuyển hướng đến client với token
                    var targetUrl = returnUrl ?? "/";
                    targetUrl = targetUrl + (targetUrl.Contains("?") ? "&" : "?") + "token=" + token;
                    return Redirect(targetUrl);
                }

                return BadRequest(new { message = "Lỗi khi tạo tài khoản mới", errors = createResult.Errors });
            }
        }

        private async Task<string> GenerateJwtToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Thêm roles vào claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Thêm permissions vào claims
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
            foreach (var permission in permissions)
            {
                if (permission.Permission != null)
                {
                    claims.Add(new Claim("permission", permission.Permission.Code));
                }
            }

            // Thêm departmentId vào claims
            if (user.DepartmentId.HasValue)
            {
                claims.Add(new Claim("departmentId", user.DepartmentId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60"));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public int? DepartmentId { get; set; }
        public string Role { get; set; }
    }
} 