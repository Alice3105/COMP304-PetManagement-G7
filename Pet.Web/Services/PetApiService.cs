using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Pet.Web.Services
{
    public class PetApiService : IPetApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PetApiService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PetApiService(HttpClient httpClient, ILogger<PetApiService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthHeader()
        {
            var apiKey = _httpContextAccessor.HttpContext?.Session.GetString("ApiKey");
            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }
        }

        public async Task<List<PetViewModel>> GetAllPetsAsync()
        {
            try
            {
                AddAuthHeader();
                var response = await _httpClient.GetAsync("api/pets");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var pets = JsonSerializer.Deserialize<List<PetViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return pets ?? new List<PetViewModel>();
                }

                _logger.LogWarning($"Failed to fetch pets: {response.StatusCode}");
                return new List<PetViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pets from API");
                return new List<PetViewModel>();
            }
        }

        public async Task<PetViewModel?> GetPetByIdAsync(string petId)
        {
            try
            {
                AddAuthHeader();
                var response = await _httpClient.GetAsync($"api/pets/{petId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PetViewModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching pet {petId} from API");
                return null;
            }
        }

        public async Task<PetViewModel?> CreatePetAsync(CreatePetViewModel model)
        {
            try
            {
                AddAuthHeader();

                using var formData = new MultipartFormDataContent();
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
                    var fileContent = new StreamContent(model.Photo.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.Photo.ContentType);
                    formData.Add(fileContent, "Photo", model.Photo.FileName);
                }

                var response = await _httpClient.PostAsync("api/pets", formData);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PetViewModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning($"Failed to create pet: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pet via API");
                return null;
            }
        }

        public async Task<bool> UpdatePetAsync(string petId, PetViewModel model)
        {
            try
            {
                AddAuthHeader();
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/pets/{petId}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating pet {petId} via API");
                return false;
            }
        }

        public async Task<bool> DeletePetAsync(string petId)
        {
            try
            {
                AddAuthHeader();
                var response = await _httpClient.DeleteAsync($"api/pets/{petId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting pet {petId} via API");
                return false;
            }
        }

        private static void AddFormField(MultipartFormDataContent formData, string name, string value)
        {
            formData.Add(new StringContent(value), name);
        }
    }
}
