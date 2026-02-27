using Application.DTOs;
using Application.DTOs.PermissionDTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize(Roles = "SuperAdmin")]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get all predefined permissions grouped by section
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var groups = await _permissionService.GetAllPermissionsAsync();
        return Ok(ApiResult<List<PermissionGroupDto>>.Success(groups));
    }

    /// <summary>
    /// Get current permissions for a specific user
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetUserPermissions(int userId)
    {
        var result = await _permissionService.GetUserPermissionsAsync(userId);
        return Ok(ApiResult<UserPermissionsDto>.Success(result));
    }

    /// <summary>
    /// Update (replace) permissions for a specific user
    /// </summary>
    [HttpPut("user/{userId:int}")]
    public async Task<IActionResult> UpdateUserPermissions(int userId, [FromBody] UpdateUserPermissionsDto dto)
    {
        await _permissionService.UpdateUserPermissionsAsync(userId, dto.PermissionIds);
        return Ok(ApiResult<string>.Success("Ruxsatlar muvaffaqiyatli yangilandi"));
    }
}
