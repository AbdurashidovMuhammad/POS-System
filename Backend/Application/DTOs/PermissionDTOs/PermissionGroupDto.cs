namespace Application.DTOs.PermissionDTOs;

public class PermissionGroupDto
{
    public string Section { get; set; } = null!;
    public string SectionDisplayName { get; set; } = null!;
    public List<PermissionDto> Permissions { get; set; } = new();
}
