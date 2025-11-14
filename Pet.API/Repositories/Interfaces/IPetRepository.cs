using Pet.API.Models.Entities;

namespace Pet.API.Repositories.Interfaces
{
    public interface IPetRepository
    {
        Task<Pet> CreateAsync(Pet pet);
        Task<Pet?> GetByIdAsync(string petId);
        Task<IEnumerable<Pet>> GetAllAsync();
        Task<Pet> UpdateAsync(Pet pet);
        Task DeleteAsync(string petId);
    }
}

