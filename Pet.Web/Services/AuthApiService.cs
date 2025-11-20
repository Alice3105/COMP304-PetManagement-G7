using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace Pet.Web.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthApiService> _logger;

        public AuthApiService(HttpClient httpClient, ILogger<AuthApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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
                        Role = result.GetProperty("role").GetString() ?? "Public",
                        ApiKey = result.GetProperty("apiKey").GetString() ?? ""
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
                        Role = result.GetProperty("role").GetString() ?? "Public",
                        ApiKey = result.GetProperty("apiKey").GetString() ?? ""
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
    }
}
