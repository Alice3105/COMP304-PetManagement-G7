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
        /// Adds authentication header from session if available
        /// </summary>
        protected void AddAuthHeader()
        {
            if (_httpContextAccessor?.HttpContext == null)
                return;

            string? apiKey = _httpContextAccessor.HttpContext.Session.GetString("ApiKey");
            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
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
                    AddAuthHeader();

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
                    AddAuthHeader();

                string json = JsonSerializer.Serialize(data);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
                }
                _logger.LogWarning($"Failed to POST to {endpoint}: {response.StatusCode}");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error POSTing to {endpoint}");
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
                    AddAuthHeader();

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
        /// Generic DELETE request handler
        /// </summary>
        protected async Task<bool> DeleteAsync(string endpoint, bool requireAuth = false)
        {
            try
            {
                if (requireAuth)
                    AddAuthHeader();

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

