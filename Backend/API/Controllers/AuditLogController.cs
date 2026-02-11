using System.Security.Claims;
using Application.DTOs;
using Application.DTOs.AuditLogDTOs;
using Application.DTOs.Common;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterDto filter)
    {
        try
        {
            var result = await _auditLogService.GetLogsAsync(filter);
            return Ok(ApiResult<PagedResult<AuditLogDto>>.Success(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<PagedResult<AuditLogDto>>.Failure([ex.Message]));
        }
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyLogs([FromQuery] int pageSize = 10)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(ApiResult<PagedResult<AuditLogDto>>.Failure(["Foydalanuvchi aniqlanmadi"]));

            var filter = new AuditLogFilterDto
            {
                UserId = userId,
                Page = 1,
                PageSize = pageSize
            };
            var result = await _auditLogService.GetLogsAsync(filter);
            return Ok(ApiResult<PagedResult<AuditLogDto>>.Success(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResult<PagedResult<AuditLogDto>>.Failure([ex.Message]));
        }
    }
}
