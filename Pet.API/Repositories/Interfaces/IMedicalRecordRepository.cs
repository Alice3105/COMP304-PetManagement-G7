using Pet.API.Models.Entities;

namespace Pet.API.Repositories.Interfaces
{
    public interface IMedicalRecordRepository
    {
        Task<MedicalRecord> CreateAsync(MedicalRecord record);
        Task<MedicalRecord?> GetByIdAsync(string recordId);
        Task<List<MedicalRecord>> GetByPetIdAsync(string petId);
        Task<List<MedicalRecord>> GetAllAsync();
        Task<MedicalRecord> UpdateAsync(MedicalRecord record);
        Task DeleteAsync(string recordId);
    }
}

