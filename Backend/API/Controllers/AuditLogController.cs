using Application.DTOs;
using Application.DTOs.AuditLogDTOs;
using Application.DTOs.Common;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "SuperAdmin")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
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
}
