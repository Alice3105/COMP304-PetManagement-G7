using Pet.Web.Models.ViewModels;

namespace Pet.Web.Services.Interfaces
{
    public interface IAuthApiService
    {
        Task<UserSessionModel?> RegisterAsync(RegisterViewModel model);
        Task<UserSessionModel?> LoginAsync(LoginViewModel model);
    }
}

