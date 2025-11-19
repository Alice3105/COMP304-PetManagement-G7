using Pet.API.Models.Entities;

namespace Pet.API.Repositories.Interfaces
{
    public interface IAdoptionRepository
    {
        Task<Adoption> CreateAsync(Adoption adoption);
        Task<Adoption?> GetByIdAsync(string adoptionId);
        Task<IEnumerable<Adoption>> GetAllAsync();
        Task<IEnumerable<Adoption>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Adoption>> GetByPetIdAsync(string petId);
        Task<Adoption> UpdateAsync(Adoption adoption);
        Task<Adoption> UpdateStatusAsync(string adoptionId, string status, string reviewedBy, string? reviewNotes = null);
        Task DeleteAsync(string adoptionId);
    }
}
