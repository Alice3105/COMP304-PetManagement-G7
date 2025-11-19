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

            Dictionary<string, AttributeValue> item = new Dictionary<string, AttributeValue>
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

            PutItemRequest request = new PutItemRequest
            {
                TableName = _adoptionsTableName,
                Item = item
            };

            await _dynamoDBContext.Client.PutItemAsync(request);

            return adoption;
        }

        public async Task<Adoption?> GetByIdAsync(string adoptionId)
        {
            GetItemRequest request = new GetItemRequest
            {
                TableName = _adoptionsTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "AdoptionId", new AttributeValue { S = adoptionId } }
                }
            };

            GetItemResponse response = await _dynamoDBContext.Client.GetItemAsync(request);

            if (response.Item == null || response.Item.Count == 0)
                return null;

            return MapToAdoption(response.Item);
        }

        public async Task<IEnumerable<Adoption>> GetAllAsync()
        {
            ScanRequest request = new ScanRequest
            {
                TableName = _adoptionsTableName
            };

            ScanResponse response = await _dynamoDBContext.Client.ScanAsync(request);

            List<Adoption> adoptions = new List<Adoption>();
            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                adoptions.Add(MapToAdoption(item));
            }

            return adoptions.OrderByDescending(a => a.ApplicationDate);
        }

        public async Task<IEnumerable<Adoption>> GetByUserIdAsync(string userId)
        {
            ScanRequest request = new ScanRequest
            {
                TableName = _adoptionsTableName,
                FilterExpression = "UserId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = userId } }
                }
            };

            ScanResponse response = await _dynamoDBContext.Client.ScanAsync(request);

            List<Adoption> adoptions = new List<Adoption>();
            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                adoptions.Add(MapToAdoption(item));
            }

            return adoptions.OrderByDescending(a => a.ApplicationDate);
        }

        public async Task<IEnumerable<Adoption>> GetByPetIdAsync(string petId)
        {
            ScanRequest request = new ScanRequest
            {
                TableName = _adoptionsTableName,
                FilterExpression = "PetId = :petId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":petId", new AttributeValue { S = petId } }
                }
            };

            ScanResponse response = await _dynamoDBContext.Client.ScanAsync(request);

            List<Adoption> adoptions = new List<Adoption>();
            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                adoptions.Add(MapToAdoption(item));
            }

            return adoptions.OrderByDescending(a => a.ApplicationDate);
        }

        public async Task<Adoption> UpdateAsync(Adoption adoption)
        {
            if (string.IsNullOrEmpty(adoption.AdoptionId))
                throw new Exception("AdoptionId is required for update");

            Adoption? existingAdoption = await GetByIdAsync(adoption.AdoptionId);
            if (existingAdoption == null)
                throw new Exception("Adoption not found");

            // Preserve fields that shouldn't be updated
            adoption.PetId = existingAdoption.PetId;
            adoption.PetName = existingAdoption.PetName;
            adoption.UserId = existingAdoption.UserId;
            adoption.UserEmail = existingAdoption.UserEmail;
            adoption.UserFirstName = existingAdoption.UserFirstName;
            adoption.UserLastName = existingAdoption.UserLastName;
            adoption.Status = existingAdoption.Status;
            adoption.ApplicationDate = existingAdoption.ApplicationDate;
            adoption.ReviewedDate = existingAdoption.ReviewedDate;
            adoption.ReviewedBy = existingAdoption.ReviewedBy;
            adoption.ReviewNotes = existingAdoption.ReviewNotes;

            Dictionary<string, AttributeValue> item = new Dictionary<string, AttributeValue>
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

            if (adoption.ReviewedDate.HasValue)
            {
                item["ReviewedDate"] = new AttributeValue { S = adoption.ReviewedDate.Value.ToString("o") };
            }

            if (!string.IsNullOrEmpty(adoption.ReviewedBy))
            {
                item["ReviewedBy"] = new AttributeValue { S = adoption.ReviewedBy };
            }

            if (!string.IsNullOrEmpty(adoption.ReviewNotes))
            {
                item["ReviewNotes"] = new AttributeValue { S = adoption.ReviewNotes };
            }

            PutItemRequest request = new PutItemRequest
            {
                TableName = _adoptionsTableName,
                Item = item
            };

            await _dynamoDBContext.Client.PutItemAsync(request);

            return adoption;
        }

        public async Task<Adoption> UpdateStatusAsync(string adoptionId, string status, string reviewedBy, string? reviewNotes = null)
        {
            Adoption? adoption = await GetByIdAsync(adoptionId);
            if (adoption == null)
                throw new Exception("Adoption not found");

            adoption.Status = status;
            adoption.ReviewedDate = DateTime.UtcNow;
            adoption.ReviewedBy = reviewedBy;
            adoption.ReviewNotes = reviewNotes;

            Dictionary<string, AttributeValue> item = new Dictionary<string, AttributeValue>
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

            PutItemRequest request = new PutItemRequest
            {
                TableName = _adoptionsTableName,
                Item = item
            };

            await _dynamoDBContext.Client.PutItemAsync(request);

            return adoption;
        }

        public async Task DeleteAsync(string adoptionId)
        {
            DeleteItemRequest request = new DeleteItemRequest
            {
                TableName = _adoptionsTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "AdoptionId", new AttributeValue { S = adoptionId } }
                }
            };

            await _dynamoDBContext.Client.DeleteItemAsync(request);
        }

        private Adoption MapToAdoption(Dictionary<string, AttributeValue> item)
        {
            Adoption adoption = new Adoption
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
