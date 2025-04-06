using BlazorHybridApp.Core.Data;
using BlazorHybridApp.Core.Interfaces;
using BlazorHybridApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorHybridApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(
            AppDbContext context,
            IPermissionService permissionService,
            ILogger<DepartmentController> logger)
        {
            _context = context;
            _permissionService = permissionService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            try
            {
                return await _context.Departments.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng ban");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                // Kiểm tra người dùng có quyền xem danh sách phòng ban không
                var hasPermission = await _permissionService.HasPermissionAsync(userId, "departments.view");
                if (!hasPermission)
                {
                    return Forbid();
                }

                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                {
                    return NotFound(new { message = "Không tìm thấy phòng ban" });
                }

                return department;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy thông tin phòng ban {id}");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutDepartment(int id, Department department)
        {
            try
            {
                if (id != department.Id)
                {
                    return BadRequest(new { message = "Id không khớp" });
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                // Kiểm tra người dùng có quyền sửa phòng ban không
                var hasPermission = await _permissionService.HasPermissionAsync(userId, "departments.edit");
                if (!hasPermission)
                {
                    return Forbid();
                }

                _context.Entry(department).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(id))
                    {
                        return NotFound(new { message = "Không tìm thấy phòng ban" });
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật phòng ban {id}");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Department>> PostDepartment(Department department)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                // Kiểm tra người dùng có quyền tạo phòng ban không
                var hasPermission = await _permissionService.HasPermissionAsync(userId, "departments.create");
                if (!hasPermission)
                {
                    return Forbid();
                }

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetDepartment", new { id = department.Id }, department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phòng ban mới");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                // Kiểm tra người dùng có quyền xóa phòng ban không
                var hasPermission = await _permissionService.HasPermissionAsync(userId, "departments.delete");
                if (!hasPermission)
                {
                    return Forbid();
                }

                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                {
                    return NotFound(new { message = "Không tìm thấy phòng ban" });
                }

                // Kiểm tra xem phòng ban có người dùng không
                var hasUsers = await _context.Users.AnyAsync(u => u.DepartmentId == id);
                if (hasUsers)
                {
                    return BadRequest(new { message = "Không thể xóa phòng ban vì đang có người dùng thuộc phòng ban này" });
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa phòng ban {id}");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        [HttpGet("{departmentId}/users")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AppUserDto>>> GetDepartmentUsers(int departmentId)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
                }

                // Kiểm tra người dùng có quyền xem người dùng không
                var hasPermission = await _permissionService.HasPermissionAsync(userId, "users.view");
                var hasDepartmentPermission = await _permissionService.HasDepartmentPermissionAsync(userId, departmentId, "users.view");
                
                if (!hasPermission && !hasDepartmentPermission)
                {
                    return Forbid();
                }

                var users = await _context.Users
                    .Where(u => u.DepartmentId == departmentId)
                    .Select(u => new AppUserDto
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        PhoneNumber = u.PhoneNumber,
                        DepartmentId = u.DepartmentId
                    })
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách người dùng thuộc phòng ban {departmentId}");
                return StatusCode(500, new { message = "Lỗi máy chủ nội bộ" });
            }
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }

    public class AppUserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public int? DepartmentId { get; set; }
    }
} 