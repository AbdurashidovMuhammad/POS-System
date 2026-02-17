namespace WPF.Models;

public class UserFilterItem
{
    public int? Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public override string ToString() => DisplayName;
}
