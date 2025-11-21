using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Pet.Web.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthApiService> _logger;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AuthApiService(HttpClient httpClient, ILogger<AuthApiService> logger, IHttpContextAccessor? httpContextAccessor = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserSessionModel?> RegisterAsync(RegisterViewModel model)
        {
            _logger.LogInformation($"Service: AuthApiService, Method: RegisterAsync, Email: {model?.Email ?? "unknown"}");
            try
            {
                object requestData = new
                {
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = model.Role
                };

                string json = JsonSerializer.Serialize(requestData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("api/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JsonElement result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    return new UserSessionModel
                    {
                        UserId = result.GetProperty("userId").GetString() ?? "",
                        Email = result.GetProperty("email").GetString() ?? "",
                        FirstName = result.GetProperty("firstName").GetString() ?? "",
                        LastName = result.GetProperty("lastName").GetString() ?? "",
                        Role = result.GetProperty("role").GetString() ?? "Public"
                    };
                }

                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Registration failed: {response.StatusCode} - {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return null;
            }
        }

        public async Task<UserSessionModel?> LoginAsync(LoginViewModel model)
        {
            _logger.LogInformation($"Service: AuthApiService, Method: LoginAsync, Email: {model?.Email ?? "unknown"}");
            try
            {
                object requestData = new
                {
                    Email = model.Email,
                    Password = model.Password
                };

                string json = JsonSerializer.Serialize(requestData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    JsonElement result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    return new UserSessionModel
                    {
                        UserId = result.GetProperty("userId").GetString() ?? "",
                        Email = result.GetProperty("email").GetString() ?? "",
                        FirstName = result.GetProperty("firstName").GetString() ?? "",
                        LastName = result.GetProperty("lastName").GetString() ?? "",
                        Role = result.GetProperty("role").GetString() ?? "Public"
                    };
                }

                _logger.LogWarning($"Login failed: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return null;
            }
        }

        public async Task<List<UserViewModel>> GetAllUsersAsync()
        {
            _logger.LogInformation($"Service: AuthApiService, Method: GetAllUsersAsync");
            try
            {
                AddUserHeaders();

                HttpResponseMessage response = await _httpClient.GetAsync("api/auth/users");

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<UserViewModel>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return users ?? new List<UserViewModel>();
                }

                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"GetAllUsers failed: {response.StatusCode} - {errorContent}");
                return new List<UserViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all users");
                return new List<UserViewModel>();
            }
        }

        public async Task<UserViewModel?> UpdateUserAsync(string userId, UpdateUserViewModel model)
        {
            _logger.LogInformation($"Service: AuthApiService, Method: UpdateUserAsync, UserId: {userId}");
            try
            {
                AddUserHeaders();

                object requestData = new
                {
                    Password = model.Password,
                    Role = model.Role
                };

                string json = JsonSerializer.Serialize(requestData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PutAsync($"api/auth/users/{userId}", content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<UserViewModel>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return user;
                }

                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"UpdateUser failed: {response.StatusCode} - {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return null;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            _logger.LogInformation($"Service: AuthApiService, Method: DeleteUserAsync, UserId: {userId}");
            try
            {
                AddUserHeaders();

                HttpResponseMessage response = await _httpClient.DeleteAsync($"api/auth/users/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"DeleteUser failed: {response.StatusCode} - {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            _logger.LogInformation($"Service: AuthApiService, Method: ChangePasswordAsync");
            try
            {
                AddUserHeaders();

                object requestData = new
                {
                    CurrentPassword = model.CurrentPassword,
                    NewPassword = model.NewPassword
                };

                string json = JsonSerializer.Serialize(requestData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, "api/auth/password")
                {
                    Content = content
                };

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"ChangePassword failed: {response.StatusCode} - {errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return false;
            }
        }

        private void AddUserHeaders()
        {
            if (_httpContextAccessor?.HttpContext == null)
                return;

            string? userEmail = _httpContextAccessor.HttpContext.Session.GetString("Email");
            string? userRole = _httpContextAccessor.HttpContext.Session.GetString("Role");
            
            _httpClient.DefaultRequestHeaders.Remove("X-User-Email");
            _httpClient.DefaultRequestHeaders.Remove("X-User-Role");
            
            if (!string.IsNullOrEmpty(userEmail))
            {
                _httpClient.DefaultRequestHeaders.Add("X-User-Email", userEmail);
            }
            
            if (!string.IsNullOrEmpty(userRole))
            {
                _httpClient.DefaultRequestHeaders.Add("X-User-Role", userRole);
            }
        }
    }
}
