using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Pet.Web.Services
{
    public class PetApiService : BaseApiService, IPetApiService
    {
        public PetApiService(HttpClient httpClient, ILogger<PetApiService> logger, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, logger, httpContextAccessor)
        {
        }

        public async Task<List<PetViewModel>> GetAllPetsAsync()
        {
            return await GetListAsync<PetViewModel>("api/pets", requireAuth: true);
        }

        public async Task<PetViewModel?> GetPetByIdAsync(string petId)
        {
            return await GetAsync<PetViewModel>($"api/pets/{petId}", requireAuth: true);
        }

        public async Task<PetViewModel?> CreatePetAsync(CreatePetViewModel model)
        {
            try
            {
                _logger.LogInformation($"CreatePetAsync called for pet: {model.Name}, Species: {model.Species}, Breed: {model.Breed}");
                
                string? apiKey = _httpContextAccessor?.HttpContext?.Session?.GetString("ApiKey");
                _logger.LogInformation($"API Key present in session: {!string.IsNullOrEmpty(apiKey)}");

                using MultipartFormDataContent formData = new MultipartFormDataContent();
                AddFormField(formData, "Name", model.Name);
                AddFormField(formData, "Species", model.Species);
                AddFormField(formData, "Breed", model.Breed);
                AddFormField(formData, "Age", model.Age.ToString());
                AddFormField(formData, "Gender", model.Gender);
                AddFormField(formData, "Size", model.Size);
                AddFormField(formData, "Color", model.Color);
                AddFormField(formData, "Description", model.Description);
                AddFormField(formData, "Vaccinated", model.Vaccinated.ToString());
                AddFormField(formData, "Neutered", model.Neutered.ToString());
                AddFormField(formData, "GoodWithKids", model.GoodWithKids.ToString());
                AddFormField(formData, "GoodWithPets", model.GoodWithPets.ToString());

                if (model.Photo != null)
                {
                    _logger.LogInformation($"Photo file provided: {model.Photo.FileName}, Size: {model.Photo.Length} bytes, ContentType: {model.Photo.ContentType}");
                    StreamContent fileContent = new StreamContent(model.Photo.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.Photo.ContentType);
                    formData.Add(fileContent, "Photo", model.Photo.FileName);
                }
                else
                {
                    _logger.LogInformation("No photo file provided");
                }

                string endpoint = "api/pets";
                string fullUrl = $"{_httpClient.BaseAddress}{endpoint}";
                _logger.LogInformation($"Sending POST request to: {fullUrl}");

                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = formData
                };

                // Set Authorization header on the request message
                if (!string.IsNullOrEmpty(apiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    _logger.LogInformation("Authorization header set on request");
                }
                else
                {
                    _logger.LogWarning("No API key found in session - request will be sent without authorization");
                }

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                _logger.LogInformation($"Response received: StatusCode={response.StatusCode}, IsSuccessStatusCode={response.IsSuccessStatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Response content length: {content.Length} characters");
                    var pet = JsonSerializer.Deserialize<PetViewModel>(content, JsonOptions);
                    _logger.LogInformation($"Pet created successfully: {pet?.PetId} - {pet?.Name}");
                    return pet;
                }

                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Failed to create pet: StatusCode={response.StatusCode}, Response={errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating pet via API. Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<bool> UpdatePetAsync(string petId, PetViewModel model)
        {
            return await PutAsync($"api/pets/{petId}", model, requireAuth: true);
        }

        public async Task<bool> DeletePetAsync(string petId)
        {
            return await DeleteAsync($"api/pets/{petId}", requireAuth: true);
        }

        private static void AddFormField(MultipartFormDataContent formData, string name, string value)
        {
            formData.Add(new StringContent(value), name);
        }
    }
}
