using BlazorHybridApp.Core.Interfaces;
using BlazorHybridApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorHybridApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionController> _logger;

        public PermissionController(
            IPermissionService permissionService,
            ILogger<PermissionController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                var permissions = await _permissionService.GetAllPermissionsAsync();
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách quyền");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            try
            {
                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách quyền của người dùng");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpPost("user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUserPermission([FromBody] UserPermissionModel model)
        {
            try
            {
                await _permissionService.AddUserPermissionAsync(model.UserId, model.PermissionCode);
                return Ok(new { message = "Thêm quyền thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm quyền cho người dùng");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpDelete("user/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUserPermission(int id)
        {
            try
            {
                await _permissionService.RemoveUserPermissionAsync(id);
                return Ok(new { message = "Xóa quyền thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa quyền người dùng");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpGet("check/{permissionCode}")]
        public async Task<IActionResult> CheckPermission(string permissionCode)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var hasPermission = await _permissionService.HasPermissionAsync(userId, permissionCode);
                return Ok(new { hasPermission });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra quyền");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpGet("check/department/{departmentId}/{permissionCode}")]
        public async Task<IActionResult> CheckDepartmentPermission(int departmentId, string permissionCode)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var hasDepartmentPermission = await _permissionService.HasDepartmentPermissionAsync(userId, departmentId, permissionCode);
                return Ok(new { hasDepartmentPermission });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra quyền phòng ban");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpGet("check/self/{targetUserId}/{permissionCode}")]
        public async Task<IActionResult> CheckSelfPermission(string targetUserId, string permissionCode)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                var hasSelfPermission = await _permissionService.HasSelfPermissionAsync(userId, targetUserId, permissionCode);
                return Ok(new { hasSelfPermission });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra quyền cá nhân");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }
    }

    public class UserPermissionModel
    {
        public string UserId { get; set; }
        public string PermissionCode { get; set; }
    }
}