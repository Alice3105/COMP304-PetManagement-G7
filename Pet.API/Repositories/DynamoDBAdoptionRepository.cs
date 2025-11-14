using Amazon.DynamoDBv2.Model;
using Pet.API.Models.Entities;
using Pet.API.Repositories.Interfaces;
using Pet.API.Services.Interfaces;

namespace Pet.API.Repositories
{
    public class DynamoDBAdoptionRepository : IAdoptionRepository
    {
        private readonly IDynamoDBContext _dynamoDBContext;
        private readonly string _adoptionsTableName;

        public DynamoDBAdoptionRepository(IDynamoDBContext dynamoDBContext)
        {
            _dynamoDBContext = dynamoDBContext;
            _adoptionsTableName = dynamoDBContext.AdoptionsTableName;
        }

        public async Task<Adoption> CreateAsync(Adoption adoption)
        {
            if (string.IsNullOrEmpty(adoption.AdoptionId))
            {
                adoption.AdoptionId = $"adoption-{Guid.NewGuid():N}";
            }

            adoption.ApplicationDate = DateTime.UtcNow;

            var item = new Dictionary<string, AttributeValue>
            {
                { "AdoptionId", new AttributeValue { S = adoption.AdoptionId } },
                { "PetId", new AttributeValue { S = adoption.PetId } },
                { "PetName", new AttributeValue { S = adoption.PetName } },
                { "UserId", new AttributeValue { S = adoption.UserId } },
                { "UserEmail", new AttributeValue { S = adoption.UserEmail } },
                { "UserFirstName", new AttributeValue { S = adoption.UserFirstName } },
                { "UserLastName", new AttributeValue { S = adoption.UserLastName } },
                { "PhoneNumber", new AttributeValue { S = adoption.PhoneNumber } },
                { "Address", new AttributeValue { S = adoption.Address } },
                { "HousingType", new AttributeValue { S = adoption.HousingType } },
                { "HasYard", new AttributeValue { BOOL = adoption.HasYard } },
                { "HasOtherPets", new AttributeValue { BOOL = adoption.HasOtherPets } },
                { "OtherPetsDescription", new AttributeValue { S = adoption.OtherPetsDescription ?? "" } },
                { "HasChildren", new AttributeValue { BOOL = adoption.HasChildren } },
                { "ChildrenAges", new AttributeValue { S = adoption.ChildrenAges ?? "" } },
                { "EmploymentStatus", new AttributeValue { S = adoption.EmploymentStatus } },
                { "Reason", new AttributeValue { S = adoption.Reason } },
                { "Status", new AttributeValue { S = adoption.Status } },
                { "ApplicationDate", new AttributeValue { S = adoption.ApplicationDate.ToString("o") } }
            };

            var request = new PutItemRequest
            {
                TableName = _adoptionsTableName,
                Item = item
            };

            await _dynamoDBContext.Client.PutItemAsync(request);

            return adoption;
        }

        public async Task<Adoption?> GetByIdAsync(string adoptionId)
        {
            var request = new GetItemRequest
            {
                TableName = _adoptionsTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "AdoptionId", new AttributeValue { S = adoptionId } }
                }
            };

            var response = await _dynamoDBContext.Client.GetItemAsync(request);

            if (response.Item == null || response.Item.Count == 0)
                return null;

            return MapToAdoption(response.Item);
        }

        public async Task<IEnumerable<Adoption>> GetAllAsync()
        {
            var request = new ScanRequest
            {
                TableName = _adoptionsTableName
            };

            var response = await _dynamoDBContext.Client.ScanAsync(request);

            var adoptions = new List<Adoption>();
            foreach (var item in response.Items)
            {
                adoptions.Add(MapToAdoption(item));
            }

            return adoptions.OrderByDescending(a => a.ApplicationDate);
        }

        public async Task<IEnumerable<Adoption>> GetByUserIdAsync(string userId)
        {
            var request = new ScanRequest
            {
                TableName = _adoptionsTableName,
                FilterExpression = "UserId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = userId } }
                }
            };

            var response = await _dynamoDBContext.Client.ScanAsync(request);

            var adoptions = new List<Adoption>();
            foreach (var item in response.Items)
            {
                adoptions.Add(MapToAdoption(item));
            }

            return adoptions.OrderByDescending(a => a.ApplicationDate);
        }

        public async Task<IEnumerable<Adoption>> GetByPetIdAsync(string petId)
        {
            var request = new ScanRequest
            {
                TableName = _adoptionsTableName,
                FilterExpression = "PetId = :petId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":petId", new AttributeValue { S = petId } }
                }
            };

            var response = await _dynamoDBContext.Client.ScanAsync(request);

            var adoptions = new List<Adoption>();
            foreach (var item in response.Items)
            {
                adoptions.Add(MapToAdoption(item));
            }

            return adoptions.OrderByDescending(a => a.ApplicationDate);
        }

        public async Task<Adoption> UpdateStatusAsync(string adoptionId, string status, string reviewedBy, string? reviewNotes = null)
        {
            var adoption = await GetByIdAsync(adoptionId);
            if (adoption == null)
                throw new Exception("Adoption not found");

            adoption.Status = status;
            adoption.ReviewedDate = DateTime.UtcNow;
            adoption.ReviewedBy = reviewedBy;
            adoption.ReviewNotes = reviewNotes;

            var item = new Dictionary<string, AttributeValue>
            {
                { "AdoptionId", new AttributeValue { S = adoption.AdoptionId } },
                { "PetId", new AttributeValue { S = adoption.PetId } },
                { "PetName", new AttributeValue { S = adoption.PetName } },
                { "UserId", new AttributeValue { S = adoption.UserId } },
                { "UserEmail", new AttributeValue { S = adoption.UserEmail } },
                { "UserFirstName", new AttributeValue { S = adoption.UserFirstName } },
                { "UserLastName", new AttributeValue { S = adoption.UserLastName } },
                { "PhoneNumber", new AttributeValue { S = adoption.PhoneNumber } },
                { "Address", new AttributeValue { S = adoption.Address } },
                { "HousingType", new AttributeValue { S = adoption.HousingType } },
                { "HasYard", new AttributeValue { BOOL = adoption.HasYard } },
                { "HasOtherPets", new AttributeValue { BOOL = adoption.HasOtherPets } },
                { "OtherPetsDescription", new AttributeValue { S = adoption.OtherPetsDescription ?? "" } },
                { "HasChildren", new AttributeValue { BOOL = adoption.HasChildren } },
                { "ChildrenAges", new AttributeValue { S = adoption.ChildrenAges ?? "" } },
                { "EmploymentStatus", new AttributeValue { S = adoption.EmploymentStatus } },
                { "Reason", new AttributeValue { S = adoption.Reason } },
                { "Status", new AttributeValue { S = adoption.Status } },
                { "ApplicationDate", new AttributeValue { S = adoption.ApplicationDate.ToString("o") } },
                { "ReviewedDate", new AttributeValue { S = adoption.ReviewedDate.Value.ToString("o") } },
                { "ReviewedBy", new AttributeValue { S = adoption.ReviewedBy } }
            };

            if (!string.IsNullOrEmpty(adoption.ReviewNotes))
            {
                item["ReviewNotes"] = new AttributeValue { S = adoption.ReviewNotes };
            }

            var request = new PutItemRequest
            {
                TableName = _adoptionsTableName,
                Item = item
            };

            await _dynamoDBContext.Client.PutItemAsync(request);

            return adoption;
        }

        private Adoption MapToAdoption(Dictionary<string, AttributeValue> item)
        {
            var adoption = new Adoption
            {
                AdoptionId = item.GetValueOrDefault("AdoptionId")?.S ?? "",
                PetId = item.GetValueOrDefault("PetId")?.S ?? "",
                PetName = item.GetValueOrDefault("PetName")?.S ?? "",
                UserId = item.GetValueOrDefault("UserId")?.S ?? "",
                UserEmail = item.GetValueOrDefault("UserEmail")?.S ?? "",
                UserFirstName = item.GetValueOrDefault("UserFirstName")?.S ?? "",
                UserLastName = item.GetValueOrDefault("UserLastName")?.S ?? "",
                PhoneNumber = item.GetValueOrDefault("PhoneNumber")?.S ?? "",
                Address = item.GetValueOrDefault("Address")?.S ?? "",
                HousingType = item.GetValueOrDefault("HousingType")?.S ?? "",
                HasYard = item.GetValueOrDefault("HasYard")?.BOOL ?? false,
                HasOtherPets = item.GetValueOrDefault("HasOtherPets")?.BOOL ?? false,
                OtherPetsDescription = item.GetValueOrDefault("OtherPetsDescription")?.S ?? "",
                HasChildren = item.GetValueOrDefault("HasChildren")?.BOOL ?? false,
                ChildrenAges = item.GetValueOrDefault("ChildrenAges")?.S ?? "",
                EmploymentStatus = item.GetValueOrDefault("EmploymentStatus")?.S ?? "",
                Reason = item.GetValueOrDefault("Reason")?.S ?? "",
                Status = item.GetValueOrDefault("Status")?.S ?? "Pending",
                ReviewedBy = item.GetValueOrDefault("ReviewedBy")?.S,
                ReviewNotes = item.GetValueOrDefault("ReviewNotes")?.S
            };

            // Parse ApplicationDate
            if (item.GetValueOrDefault("ApplicationDate")?.S != null)
            {
                adoption.ApplicationDate = DateTime.Parse(item["ApplicationDate"].S);
            }

            // Parse ReviewedDate
            if (item.GetValueOrDefault("ReviewedDate")?.S != null)
            {
                adoption.ReviewedDate = DateTime.Parse(item["ReviewedDate"].S);
            }

            return adoption;
        }
    }
}
