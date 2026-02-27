namespace Core.Entities;

public class Permission
{
    public int Id { get; set; }
    public string Section { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string DisplayName { get; set; } = null!;

    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
