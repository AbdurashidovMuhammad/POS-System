using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPF.Models;

public class UserDto : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    private string? _password;
    public string? Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool? IsActive { get; set; }
}
