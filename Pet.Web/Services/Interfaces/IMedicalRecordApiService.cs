using Pet.Web.Models.ViewModels;

namespace Pet.Web.Services.Interfaces
{
    public interface IMedicalRecordApiService
    {
        Task<List<MedicalRecordViewModel>> GetMedicalRecordsByPetIdAsync(string petId);
        Task<MedicalRecordViewModel?> CreateMedicalRecordAsync(MedicalRecordViewModel record);
    }
}

