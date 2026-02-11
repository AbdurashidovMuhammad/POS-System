using WPF.Models;

namespace WPF.Services;

public interface IApiService
{
    Func<Task<bool>>? OnUnauthorized { get; set; }
    Action<string>? OnForbidden { get; set; }
    Task<ApiResult<T>?> GetAsync<T>(string endpoint);
    Task<ApiResult<T>?> PostAsync<T>(string endpoint, object data);
    Task<ApiResult<T>?> PutAsync<T>(string endpoint, object data);
    Task<ApiResult<T>?> PatchAsync<T>(string endpoint, object data);
    Task<ApiResult<T>?> DeleteAsync<T>(string endpoint);
    Task<byte[]?> GetBytesAsync(string endpoint);
    void SetAuthToken(string token);
    void ClearAuthToken();
}
