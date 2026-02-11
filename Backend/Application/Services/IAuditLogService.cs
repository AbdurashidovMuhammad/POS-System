using Application.DTOs.AuditLogDTOs;
using Application.DTOs.Common;
using Core.Enums;

namespace Application.Services;

public interface IAuditLogService
{
    Task LogAsync(int userId, Action_Type actionType, string entityType, int entityId, string description);
    Task<PagedResult<AuditLogDto>> GetLogsAsync(AuditLogFilterDto filter);
}
