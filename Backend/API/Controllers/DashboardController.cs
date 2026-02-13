using System.Security.Claims;
using Application.DTOs;
using Application.DTOs.DashboardDTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResult<DashboardStatsDto>.Failure(["Foydalanuvchi aniqlanmadi"]));
            }

            var roleClaim = User.FindFirst(ClaimTypes.Role);
            int? filterUserId = userId;

            if (roleClaim?.Value == "SuperAdmin")
            {
                filterUserId = null;
            }

            var stats = await _dashboardService.GetDashboardStatsAsync(filterUserId);
            return Ok(ApiResult<DashboardStatsDto>.Success(stats));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<DashboardStatsDto>.Failure([ex.Message]));
        }
    }

    [HttpGet("admin-stats")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAdminStats()
    {
        try
        {
            var stats = await _dashboardService.GetAdminDashboardStatsAsync();
            return Ok(ApiResult<AdminDashboardStatsDto>.Success(stats));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<AdminDashboardStatsDto>.Failure([ex.Message]));
        }
    }
}
