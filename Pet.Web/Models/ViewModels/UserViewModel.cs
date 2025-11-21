namespace Pet.Web.Models.ViewModels
{
    public class UserViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? CreatedDate { get; set; }
    }

    public class UpdateUserViewModel
    {
        public string? Password { get; set; }
        public string? Role { get; set; }
    }
}

