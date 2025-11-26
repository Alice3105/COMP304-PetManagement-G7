using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Pet.API.Models.Entities;
using Pet.API.Services.Interfaces;

namespace Pet.API.Services
{
    public class DynamoDBContext : IDynamoDBContext
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly IConfiguration _configuration;

        public AmazonDynamoDBClient Client => _client;
        public string UsersTableName { get; }
        public string PetsTableName { get; }
        public string AdoptionsTableName { get; }
        public string MedicalRecordsTableName { get; }

        public DynamoDBContext(IConfiguration configuration)
        {
            _configuration = configuration;

            var region = _configuration["AWS:Region"] ?? "us-east-1";

            
            _client = new AmazonDynamoDBClient(
                Amazon.RegionEndpoint.GetBySystemName(region)
            );

            UsersTableName = _configuration["AWS:DynamoDB:UsersTable"] ?? "PetShelter-Users";
            PetsTableName = _configuration["AWS:DynamoDB:PetsTable"] ?? "PetShelter-Pets";
            AdoptionsTableName = _configuration["AWS:DynamoDB:AdoptionsTable"] ?? "PetShelter-Adoptions";
            MedicalRecordsTableName = _configuration["AWS:DynamoDB:MedicalRecordsTable"] ?? "PetShelter-MedicalRecords";
        }


        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            var request = new GetItemRequest
            {
                TableName = UsersTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "UserId", new AttributeValue { S = userId } }
                }
            };

            var response = await _client.GetItemAsync(request);
            
            if (response.Item == null || response.Item.Count == 0)
                return null;

            return MapToApplicationUser(response.Item);
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            var request = new ScanRequest
            {
                TableName = UsersTableName,
                FilterExpression = "Email = :email",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":email", new AttributeValue { S = email } }
                }
            };

            var response = await _client.ScanAsync(request);
            
            if (response.Items.Count == 0)
                return null;

            return MapToApplicationUser(response.Items[0]);
        }

        public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
        {
            // For simple auth, username = email
            return await GetUserByEmailAsync(username);
        }

        public async Task CreateUserAsync(ApplicationUser user)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = user.UserId } },
                { "Email", new AttributeValue { S = user.Email } },
                { "PasswordHash", new AttributeValue { S = user.PasswordHash } },
                { "FirstName", new AttributeValue { S = user.FirstName } },
                { "LastName", new AttributeValue { S = user.LastName } },
                { "Role", new AttributeValue { S = user.Role } },
                { "CreatedDate", new AttributeValue { S = user.CreatedDate.ToString("o") } },
                { "IsActive", new AttributeValue { BOOL = user.IsActive } }
            };

            if (user.UpdatedDate.HasValue)
            {
                item["UpdatedDate"] = new AttributeValue { S = user.UpdatedDate.Value.ToString("o") };
            }

            var request = new PutItemRequest
            {
                TableName = UsersTableName,
                Item = item
            };

            await _client.PutItemAsync(request);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            user.UpdatedDate = DateTime.UtcNow;
            await CreateUserAsync(user); // DynamoDB PutItem is also used for updates
        }

        public async Task DeleteUserAsync(string userId)
        {
            var request = new DeleteItemRequest
            {
                TableName = UsersTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "UserId", new AttributeValue { S = userId } }
                }
            };

            await _client.DeleteItemAsync(request);
        }

        private ApplicationUser MapToApplicationUser(Dictionary<string, AttributeValue> item)
        {
            return new ApplicationUser
            {
                UserId = item.GetValueOrDefault("UserId")?.S ?? "",
                Email = item.GetValueOrDefault("Email")?.S ?? "",
                PasswordHash = item.GetValueOrDefault("PasswordHash")?.S ?? "",
                FirstName = item.GetValueOrDefault("FirstName")?.S ?? "",
                LastName = item.GetValueOrDefault("LastName")?.S ?? "",
                Role = item.GetValueOrDefault("Role")?.S ?? "Public",
                CreatedDate = item.GetValueOrDefault("CreatedDate")?.S != null 
                    ? DateTime.Parse(item["CreatedDate"].S) 
                    : DateTime.UtcNow,
                UpdatedDate = item.GetValueOrDefault("UpdatedDate")?.S != null 
                    ? DateTime.Parse(item["UpdatedDate"].S) 
                    : null,
                IsActive = item.GetValueOrDefault("IsActive")?.BOOL ?? true
            };
        }
    }

    // Extension helper
    public static class DynamoDBExtensions
    {
        public static AttributeValue? GetValueOrDefault(this Dictionary<string, AttributeValue> dict, string key)
        {
            return dict.ContainsKey(key) ? dict[key] : null;
        }
    }
}
