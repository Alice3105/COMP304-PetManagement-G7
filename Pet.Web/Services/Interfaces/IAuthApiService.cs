using Pet.Web.Models.ViewModels;

namespace Pet.Web.Services.Interfaces
{
    public interface IAuthApiService
    {
        Task<UserSessionModel?> RegisterAsync(RegisterViewModel model);
        Task<UserSessionModel?> LoginAsync(LoginViewModel model);
        Task<List<UserViewModel>> GetAllUsersAsync();
        Task<UserViewModel?> UpdateUserAsync(string userId, UpdateUserViewModel model);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> ChangePasswordAsync(ChangePasswordViewModel model);
    }
}

