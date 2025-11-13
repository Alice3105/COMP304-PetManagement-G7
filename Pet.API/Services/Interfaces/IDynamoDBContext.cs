using Amazon.DynamoDBv2;
using Pet.API.Models.Entities;

namespace Pet.API.Services.Interfaces
{
    public interface IDynamoDBContext
    {
        AmazonDynamoDBClient Client { get; }
        string UsersTableName { get; }
        string PetsTableName { get; }
        string AdoptionsTableName { get; }
        string MedicalRecordsTableName { get; }

        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<ApplicationUser?> GetUserByUsernameAsync(string username);
        Task CreateUserAsync(ApplicationUser user);
        Task UpdateUserAsync(ApplicationUser user);
        Task DeleteUserAsync(string userId);
    }
}

