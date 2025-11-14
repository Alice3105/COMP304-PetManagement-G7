using Pet.Web.Models.ViewModels;
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
                formData.Add(new StringContent(model.Name), "Name");
                formData.Add(new StringContent(model.Species), "Species");
                formData.Add(new StringContent(model.Breed), "Breed");
                formData.Add(new StringContent(model.Age.ToString()), "Age");
                formData.Add(new StringContent(model.Gender), "Gender");
                formData.Add(new StringContent(model.Size), "Size");
                formData.Add(new StringContent(model.Color), "Color");
                formData.Add(new StringContent(model.Description), "Description");
                formData.Add(new StringContent(model.Vaccinated.ToString()), "Vaccinated");
                formData.Add(new StringContent(model.Neutered.ToString()), "Neutered");
                formData.Add(new StringContent(model.GoodWithKids.ToString()), "GoodWithKids");
                formData.Add(new StringContent(model.GoodWithPets.ToString()), "GoodWithPets");

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
    }
}
