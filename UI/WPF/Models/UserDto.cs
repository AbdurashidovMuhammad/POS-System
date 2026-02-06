using WPF.Enums;

namespace WPF.Models;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Role Role { get; set; }
}

public class UpdateUserDto
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public Role? Role { get; set; }
    public bool? IsActive { get; set; }
}
