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
            _logger.LogInformation($"Service: MedicalRecordApiService, Method: GetMedicalRecordsByPetIdAsync, PetId: {petId}");
            return await GetListAsync<MedicalRecordViewModel>($"api/medicalrecords/pet/{petId}", requireAuth: true);
        }

        public async Task<MedicalRecordViewModel?> CreateMedicalRecordAsync(MedicalRecordViewModel record)
        {
            _logger.LogInformation($"Service: MedicalRecordApiService, Method: CreateMedicalRecordAsync, RecordId: {record?.RecordId ?? "new"}");
            return await PostAsync<MedicalRecordViewModel>("api/medicalrecords", record, requireAuth: true);
        }

        public async Task<bool> UpdateMedicalRecordAsync(string recordId, MedicalRecordViewModel record)
        {
            _logger.LogInformation($"Service: MedicalRecordApiService, Method: UpdateMedicalRecordAsync, RecordId: {recordId}");
            return await PutAsync($"api/medicalrecords/{recordId}", record, requireAuth: true);
        }

        public async Task<bool> DeleteMedicalRecordAsync(string recordId)
        {
            _logger.LogInformation($"Service: MedicalRecordApiService, Method: DeleteMedicalRecordAsync, RecordId: {recordId}");
            return await DeleteAsync($"api/medicalrecords/{recordId}", requireAuth: true);
        }

    }
}

