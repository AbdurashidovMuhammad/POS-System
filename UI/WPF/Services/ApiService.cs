using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WPF.Models;

namespace WPF.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private bool _isRefreshing;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Func<Task<bool>>? OnUnauthorized { get; set; }
    public Action<string>? OnForbidden { get; set; }

    public async Task<ApiResult<T>?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await SendWithRefreshAsync(() => _httpClient.GetAsync(endpoint));

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return new ApiResult<T>
                    {
                        Succeeded = false,
                        Errors = ["Sizda bu amalni bajarish uchun ruxsat yo'q"]
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new ApiResult<T>
                    {
                        Succeeded = false,
                        Errors = [$"Server xatosi: {(int)response.StatusCode} {response.ReasonPhrase}"]
                    };
                }
            }

            return await response.Content.ReadFromJsonAsync<ApiResult<T>>();
        }
        catch (Exception ex)
        {
            return new ApiResult<T>
            {
                Succeeded = false,
                Errors = [ex.Message]
            };
        }
    }

    public async Task<ApiResult<T>?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var response = await SendWithRefreshAsync(() => _httpClient.PostAsJsonAsync(endpoint, data));
            if (response.StatusCode == HttpStatusCode.Forbidden)
                return new ApiResult<T> { Succeeded = false, Errors = ["Sizda bu amalni bajarish uchun ruxsat yo'q"] };
            return await response.Content.ReadFromJsonAsync<ApiResult<T>>();
        }
        catch (Exception ex)
        {
            return new ApiResult<T>
            {
                Succeeded = false,
                Errors = [ex.Message]
            };
        }
    }

    public async Task<ApiResult<T>?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var response = await SendWithRefreshAsync(() => _httpClient.PutAsJsonAsync(endpoint, data));
            if (response.StatusCode == HttpStatusCode.Forbidden)
                return new ApiResult<T> { Succeeded = false, Errors = ["Sizda bu amalni bajarish uchun ruxsat yo'q"] };
            return await response.Content.ReadFromJsonAsync<ApiResult<T>>();
        }
        catch (Exception ex)
        {
            return new ApiResult<T>
            {
                Succeeded = false,
                Errors = [ex.Message]
            };
        }
    }

    public async Task<ApiResult<T>?> PatchAsync<T>(string endpoint, object data)
    {
        try
        {
            var response = await SendWithRefreshAsync(() =>
            {
                var content = JsonContent.Create(data);
                var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
                return _httpClient.SendAsync(request);
            });
            if (response.StatusCode == HttpStatusCode.Forbidden)
                return new ApiResult<T> { Succeeded = false, Errors = ["Sizda bu amalni bajarish uchun ruxsat yo'q"] };
            return await response.Content.ReadFromJsonAsync<ApiResult<T>>();
        }
        catch (Exception ex)
        {
            return new ApiResult<T>
            {
                Succeeded = false,
                Errors = [ex.Message]
            };
        }
    }

    public async Task<ApiResult<T>?> DeleteAsync<T>(string endpoint)
    {
        try
        {
            var response = await SendWithRefreshAsync(() => _httpClient.DeleteAsync(endpoint));
            if (response.StatusCode == HttpStatusCode.Forbidden)
                return new ApiResult<T> { Succeeded = false, Errors = ["Sizda bu amalni bajarish uchun ruxsat yo'q"] };
            return await response.Content.ReadFromJsonAsync<ApiResult<T>>();
        }
        catch (Exception ex)
        {
            return new ApiResult<T>
            {
                Succeeded = false,
                Errors = [ex.Message]
            };
        }
    }

    public async Task<byte[]?> GetBytesAsync(string endpoint)
    {
        try
        {
            var response = await SendWithRefreshAsync(() => _httpClient.GetAsync(endpoint));
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    private async Task<HttpResponseMessage> SendWithRefreshAsync(Func<Task<HttpResponseMessage>> sendFunc)
    {
        var response = await sendFunc();

        if (response.StatusCode == HttpStatusCode.Unauthorized && !_isRefreshing && OnUnauthorized != null)
        {
            _isRefreshing = true;
            try
            {
                if (await OnUnauthorized())
                {
                    response = await sendFunc();
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            OnForbidden?.Invoke("Sizda bu amalni bajarish uchun ruxsat yo'q");
        }

        return response;
    }
}
