using Pet.Web.Models.ViewModels;
using System.Text;
using System.Text.Json;

namespace Pet.Web.Services
{
    public class AdoptionApiService : IAdoptionApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdoptionApiService> _logger;

        public AdoptionApiService(HttpClient httpClient, ILogger<AdoptionApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<AdoptionViewModel>> GetAllAdoptionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/adoptions");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var adoptions = JsonSerializer.Deserialize<List<AdoptionViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return adoptions ?? new List<AdoptionViewModel>();
                }

                _logger.LogWarning($"Failed to fetch adoptions: {response.StatusCode}");
                return new List<AdoptionViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching adoptions from API");
                return new List<AdoptionViewModel>();
            }
        }

        public async Task<AdoptionViewModel?> GetAdoptionByIdAsync(string adoptionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/adoptions/{adoptionId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AdoptionViewModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching adoption {adoptionId} from API");
                return null;
            }
        }

        public async Task<List<AdoptionViewModel>> GetAdoptionsByUserIdAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/adoptions/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var adoptions = JsonSerializer.Deserialize<List<AdoptionViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return adoptions ?? new List<AdoptionViewModel>();
                }

                return new List<AdoptionViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching adoptions for user {userId} from API");
                return new List<AdoptionViewModel>();
            }
        }

        public async Task<AdoptionViewModel?> CreateAdoptionAsync(CreateAdoptionViewModel model, string userId, string userEmail, string firstName, string lastName)
        {
            try
            {
                var requestData = new
                {
                    PetId = model.PetId,
                    UserId = userId,
                    UserEmail = userEmail,
                    UserFirstName = firstName,
                    UserLastName = lastName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    HousingType = model.HousingType,
                    HasYard = model.HasYard,
                    HasOtherPets = model.HasOtherPets,
                    OtherPetsDescription = model.OtherPetsDescription ?? "",
                    HasChildren = model.HasChildren,
                    ChildrenAges = model.ChildrenAges ?? "",
                    EmploymentStatus = model.EmploymentStatus,
                    Reason = model.Reason,
                    Status = "Pending"
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/adoptions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AdoptionViewModel>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning($"Failed to create adoption: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating adoption via API");
                return null;
            }
        }

        public async Task<bool> UpdateAdoptionStatusAsync(string adoptionId, string status, string reviewedBy, string? reviewNotes = null)
        {
            try
            {
                var requestData = new
                {
                    Status = status,
                    ReviewedBy = reviewedBy,
                    ReviewNotes = reviewNotes
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/adoptions/{adoptionId}/status", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating adoption {adoptionId} status via API");
                return false;
            }
        }
    }
}
