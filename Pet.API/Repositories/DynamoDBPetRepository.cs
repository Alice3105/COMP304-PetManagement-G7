using Amazon.DynamoDBv2.Model;
using PetEntity = Pet.API.Models.Entities.Pet;
using Pet.API.Repositories.Interfaces;
using Pet.API.Services;
using Pet.API.Services.Interfaces;

namespace Pet.API.Repositories
{
    public class DynamoDBPetRepository : IPetRepository
    {
        private readonly IDynamoDBContext _dynamoDBContext;
        private readonly string _petsTableName;

        public DynamoDBPetRepository(IDynamoDBContext dynamoDBContext)
        {
            _dynamoDBContext = dynamoDBContext;
            _petsTableName = dynamoDBContext.PetsTableName;
        }

        public async Task<PetEntity> CreateAsync(PetEntity pet)
        {
            if (string.IsNullOrEmpty(pet.PetId))
            {
                pet.PetId = $"pet-{Guid.NewGuid():N}";
            }

            pet.CreatedDate = DateTime.UtcNow;
            pet.UpdatedDate = null;

            var item = new Dictionary<string, AttributeValue>
            {
                { "PetId", new AttributeValue { S = pet.PetId } },
                { "Name", new AttributeValue { S = pet.Name } },
                { "Species", new AttributeValue { S = pet.Species } },
                { "Breed", new AttributeValue { S = pet.Breed } },
                { "Age", new AttributeValue { N = pet.Age.ToString() } },
                { "Gender", new AttributeValue { S = pet.Gender } },
                { "Size", new AttributeValue { S = pet.Size } },
                { "Color", new AttributeValue { S = pet.Color } },
                { "Description", new AttributeValue { S = pet.Description } },
                { "Status", new AttributeValue { S = pet.Status } },
                { "IntakeDate", new AttributeValue { S = pet.IntakeDate.ToString("o") } },
                { "Vaccinated", new AttributeValue { BOOL = pet.Vaccinated } },
                { "Neutered", new AttributeValue { BOOL = pet.Neutered } },
                { "GoodWithKids", new AttributeValue { BOOL = pet.GoodWithKids } },
                { "GoodWithPets", new AttributeValue { BOOL = pet.GoodWithPets } },
                { "CreatedDate", new AttributeValue { S = pet.CreatedDate.ToString("o") } }
            };

            // Add PhotoUrls as a list
            if (pet.PhotoUrls != null && pet.PhotoUrls.Count > 0)
            {
                var photoList = new List<AttributeValue>();
                foreach (var url in pet.PhotoUrls)
                {
                    photoList.Add(new AttributeValue { S = url });
                }
                item["PhotoUrls"] = new AttributeValue { L = photoList };
            }

            var request = new PutItemRequest
            {
                TableName = _petsTableName,
                Item = item
            };

            await _dynamoDBContext.Client.PutItemAsync(request);

            return pet;
        }

        public async Task<PetEntity?> GetByIdAsync(string petId)
        {
            var request = new GetItemRequest
            {
                TableName = _petsTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "PetId", new AttributeValue { S = petId } }
                }
            };

            var response = await _dynamoDBContext.Client.GetItemAsync(request);

            if (response.Item == null || response.Item.Count == 0)
                return null;

            return MapToPet(response.Item);
        }

        public async Task<IEnumerable<PetEntity>> GetAllAsync()
        {
            var request = new ScanRequest
            {
                TableName = _petsTableName
            };

            var response = await _dynamoDBContext.Client.ScanAsync(request);

            var pets = new List<PetEntity>();
            foreach (var item in response.Items)
            {
                pets.Add(MapToPet(item));
            }

            return pets;
        }

        public async Task<PetEntity> UpdateAsync(PetEntity pet)
        {
            pet.UpdatedDate = DateTime.UtcNow;

            var item = new Dictionary<string, AttributeValue>
            {
                { "PetId", new AttributeValue { S = pet.PetId } },
                { "Name", new AttributeValue { S = pet.Name } },
                { "Species", new AttributeValue { S = pet.Species } },
                { "Breed", new AttributeValue { S = pet.Breed } },
                { "Age", new AttributeValue { N = pet.Age.ToString() } },
                { "Gender", new AttributeValue { S = pet.Gender } },
                { "Size", new AttributeValue { S = pet.Size } },
                { "Color", new AttributeValue { S = pet.Color } },
                { "Description", new AttributeValue { S = pet.Description } },
                { "Status", new AttributeValue { S = pet.Status } },
                { "IntakeDate", new AttributeValue { S = pet.IntakeDate.ToString("o") } },
                { "Vaccinated", new AttributeValue { BOOL = pet.Vaccinated } },
                { "Neutered", new AttributeValue { BOOL = pet.Neutered } },
                { "GoodWithKids", new AttributeValue { BOOL = pet.GoodWithKids } },
                { "GoodWithPets", new AttributeValue { BOOL = pet.GoodWithPets } },
                { "CreatedDate", new AttributeValue { S = pet.CreatedDate.ToString("o") } },
                { "UpdatedDate", new AttributeValue { S = pet.UpdatedDate.Value.ToString("o") } }
            };

            // Add PhotoUrls as a list
            if (pet.PhotoUrls != null && pet.PhotoUrls.Count > 0)
            {
                var photoList = new List<AttributeValue>();
                foreach (var url in pet.PhotoUrls)
                {
                    photoList.Add(new AttributeValue { S = url });
                }
                item["PhotoUrls"] = new AttributeValue { L = photoList };
            }

            var request = new PutItemRequest
            {
                TableName = _petsTableName,
                Item = item
            };

            await _dynamoDBContext.Client.PutItemAsync(request);

            return pet;
        }

        public async Task DeleteAsync(string petId)
        {
            var request = new DeleteItemRequest
            {
                TableName = _petsTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "PetId", new AttributeValue { S = petId } }
                }
            };

            await _dynamoDBContext.Client.DeleteItemAsync(request);
        }

        private PetEntity MapToPet(Dictionary<string, AttributeValue> item)
        {
            var pet = new PetEntity
            {
                PetId = item.GetValueOrDefault("PetId")?.S ?? "",
                Name = item.GetValueOrDefault("Name")?.S ?? "",
                Species = item.GetValueOrDefault("Species")?.S ?? "",
                Breed = item.GetValueOrDefault("Breed")?.S ?? "",
                Age = int.TryParse(item.GetValueOrDefault("Age")?.N, out var age) ? age : 0,
                Gender = item.GetValueOrDefault("Gender")?.S ?? "",
                Size = item.GetValueOrDefault("Size")?.S ?? "",
                Color = item.GetValueOrDefault("Color")?.S ?? "",
                Description = item.GetValueOrDefault("Description")?.S ?? "",
                Status = item.GetValueOrDefault("Status")?.S ?? "Available",
                Vaccinated = item.GetValueOrDefault("Vaccinated")?.BOOL ?? false,
                Neutered = item.GetValueOrDefault("Neutered")?.BOOL ?? false,
                GoodWithKids = item.GetValueOrDefault("GoodWithKids")?.BOOL ?? false,
                GoodWithPets = item.GetValueOrDefault("GoodWithPets")?.BOOL ?? false
            };

            // Parse IntakeDate
            if (item.GetValueOrDefault("IntakeDate")?.S != null)
            {
                pet.IntakeDate = DateTime.Parse(item["IntakeDate"].S);
            }

            // Parse CreatedDate
            if (item.GetValueOrDefault("CreatedDate")?.S != null)
            {
                pet.CreatedDate = DateTime.Parse(item["CreatedDate"].S);
            }

            // Parse UpdatedDate
            if (item.GetValueOrDefault("UpdatedDate")?.S != null)
            {
                pet.UpdatedDate = DateTime.Parse(item["UpdatedDate"].S);
            }

            // Parse PhotoUrls list
            if (item.GetValueOrDefault("PhotoUrls")?.L != null)
            {
                pet.PhotoUrls = item["PhotoUrls"].L
                    .Where(av => av.S != null)
                    .Select(av => av.S)
                    .ToList();
            }

            return pet;
        }
    }
}
