namespace WPF.Models;

public class ApiResult<T>
{
    public bool Succeeded { get; set; }
    public T? Result { get; set; }
    public IEnumerable<string> Errors { get; set; } = [];
}
