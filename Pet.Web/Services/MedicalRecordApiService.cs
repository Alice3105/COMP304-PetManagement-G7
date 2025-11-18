using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;

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
    }
}

