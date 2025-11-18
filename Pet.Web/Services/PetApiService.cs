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
                AddAuthHeader();

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
                    StreamContent fileContent = new StreamContent(model.Photo.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.Photo.ContentType);
                    formData.Add(fileContent, "Photo", model.Photo.FileName);
                }

                HttpResponseMessage response = await _httpClient.PostAsync("api/pets", formData);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PetViewModel>(content, JsonOptions);
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
