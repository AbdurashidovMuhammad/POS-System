namespace Application.DTOs.AuditLogDTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public int ActionType { get; set; }
    public string ActionTypeName { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public int EntityId { get; set; }
    public string Description { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}
