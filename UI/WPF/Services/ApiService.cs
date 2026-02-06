using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WPF.Models;

namespace WPF.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResult<T>?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
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
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
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
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
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
            var content = JsonContent.Create(data);
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var response = await _httpClient.SendAsync(request);
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
            var response = await _httpClient.DeleteAsync(endpoint);
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

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
