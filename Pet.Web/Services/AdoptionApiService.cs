using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;

namespace Pet.Web.Services
{
    public class AdoptionApiService : BaseApiService, IAdoptionApiService
    {
        public AdoptionApiService(HttpClient httpClient, ILogger<AdoptionApiService> logger)
            : base(httpClient, logger, null)
        {
        }

        public async Task<List<AdoptionViewModel>> GetAllAdoptionsAsync()
        {
            return await GetListAsync<AdoptionViewModel>("api/adoptions");
        }

        public async Task<AdoptionViewModel?> GetAdoptionByIdAsync(string adoptionId)
        {
            return await GetAsync<AdoptionViewModel>($"api/adoptions/{adoptionId}");
        }

        public async Task<List<AdoptionViewModel>> GetAdoptionsByUserIdAsync(string userId)
        {
            return await GetListAsync<AdoptionViewModel>($"api/adoptions/user/{userId}");
        }

        public async Task<AdoptionViewModel?> CreateAdoptionAsync(CreateAdoptionViewModel model, string userId, string userEmail, string firstName, string lastName)
        {
            object requestData = new
            {
                model.PetId,
                UserId = userId,
                UserEmail = userEmail,
                UserFirstName = firstName,
                UserLastName = lastName,
                model.PhoneNumber,
                model.Address,
                model.HousingType,
                model.HasYard,
                model.HasOtherPets,
                OtherPetsDescription = model.OtherPetsDescription ?? "",
                model.HasChildren,
                ChildrenAges = model.ChildrenAges ?? "",
                model.EmploymentStatus,
                model.Reason,
                Status = "Pending"
            };

            return await PostAsync<AdoptionViewModel>("api/adoptions", requestData);
        }

        public async Task<bool> UpdateAdoptionStatusAsync(string adoptionId, string status, string reviewedBy, string? reviewNotes = null)
        {
            object requestData = new
            {
                Status = status,
                ReviewedBy = reviewedBy,
                reviewNotes
            };

            return await PutAsync($"api/adoptions/{adoptionId}/status", requestData);
        }
    }
}
