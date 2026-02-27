namespace WPF.Models;

public class PermissionDto
{
    public int Id { get; set; }
    public string Section { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class PermissionGroupDto
{
    public string Section { get; set; } = string.Empty;
    public string SectionDisplayName { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class UserPermissionsDto
{
    public int UserId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class UpdateUserPermissionsDto
{
    public List<int> PermissionIds { get; set; } = new();
}
