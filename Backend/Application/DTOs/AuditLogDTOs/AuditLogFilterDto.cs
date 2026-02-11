using Application.DTOs.Common;

namespace Application.DTOs.AuditLogDTOs;

public class AuditLogFilterDto : PaginationParams
{
    public int? UserId { get; set; }
    public int? ActionType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
