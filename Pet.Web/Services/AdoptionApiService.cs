using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace Pet.Web.Services
{
    public class AdoptionApiService : IAdoptionApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdoptionApiService> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AdoptionApiService(HttpClient httpClient, ILogger<AdoptionApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<AdoptionViewModel>> GetAllAdoptionsAsync()
        {
            return await GetAsync<List<AdoptionViewModel>>("api/adoptions") ?? new List<AdoptionViewModel>();
        }

        public async Task<AdoptionViewModel?> GetAdoptionByIdAsync(string adoptionId)
        {
            return await GetAsync<AdoptionViewModel>($"api/adoptions/{adoptionId}");
        }

        public async Task<List<AdoptionViewModel>> GetAdoptionsByUserIdAsync(string userId)
        {
            return await GetAsync<List<AdoptionViewModel>>($"api/adoptions/user/{userId}") ?? new List<AdoptionViewModel>();
        }

        public async Task<AdoptionViewModel?> CreateAdoptionAsync(CreateAdoptionViewModel model, string userId, string userEmail, string firstName, string lastName)
        {
            var requestData = new
            {
                model.PetId,
                UserId = userId,
                UserEmail = userEmail,
                UserFirstName = firstName,
                UserLastName = lastName,
                model.PhoneNumber,
                model.Address,
                model.HousingType,
                model.HasYard,
                model.HasOtherPets,
                OtherPetsDescription = model.OtherPetsDescription ?? "",
                model.HasChildren,
                ChildrenAges = model.ChildrenAges ?? "",
                model.EmploymentStatus,
                model.Reason,
                Status = "Pending"
            };

            return await PostAsync<AdoptionViewModel>("api/adoptions", requestData);
        }

        public async Task<bool> UpdateAdoptionStatusAsync(string adoptionId, string status, string reviewedBy, string? reviewNotes = null)
        {
            var requestData = new
            {
                Status = status,
                ReviewedBy = reviewedBy,
                reviewNotes
            };

            return await PutAsync($"api/adoptions/{adoptionId}/status", requestData);
        }

        #region Helper Methods

        private async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
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

        private async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
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

        private async Task<bool> PutAsync(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(endpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error PUTting to {endpoint}");
                return false;
            }
        }

        #endregion
    }
}
