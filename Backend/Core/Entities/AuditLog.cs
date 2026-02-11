using Core.Enums;

namespace Core.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public Action_Type ActionType { get; set; }
    public string EntityType { get; set; } = null!;
    public int EntityId { get; set; }
    public string Description { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public User User { get; set; } = null!;
}
