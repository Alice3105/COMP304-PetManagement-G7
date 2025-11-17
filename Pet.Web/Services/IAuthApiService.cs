using Pet.Web.Models.ViewModels;

namespace Pet.Web.Services
{
    public interface IAuthApiService
    {
        Task<UserSessionModel?> RegisterAsync(RegisterViewModel model);
        Task<UserSessionModel?> LoginAsync(LoginViewModel model);
    }
}
