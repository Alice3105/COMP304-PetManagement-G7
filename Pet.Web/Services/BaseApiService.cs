using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Pet.Web.Services
{
    /// <summary>
    /// Base class for API services that provides common HTTP operations and authentication
    /// </summary>
    public abstract class BaseApiService
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;
        protected readonly IHttpContextAccessor? _httpContextAccessor;
        protected static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        protected BaseApiService(
            HttpClient httpClient,
            ILogger logger,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Adds user identification headers from session if available (for admin endpoints)
        /// </summary>
        protected void AddUserHeaders()
        {
            if (_httpContextAccessor?.HttpContext == null)
                return;

            string? userEmail = _httpContextAccessor.HttpContext.Session.GetString("Email");
            string? userRole = _httpContextAccessor.HttpContext.Session.GetString("Role");
            
            if (!string.IsNullOrEmpty(userEmail))
            {
                _httpClient.DefaultRequestHeaders.Remove("X-User-Email");
                _httpClient.DefaultRequestHeaders.Add("X-User-Email", userEmail);
            }
            
            if (!string.IsNullOrEmpty(userRole))
            {
                _httpClient.DefaultRequestHeaders.Remove("X-User-Role");
                _httpClient.DefaultRequestHeaders.Add("X-User-Role", userRole);
            }
        }

        /// <summary>
        /// Generic GET request handler
        /// </summary>
        protected async Task<T?> GetAsync<T>(string endpoint, bool requireAuth = false)
        {
            try
            {
                if (requireAuth)
                    AddUserHeaders();

                HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(content, JsonOptions);
                }
                _logger.LogWarning($"Failed to fetch from {endpoint}: {response.StatusCode}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching from {endpoint}");
                return default;
            }
        }

        /// <summary>
        /// Generic GET request handler that returns a list with fallback to empty list
        /// </summary>
        protected async Task<List<T>> GetListAsync<T>(string endpoint, bool requireAuth = false)
        {
            List<T>? result = await GetAsync<List<T>>(endpoint, requireAuth);
            return result ?? new List<T>();
        }

        /// <summary>
        /// Generic POST request handler
        /// </summary>
        protected async Task<T?> PostAsync<T>(string endpoint, object data, bool requireAuth = false)
        {
            try
            {
                if (requireAuth)
                    AddUserHeaders();

                string json = JsonSerializer.Serialize(data);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                
                string fullUrl = $"{_httpClient.BaseAddress}{endpoint}";
                _logger.LogInformation($"POSTing to {fullUrl}");
                _logger.LogDebug($"Request data: {json}");
                
                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

                string responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"POST to {endpoint} succeeded with status {response.StatusCode}");
                    _logger.LogDebug($"Response content: {responseContent}");
                    return JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
                }
                
                _logger.LogError($"Failed to POST to {endpoint}: {response.StatusCode}");
                _logger.LogError($"Response content: {responseContent}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception POSTing to {endpoint}: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return default;
            }
        }

        /// <summary>
        /// Generic PUT request handler
        /// </summary>
        protected async Task<bool> PutAsync(string endpoint, object data, bool requireAuth = false)
        {
            try
            {
                if (requireAuth)
                    AddUserHeaders();

                string json = JsonSerializer.Serialize(data);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PutAsync(endpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error PUTting to {endpoint}");
                return false;
            }
        }

        /// <summary>
        /// Generic PATCH request handler
        /// </summary>
        protected async Task<bool> PatchAsync(string endpoint, object data, bool requireAuth = false)
        {
            try
            {
                if (requireAuth)
                    AddUserHeaders();

                string json = JsonSerializer.Serialize(data);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
                {
                    Content = content
                };
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error PATCHing to {endpoint}");
                return false;
            }
        }

        /// <summary>
        /// Generic DELETE request handler
        /// </summary>
        protected async Task<bool> DeleteAsync(string endpoint, bool requireAuth = false)
        {
            try
            {
                if (requireAuth)
                    AddUserHeaders();

                HttpResponseMessage response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error DELETEing from {endpoint}");
                return false;
            }
        }
    }
}

