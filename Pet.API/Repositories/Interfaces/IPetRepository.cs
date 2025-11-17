using PetEntity = Pet.API.Models.Entities.Pet;

namespace Pet.API.Repositories.Interfaces
{
    public interface IPetRepository
    {
        Task<PetEntity> CreateAsync(PetEntity pet);
        Task<PetEntity?> GetByIdAsync(string petId);
        Task<IEnumerable<PetEntity>> GetAllAsync();
        Task<PetEntity> UpdateAsync(PetEntity pet);
        Task DeleteAsync(string petId);
    }
}
