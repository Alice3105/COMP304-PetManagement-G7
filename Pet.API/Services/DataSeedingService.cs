using Pet.API.Models.Entities;
using Pet.API.Models.Enums;
using Pet.API.Repositories.Interfaces;
using Pet.API.Services.Interfaces;
using PetEntity = Pet.API.Models.Entities.Pet;
using System.Text.Json;

namespace Pet.API.Services
{
    public class DataSeedingService : IDataSeedingService
    {
        private readonly IDynamoDBContext _dynamoDBContext;
        private readonly IPetRepository _petRepository;
        private readonly IAdoptionRepository _adoptionRepository;
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<DataSeedingService> _logger;
        private readonly IConfiguration _configuration;

        public DataSeedingService(
            IDynamoDBContext dynamoDBContext,
            IPetRepository petRepository,
            IAdoptionRepository adoptionRepository,
            IMedicalRecordRepository medicalRecordRepository,
            IFileUploadService fileUploadService,
            ILogger<DataSeedingService> logger,
            IConfiguration configuration)
        {
            _dynamoDBContext = dynamoDBContext;
            _petRepository = petRepository;
            _adoptionRepository = adoptionRepository;
            _medicalRecordRepository = medicalRecordRepository;
            _fileUploadService = fileUploadService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> IsDataSeededAsync()
        {
            try
            {
                var pets = await _petRepository.GetAllAsync();
                return pets.Any();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking if data is seeded. Will attempt to seed.");
                return false;
            }
        }

        public async Task SeedDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting data seeding...");

              
                if (await IsDataSeededAsync())
                {
                    _logger.LogInformation("Data already seeded. Skipping seeding process.");
                    return;
                }

             
                var users = await SeedUsersAsync();
                _logger.LogInformation($"Seeded {users.Count} users");

               
                var pets = await SeedPetsAsync();
                _logger.LogInformation($"Seeded {pets.Count} pets");

              
                var adoptions = await SeedAdoptionsAsync(users, pets);
                _logger.LogInformation($"Seeded {adoptions.Count} adoption applications");

                var medicalRecords = await SeedMedicalRecordsAsync(users, pets);
                _logger.LogInformation($"Seeded {medicalRecords.Count} medical records");

                _logger.LogInformation("Data seeding completed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data seeding");
                throw;
            }
        }

        private async Task<List<ApplicationUser>> SeedUsersAsync()
        {
            var users = new List<ApplicationUser>();
            var defaultPassword = "Password123!"; // Default password for all seeded users
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);

            var userData = new[]
            {
               
                new { FirstName = "Admin", LastName = "User", Email = "admin@petshelter.com", Role = RoleConstants.Admin },
                new { FirstName = "John", LastName = "Administrator", Email = "john.admin@petshelter.com", Role = RoleConstants.Admin },
                
           
                new { FirstName = "Sarah", LastName = "Staff", Email = "sarah.staff@petshelter.com", Role = RoleConstants.Staff },
                new { FirstName = "Mike", LastName = "Manager", Email = "mike.manager@petshelter.com", Role = RoleConstants.Staff },
                new { FirstName = "Emily", LastName = "Caregiver", Email = "emily.caregiver@petshelter.com", Role = RoleConstants.Staff },
                new { FirstName = "David", LastName = "Veterinarian", Email = "david.vet@petshelter.com", Role = RoleConstants.Staff },
                
           
                new { FirstName = "Alice", LastName = "Public", Email = "alice.public@example.com", Role = RoleConstants.Public },
                new { FirstName = "Bob", LastName = "Smith", Email = "bob.smith@example.com", Role = RoleConstants.Public },
                new { FirstName = "Carol", LastName = "Johnson", Email = "carol.johnson@example.com", Role = RoleConstants.Public },
                new { FirstName = "Daniel", LastName = "Williams", Email = "daniel.williams@example.com", Role = RoleConstants.Public }
            };

            foreach (var userInfo in userData)
            {
                var user = new ApplicationUser
                {
                    UserId = $"user-{Guid.NewGuid():N}",
                    Email = userInfo.Email,
                    PasswordHash = passwordHash,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    Role = userInfo.Role,
                    ApiKey = $"sk_live_{Guid.NewGuid():N}",
                    CreatedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 90)),
                    IsActive = true
                };

                await _dynamoDBContext.CreateUserAsync(user);
                users.Add(user);
                _logger.LogInformation($"Created user: {user.Email} ({user.Role})");
            }

            return users;
        }

        private async Task<List<PetEntity>> SeedPetsAsync()
        {
            var pets = new List<PetEntity>();
            var petData = new[]
            {
                new { Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", Age = 3, Gender = "Male", Size = "Large", Color = "Golden", Description = "Friendly and energetic golden retriever. Loves playing fetch and going for walks.", Status = "Available", Vaccinated = true, Neutered = true, GoodWithKids = true, GoodWithPets = true },
                new { Name = "Luna", Species = "Cat", Breed = "Siamese", Age = 2, Gender = "Female", Size = "Medium", Color = "Cream", Description = "Elegant Siamese cat with beautiful blue eyes. Very affectionate and playful.", Status = "Available", Vaccinated = true, Neutered = true, GoodWithKids = true, GoodWithPets = false },
                new { Name = "Max", Species = "Dog", Breed = "German Shepherd", Age = 4, Gender = "Male", Size = "Large", Color = "Black and Tan", Description = "Loyal and protective German Shepherd. Great with families and very trainable.", Status = "Pending", Vaccinated = true, Neutered = true, GoodWithKids = true, GoodWithPets = true },
                new { Name = "Whiskers", Species = "Cat", Breed = "Persian", Age = 5, Gender = "Male", Size = "Medium", Color = "White", Description = "Calm and gentle Persian cat. Perfect for a quiet home environment.", Status = "Available", Vaccinated = true, Neutered = true, GoodWithKids = false, GoodWithPets = true },
                new { Name = "Bella", Species = "Dog", Breed = "Labrador", Age = 2, Gender = "Female", Size = "Large", Color = "Chocolate", Description = "Playful and friendly Labrador. Loves water and outdoor activities.", Status = "Adopted", Vaccinated = true, Neutered = true, GoodWithKids = true, GoodWithPets = true },
                new { Name = "Shadow", Species = "Cat", Breed = "Maine Coon", Age = 3, Gender = "Male", Size = "Large", Color = "Black", Description = "Large and majestic Maine Coon. Very social and friendly with people.", Status = "Available", Vaccinated = true, Neutered = true, GoodWithKids = true, GoodWithPets = true },
                new { Name = "Rocky", Species = "Dog", Breed = "Bulldog", Age = 6, Gender = "Male", Size = "Medium", Color = "Brindle", Description = "Calm and gentle bulldog. Perfect companion for apartment living.", Status = "MedicalHold", Vaccinated = true, Neutered = true, GoodWithKids = true, GoodWithPets = false },
                new { Name = "Mittens", Species = "Cat", Breed = "Ragdoll", Age = 1, Gender = "Female", Size = "Medium", Color = "Seal Point", Description = "Young and playful Ragdoll. Very affectionate and loves to cuddle.", Status = "Available", Vaccinated = true, Neutered = true, GoodWithKids = true, GoodWithPets = true },
                new { Name = "Charlie", Species = "Dog", Breed = "Beagle", Age = 4, Gender = "Male", Size = "Medium", Color = "Tri-color", Description = "Curious and friendly Beagle. Great sense of smell and loves exploring.", Status = "Available", Vaccinated = true, Neutered = true, GoodWithKids = true, GoodWithPets = true },
                new { Name = "Princess", Species = "Cat", Breed = "British Shorthair", Age = 3, Gender = "Female", Size = "Medium", Color = "Blue", Description = "Calm and dignified British Shorthair. Independent but affectionate.", Status = "Pending", Vaccinated = true, Neutered = true, GoodWithKids = true, GoodWithPets = true }
            };

            foreach (var petInfo in petData)
            {
                var pet = new PetEntity
                {
                    PetId = $"pet-{Guid.NewGuid():N}",
                    Name = petInfo.Name,
                    Species = petInfo.Species,
                    Breed = petInfo.Breed,
                    Age = petInfo.Age,
                    Gender = petInfo.Gender,
                    Size = petInfo.Size,
                    Color = petInfo.Color,
                    Description = petInfo.Description,
                    Status = petInfo.Status,
                    Vaccinated = petInfo.Vaccinated,
                    Neutered = petInfo.Neutered,
                    GoodWithKids = petInfo.GoodWithKids,
                    GoodWithPets = petInfo.GoodWithPets,
                    IntakeDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 180)),
                    CreatedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 180))
                };

                // Generate 1-3 photos for each pet
                var photoCount = Random.Shared.Next(1, 4);
                _logger.LogInformation($"Generating {photoCount} photos for pet {pet.Name} ({pet.Species})");
                
                for (int i = 1; i <= photoCount; i++)
                {
                    try
                    {
                        _logger.LogInformation($"Attempting to download and upload photo {i} for {pet.Name}...");
                        var photoUrl = await GenerateAndUploadPetImageAsync(
                            pet.Species,
                            pet.Name,
                            pet.Breed,
                            pet.PetId,
                            i);
                        pet.PhotoUrls.Add(photoUrl);
                        _logger.LogInformation($"Successfully added photo {i} for {pet.Name}: {photoUrl}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to generate photo {i} for pet {pet.Name}. Error: {ex.Message}");
                        // Fallback to generated URL if download/upload fails
                        var fallbackUrl = GeneratePetPhotoUrl(pet.Name, pet.PetId, i);
                        pet.PhotoUrls.Add(fallbackUrl);
                        _logger.LogWarning($"Using fallback URL for {pet.Name} photo {i}: {fallbackUrl}");
                    }
                }

                await _petRepository.CreateAsync(pet);
                pets.Add(pet);
                _logger.LogInformation($"Created pet: {pet.Name} ({pet.Species})");
            }

            return pets;
        }

        private async Task<List<Adoption>> SeedAdoptionsAsync(List<ApplicationUser> users, List<PetEntity> pets)
        {
            var adoptions = new List<Adoption>();
            var publicUsers = users.Where(u => u.Role == RoleConstants.Public).ToList();
            var staffUsers = users.Where(u => u.Role == RoleConstants.Staff || u.Role == RoleConstants.Admin).ToList();
            var availablePets = pets.Where(p => p.Status == "Available" || p.Status == "Pending").ToList();

            var statuses = new[] { "Pending", "Approved", "Rejected" };
            var housingTypes = new[] { "House", "Apartment", "Condo", "Townhouse" };
            var employmentStatuses = new[] { "Employed", "Self-Employed", "Retired", "Student" };

            // Create 10 adoption applications
            for (int i = 0; i < 10; i++)
            {
                var user = publicUsers[Random.Shared.Next(publicUsers.Count)];
                var pet = availablePets[Random.Shared.Next(availablePets.Count)];
                var status = statuses[Random.Shared.Next(statuses.Length)];
                var housingType = housingTypes[Random.Shared.Next(housingTypes.Length)];
                var employmentStatus = employmentStatuses[Random.Shared.Next(employmentStatuses.Length)];

                var adoption = new Adoption
                {
                    AdoptionId = $"adoption-{Guid.NewGuid():N}",
                    PetId = pet.PetId,
                    PetName = pet.Name,
                    UserId = user.UserId,
                    UserEmail = user.Email,
                    UserFirstName = user.FirstName,
                    UserLastName = user.LastName,
                    PhoneNumber = $"{Random.Shared.Next(200, 999)}-{Random.Shared.Next(200, 999)}-{Random.Shared.Next(1000, 9999)}",
                    Address = $"{Random.Shared.Next(100, 9999)} {GetRandomStreetName()} St, City, State {Random.Shared.Next(10000, 99999)}",
                    HousingType = housingType,
                    HasYard = Random.Shared.Next(2) == 1,
                    HasOtherPets = Random.Shared.Next(2) == 1,
                    OtherPetsDescription = Random.Shared.Next(2) == 1 ? "I have a friendly dog at home" : "",
                    HasChildren = Random.Shared.Next(2) == 1,
                    ChildrenAges = Random.Shared.Next(2) == 1 ? "5, 8" : "",
                    EmploymentStatus = employmentStatus,
                    Reason = $"I have always wanted a {pet.Species.ToLower()} and I believe {pet.Name} would be a perfect addition to my family.",
                    Status = status,
                    ApplicationDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 60))
                };

                // If status is Approved or Rejected, add review information
                if (status != "Pending" && staffUsers.Any())
                {
                    var reviewer = staffUsers[Random.Shared.Next(staffUsers.Count)];
                    adoption.ReviewedBy = reviewer.UserId;
                    adoption.ReviewedDate = adoption.ApplicationDate.AddDays(Random.Shared.Next(1, 7));
                    adoption.ReviewNotes = status == "Approved" 
                        ? "Application approved after thorough review. Applicant meets all requirements."
                        : "Application rejected due to housing constraints.";
                }

                await _adoptionRepository.CreateAsync(adoption);
                adoptions.Add(adoption);
                _logger.LogInformation($"Created adoption application: {adoption.AdoptionId} for {pet.Name} by {user.Email} - Status: {status}");
            }

            return adoptions;
        }

        private async Task<List<MedicalRecord>> SeedMedicalRecordsAsync(List<ApplicationUser> users, List<PetEntity> pets)
        {
            var medicalRecords = new List<MedicalRecord>();
            var staffUsers = users.Where(u => u.Role == RoleConstants.Staff || u.Role == RoleConstants.Admin).ToList();
            
            if (!staffUsers.Any())
            {
                _logger.LogWarning("No staff users found for medical records. Skipping medical record seeding.");
                return medicalRecords;
            }

            var recordTypes = new[] { "Vaccination", "General Checkup", "Treatment", "Follow-up", "Surgery" };
            var vaccineNames = new Dictionary<string, string[]>
            {
                { "Dog", new[] { "DHPP (Distemper, Hepatitis, Parvovirus, Parainfluenza)", "Rabies", "Bordetella", "Lyme", "Canine Influenza" } },
                { "Cat", new[] { "FVRCP", "Rabies", "FeLV (Feline Leukemia)", "FIV", "Feline Distemper" } }
            };

            foreach (var pet in pets)
            {
                var recordCount = Random.Shared.Next(2, 5);
                var veterinarian = staffUsers[Random.Shared.Next(staffUsers.Count)];
                var vetName = $"Dr. {veterinarian.FirstName} {veterinarian.LastName}";

                for (int i = 0; i < recordCount; i++)
                {
                    var recordType = recordTypes[Random.Shared.Next(recordTypes.Length)];
                    var recordDate = pet.IntakeDate.AddDays(Random.Shared.Next(0, 180));
                    
                    var record = new MedicalRecord
                    {
                        RecordId = $"medical-{Guid.NewGuid():N}",
                        PetId = pet.PetId,
                        PetName = pet.Name,
                        RecordType = recordType,
                        RecordDate = recordDate,
                        VeterinarianId = veterinarian.UserId,
                        VeterinarianName = vetName,
                        Description = GenerateMedicalRecordDescription(recordType, pet),
                        Cost = GenerateMedicalRecordCost(recordType),
                        Notes = GenerateMedicalRecordNotes(recordType, pet),
                        CreatedDate = recordDate
                    };

                    if (recordType == "Vaccination")
                    {
                        var vaccines = vaccineNames.GetValueOrDefault(pet.Species, new[] { "Standard Vaccine" });
                        record.VaccineName = vaccines[Random.Shared.Next(vaccines.Length)];
                        
                        if (record.VaccineName == "Rabies")
                        {
                            record.NextDueDate = recordDate.AddYears(Random.Shared.Next(1, 4));
                        }
                        else
                        {
                            record.NextDueDate = recordDate.AddYears(1);
                        }
                    }
                    else if (recordType == "Treatment" || recordType == "Surgery")
                    {
                        record.NextDueDate = recordDate.AddDays(Random.Shared.Next(7, 30));
                    }
                    else if (recordType == "General Checkup")
                    {
                        record.NextDueDate = recordDate.AddMonths(Random.Shared.Next(3, 12));
                    }

                    await _medicalRecordRepository.CreateAsync(record);
                    medicalRecords.Add(record);
                    _logger.LogInformation($"Created medical record: {record.RecordType} for {pet.Name} on {record.RecordDate:yyyy-MM-dd}");
                }
            }

            return medicalRecords;
        }

        private string GenerateMedicalRecordDescription(string recordType, PetEntity pet)
        {
            return recordType switch
            {
                "Vaccination" => $"Routine vaccination administered to {pet.Name}. Pet showed no adverse reactions.",
                "General Checkup" => $"Routine wellness examination for {pet.Name}. Physical exam shows no abnormalities.",
                "Treatment" => $"Medical treatment provided for {pet.Name}. Condition monitored and managed.",
                "Follow-up" => $"Follow-up appointment for {pet.Name}. Previous treatment progress reviewed.",
                "Surgery" => $"Surgical procedure performed on {pet.Name}. Procedure completed successfully without complications.",
                _ => $"Medical record for {pet.Name}."
            };
        }

        private decimal GenerateMedicalRecordCost(string recordType)
        {
            return recordType switch
            {
                "Vaccination" => Random.Shared.Next(30, 60),
                "General Checkup" => Random.Shared.Next(50, 80),
                "Treatment" => Random.Shared.Next(70, 150),
                "Follow-up" => Random.Shared.Next(30, 60),
                "Surgery" => Random.Shared.Next(200, 500),
                _ => Random.Shared.Next(40, 100)
            };
        }

        private string GenerateMedicalRecordNotes(string recordType, PetEntity pet)
        {
            var notes = recordType switch
            {
                "Vaccination" => $"Pet is healthy and up to date on vaccinations. Next checkup recommended in 6 months.",
                "General Checkup" => $"Weight: {Random.Shared.Next(10, 80)} lbs. Heart rate normal. Teeth in good condition. Overall excellent health.",
                "Treatment" => $"Treatment plan established. Follow-up appointment scheduled to check progress.",
                "Follow-up" => $"Previous condition has improved. Continue monitoring as needed.",
                "Surgery" => $"Post-op recovery normal. Pain medication prescribed. Suture removal scheduled for 14 days.",
                _ => $"Routine medical care provided."
            };

            if (pet.Species == "Cat")
            {
                notes = notes.Replace("lbs", "lbs").Replace("Weight: ", "Weight: ").Replace("Overall excellent health", "Overall excellent health. Recommend dental cleaning at next visit.");
            }

            return notes;
        }

        private async Task<string> GenerateAndUploadPetImageAsync(string species, string petName, string breed, string petId, int increment)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                string imageUrl;
                
                if (species.ToLower() == "dog")
                {
                    imageUrl = await GetDogImageUrlAsync(httpClient, breed);
                }
                else
                {
                    imageUrl = await GetCatImageUrlAsync(httpClient, breed);
                }
                
                var response = await httpClient.GetAsync(imageUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to download image from {imageUrl}. Status: {response.StatusCode}");
                }
                
                using var imageStream = await response.Content.ReadAsStreamAsync();
                var sanitizedName = petName.Replace(" ", "-");
                var fileName = $"{sanitizedName}-{increment:D3}.jpg";
                var folder = $"pets/{petId}/";
                var uploadedUrl = await _fileUploadService.UploadImageAsync(imageStream, fileName, folder);
                
                _logger.LogInformation($"Successfully uploaded image for {petName} ({species}): {uploadedUrl}");
                return uploadedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading/uploading image for {petName}");
                throw;
            }
        }

        private async Task<string> GetDogImageUrlAsync(HttpClient httpClient, string breed)
        {
            try
            {
                var apiBreedPath = MapDogBreedToApiFormat(breed);
                var apiEndpoint = $"https://dog.ceo/api/breed/{apiBreedPath}/images/random";
                
                var apiResponse = await httpClient.GetStringAsync(apiEndpoint);
                using var doc = JsonDocument.Parse(apiResponse);
                
                if (doc.RootElement.TryGetProperty("message", out var messageElement) && 
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    var imageUrl = messageElement.GetString();
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        _logger.LogInformation($"Fetched breed-specific image for {breed} ({apiBreedPath})");
                        return imageUrl;
                    }
                }
                
                throw new Exception("Breed-specific image URL is empty");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to get breed-specific image for {breed}, trying random image");
                // Fallback to random image
                var apiResponse = await httpClient.GetStringAsync("https://dog.ceo/api/breeds/image/random");
                using var doc = JsonDocument.Parse(apiResponse);
                if (doc.RootElement.TryGetProperty("message", out var messageElement) && 
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    return messageElement.GetString() ?? throw new Exception("Random image URL is empty");
                }
                throw new Exception("Failed to parse random image URL from Dog API");
            }
        }

        private async Task<string> GetCatImageUrlAsync(HttpClient httpClient, string breed)
        {
            var catApiKey = _configuration["CatApi:ApiKey"];
            if (string.IsNullOrWhiteSpace(catApiKey))
            {
                throw new Exception("Cat API key is not configured in appsettings.json");
            }

            if (!httpClient.DefaultRequestHeaders.Contains("x-api-key"))
            {
                httpClient.DefaultRequestHeaders.Add("x-api-key", catApiKey);
            }

            try
            {
                var breedId = MapCatBreedToApiFormat(breed);
                var apiEndpoint = $"https://api.thecatapi.com/v1/images/search?breed_ids={breedId}";
                
                var apiResponse = await httpClient.GetStringAsync(apiEndpoint);
                using var doc = JsonDocument.Parse(apiResponse);
                
                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                {
                    var firstImage = doc.RootElement[0];
                    if (firstImage.TryGetProperty("url", out var urlElement) && 
                        urlElement.ValueKind == JsonValueKind.String)
                    {
                        var imageUrl = urlElement.GetString();
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            _logger.LogInformation($"Fetched breed-specific image for {breed} (breed_id: {breedId})");
                            return imageUrl;
                        }
                    }
                }
                
                throw new Exception("Breed-specific image URL is empty");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to get breed-specific image for {breed}, trying random image");
                // Fallback to random image
                var apiResponse = await httpClient.GetStringAsync("https://api.thecatapi.com/v1/images/search");
                using var doc = JsonDocument.Parse(apiResponse);
                
                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                {
                    var firstImage = doc.RootElement[0];
                    if (firstImage.TryGetProperty("url", out var urlElement) && 
                        urlElement.ValueKind == JsonValueKind.String)
                    {
                        return urlElement.GetString() ?? throw new Exception("Random image URL is empty");
                    }
                }
                throw new Exception("Failed to parse random image URL from Cat API");
            }
        }

        private string MapDogBreedToApiFormat(string breed)
        {
            if (string.IsNullOrWhiteSpace(breed))
                return "mix";

            var normalizedBreed = breed.ToLower().Trim();
            
            var breedMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "golden retriever", "retriever/golden" },
                { "golden", "retriever/golden" },
                { "german shepherd", "german/shepherd" },
                { "german", "german/shepherd" },
                { "labrador", "labrador" },
                { "labrador retriever", "labrador" },
                { "bulldog", "bulldog/english" },
                { "english bulldog", "bulldog/english" },
                { "boston bulldog", "bulldog/boston" },
                { "french bulldog", "bulldog/french" },
                { "beagle", "beagle" },
                { "boxer", "boxer" },
                { "poodle", "poodle/standard" },
                { "pug", "pug" },
                { "rottweiler", "rottweiler" },
                { "doberman", "doberman" },
                { "siberian husky", "husky" },
                { "husky", "husky" },
                { "border collie", "collie/border" },
                { "collie", "collie/border" },
                { "australian shepherd", "australian/shepherd" },
                { "corgi", "corgi/cardigan" },
                { "pembroke corgi", "corgi/pembroke" },
                { "cardigan corgi", "corgi/cardigan" },
                { "dachshund", "dachshund" },
                { "chihuahua", "chihuahua" },
                { "shiba inu", "shiba" },
                { "shiba", "shiba" },
                { "great dane", "dane/great" },
                { "mastiff", "mastiff/english" },
                { "english mastiff", "mastiff/english" },
                { "saint bernard", "stbernard" },
                { "st bernard", "stbernard" },
                { "bernese mountain dog", "mountain/bernese" },
                { "newfoundland", "newfoundland" },
                { "samoyed", "samoyed" },
                { "akita", "akita" },
                { "chow chow", "chow" },
                { "chow", "chow" },
                { "dalmatian", "dalmatian" },
                { "basset hound", "hound/basset" },
                { "bloodhound", "hound/blood" },
                { "afghan hound", "hound/afghan" },
                { "irish setter", "setter/irish" },
                { "english setter", "setter/english" },
                { "gordon setter", "setter/gordon" },
                { "cocker spaniel", "spaniel/cocker" },
                { "english springer spaniel", "springer/english" },
                { "brittany spaniel", "spaniel/brittany" },
                { "welsh springer spaniel", "spaniel/welsh" },
                { "yorkshire terrier", "terrier/yorkshire" },
                { "scottish terrier", "terrier/scottish" },
                { "west highland terrier", "terrier/westhighland" },
                { "jack russell terrier", "terrier/russell" },
                { "australian terrier", "terrier/australian" },
                { "boston terrier", "terrier/boston" },
                { "pitbull", "pitbull" },
                { "pit bull", "pitbull" },
                { "staffordshire bull terrier", "bullterrier/staffordshire" },
                { "australian cattle dog", "cattledog/australian" },
                { "blue heeler", "cattledog/australian" },
                { "kelpie", "kelpie" },
                { "australian kelpie", "kelpie" }
            };

            if (breedMappings.TryGetValue(normalizedBreed, out var mappedBreed))
            {
                return mappedBreed;
            }

            foreach (var mapping in breedMappings)
            {
                if (normalizedBreed.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase) ||
                    mapping.Key.Contains(normalizedBreed, StringComparison.OrdinalIgnoreCase))
                {
                    return mapping.Value;
                }
            }

            var apiFormat = normalizedBreed.Replace(" ", "");
            _logger.LogWarning($"No breed mapping found for '{breed}', using '{apiFormat}' (will fallback to random if fails)");
            return apiFormat;
        }

        private string MapCatBreedToApiFormat(string breed)
        {
            if (string.IsNullOrWhiteSpace(breed))
                return "pers"; // Default to Persian

            var normalizedBreed = breed.ToLower().Trim();
            
            var breedMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "abyssinian", "abys" },
                { "bengal", "beng" },
                { "birman", "birm" },
                { "bombay", "bomb" },
                { "british longhair", "bslo" },
                { "british shorthair", "bsho" },
                { "burmese", "bure" },
                { "chantilly-tiffany", "cspa" },
                { "chartreux", "ctif" },
                { "donskoy", "dons" },
                { "european burmese", "ebur" },
                { "egyptian mau", "emau" },
                { "himalayan", "hima" },
                { "japanese bobtail", "jbob" },
                { "khao manee", "khao" },
                { "korat", "kora" },
                { "maine coon", "mcoo" },
                { "malayan", "mala" },
                { "manx", "manx" },
                { "munchkin", "munc" },
                { "nebelung", "nebe" },
                { "norwegian forest cat", "norw" },
                { "ocicat", "ocic" },
                { "oriental", "orie" },
                { "persian", "pers" },
                { "ragamuffin", "raga" },
                { "ragdoll", "ragd" },
                { "russian blue", "rblu" },
                { "savannah", "sava" },
                { "scottish fold", "sfol" },
                { "siamese", "siam" },
                { "siberian", "sibe" },
                { "singapura", "sing" },
                { "snowshoe", "snow" },
                { "somali", "soma" },
                { "sphynx", "sphy" },
                { "tonkinese", "tonk" },
                { "toyger", "toyg" },
                { "turkish angora", "turk" },
                { "turkish van", "tvan" }
            };

            if (breedMappings.TryGetValue(normalizedBreed, out var breedId))
            {
                return breedId;
            }

            foreach (var mapping in breedMappings)
            {
                if (normalizedBreed.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase) ||
                    mapping.Key.Contains(normalizedBreed, StringComparison.OrdinalIgnoreCase))
                {
                    return mapping.Value;
                }
            }

            _logger.LogWarning($"No cat breed mapping found for '{breed}', using default 'pers' (Persian)");
            return "pers"; // Default to Persian
        }

        private string GeneratePetPhotoUrl(string petName, string petId, int increment)
        {
            var bucketName = _configuration["AWS:S3:BucketName"] ?? "petshelter-images";
            var sanitizedName = petName.Replace(" ", "-");
            var photoUrl = $"https://{bucketName}.s3.amazonaws.com/pets/{petId}/{sanitizedName}-{increment:D3}.jpg";
            return photoUrl;
        }


        private string GetRandomStreetName()
        {
            var streetNames = new[]
            {
                "Main", "Oak", "Elm", "Park", "Maple", "Cedar", "Pine", "First", "Second", "Third",
                "Washington", "Lincoln", "Jefferson", "Madison", "Adams", "Jackson", "Roosevelt"
            };
            return streetNames[Random.Shared.Next(streetNames.Length)];
        }
    }
}

