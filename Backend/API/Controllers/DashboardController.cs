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
            var stats = await _dashboardService.GetDashboardStatsAsync();
            return Ok(ApiResult<DashboardStatsDto>.Success(stats));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<DashboardStatsDto>.Failure([ex.Message]));
        }
    }
}
