using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace Pet.Web.Services
{
    public class MedicalRecordApiService : BaseApiService, IMedicalRecordApiService
    {
        public MedicalRecordApiService(HttpClient httpClient, ILogger<MedicalRecordApiService> logger, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, logger, httpContextAccessor)
        {
        }

        public async Task<List<MedicalRecordViewModel>> GetMedicalRecordsByPetIdAsync(string petId)
        {
            return await GetListAsync<MedicalRecordViewModel>($"api/medicalrecords/pet/{petId}", requireAuth: true);
        }

        public async Task<MedicalRecordViewModel?> CreateMedicalRecordAsync(MedicalRecordViewModel record)
        {
            return await PostAsync<MedicalRecordViewModel>("api/medicalrecords", record, requireAuth: true);
        }

        public async Task<bool> UpdateMedicalRecordAsync(string recordId, MedicalRecordViewModel record)
        {
            return await PutAsync($"api/medicalrecords/{recordId}", record, requireAuth: true);
        }

        public async Task<bool> PatchMedicalRecordAsync(string recordId, MedicalRecordViewModel record)
        {
            return await PatchAsync($"api/medicalrecords/{recordId}", record, requireAuth: true);
        }

        public async Task<bool> DeleteMedicalRecordAsync(string recordId)
        {
            return await DeleteAsync($"api/medicalrecords/{recordId}", requireAuth: true);
        }

        private async Task<bool> PatchAsync(string endpoint, object data, bool requireAuth = false)
        {
            try
            {
                if (requireAuth)
                    AddAuthHeader();

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
    }
}

