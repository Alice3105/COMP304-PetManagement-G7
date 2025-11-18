using Pet.Web.Models.ViewModels;

namespace Pet.Web.Services.Interfaces
{
    public interface IPetApiService
    {
        Task<List<PetViewModel>> GetAllPetsAsync();
        Task<PetViewModel?> GetPetByIdAsync(string petId);
        Task<PetViewModel?> CreatePetAsync(CreatePetViewModel model);
        Task<bool> UpdatePetAsync(string petId, PetViewModel model);
        Task<bool> DeletePetAsync(string petId);
    }
}

