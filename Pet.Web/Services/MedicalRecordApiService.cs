using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Pet.Web.Services
{
    public class MedicalRecordApiService : IMedicalRecordApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MedicalRecordApiService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MedicalRecordApiService(HttpClient httpClient, ILogger<MedicalRecordApiService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthHeader()
        {
            var apiKey = _httpContextAccessor.HttpContext?.Session.GetString("ApiKey");
            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }
        }

        public async Task<List<MedicalRecordViewModel>> GetMedicalRecordsByPetIdAsync(string petId)
        {
            try
            {
                AddAuthHeader();
                var response = await _httpClient.GetAsync($"api/medicalrecords/pet/{petId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var records = JsonSerializer.Deserialize<List<MedicalRecordViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return records ?? new List<MedicalRecordViewModel>();
                }

                _logger.LogWarning($"Failed to fetch medical records for pet {petId}: {response.StatusCode}");
                return new List<MedicalRecordViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching medical records for pet {petId} from API");
                return new List<MedicalRecordViewModel>();
            }
        }

        public async Task<MedicalRecordViewModel?> CreateMedicalRecordAsync(MedicalRecordViewModel record)
        {
            try
            {
                AddAuthHeader();
                var json = JsonSerializer.Serialize(record);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/medicalrecords", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var createdRecord = JsonSerializer.Deserialize<MedicalRecordViewModel>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return createdRecord;
                }

                _logger.LogWarning($"Failed to create medical record: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medical record from API");
                return null;
            }
        }
    }
}

