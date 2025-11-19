using Pet.Web.Models.ViewModels;

namespace Pet.Web.Services.Interfaces
{
    public interface IAdoptionApiService
    {
        Task<List<AdoptionViewModel>> GetAllAdoptionsAsync();
        Task<AdoptionViewModel?> GetAdoptionByIdAsync(string adoptionId);
        Task<List<AdoptionViewModel>> GetAdoptionsByUserIdAsync(string userId);
        Task<AdoptionViewModel?> CreateAdoptionAsync(CreateAdoptionViewModel model, string userId, string userEmail, string firstName, string lastName);
        Task<bool> UpdateAdoptionAsync(string adoptionId, CreateAdoptionViewModel model);
        Task<bool> UpdateAdoptionStatusAsync(string adoptionId, string status, string reviewedBy, string? reviewNotes = null);
        Task<bool> DeleteAdoptionAsync(string adoptionId);
    }
}

