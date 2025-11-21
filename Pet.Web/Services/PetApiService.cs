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
            _logger.LogInformation($"Service: PetApiService, Method: GetAllPetsAsync");
            return await GetListAsync<PetViewModel>("api/pets", requireAuth: true);
        }

        public async Task<PetViewModel?> GetPetByIdAsync(string petId)
        {
            _logger.LogInformation($"Service: PetApiService, Method: GetPetByIdAsync, PetId: {petId}");
            return await GetAsync<PetViewModel>($"api/pets/{petId}", requireAuth: true);
        }

        public async Task<PetViewModel?> CreatePetAsync(CreatePetViewModel model)
        {
            _logger.LogInformation($"Service: PetApiService, Method: CreatePetAsync, PetName: {model?.Name ?? "unknown"}");
            try
            {
                if (model == null)
                {
                    _logger.LogError("CreatePetAsync called with null model");
                    return null;
                }
                
                _logger.LogInformation($"CreatePetAsync called for pet: {model.Name}, Species: {model.Species}, Breed: {model.Breed}");
                
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

                // Add user headers for authentication
                if (_httpContextAccessor?.HttpContext != null)
                {
                    string? userEmail = _httpContextAccessor.HttpContext.Session.GetString("Email");
                    string? userRole = _httpContextAccessor.HttpContext.Session.GetString("Role");
                    
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        request.Headers.Add("X-User-Email", userEmail);
                    }
                    
                    if (!string.IsNullOrEmpty(userRole))
                    {
                        request.Headers.Add("X-User-Role", userRole);
                    }
                }

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                _logger.LogInformation($"Response received: StatusCode={response.StatusCode}, IsSuccessStatusCode={response.IsSuccessStatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Response content length: {content.Length} characters");
                    PetViewModel? pet = JsonSerializer.Deserialize<PetViewModel>(content, JsonOptions);
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
            _logger.LogInformation($"Service: PetApiService, Method: UpdatePetAsync, PetId: {petId}");
            
            // If photo is provided, use multipart form data (PATCH)
            if (model.Photo != null && model.Photo.Length > 0)
            {
                return await UpdatePetWithPhotoAsync(petId, model);
            }
            
            // Otherwise use JSON (PUT)
            return await PutAsync($"api/pets/{petId}", model, requireAuth: true);
        }

        private async Task<bool> UpdatePetWithPhotoAsync(string petId, PetViewModel model)
        {
            _logger.LogInformation($"Service: PetApiService, Method: UpdatePetWithPhotoAsync, PetId: {petId}");
            try
            {
                if (model == null)
                {
                    _logger.LogError("UpdatePetWithPhotoAsync called with null model");
                    return false;
                }

                using MultipartFormDataContent formData = new MultipartFormDataContent();
                AddFormField(formData, "Name", model.Name);
                AddFormField(formData, "Species", model.Species);
                AddFormField(formData, "Breed", model.Breed);
                AddFormField(formData, "Age", model.Age.ToString());
                AddFormField(formData, "Gender", model.Gender);
                AddFormField(formData, "Size", model.Size);
                AddFormField(formData, "Color", model.Color);
                AddFormField(formData, "Description", model.Description);
                AddFormField(formData, "Status", model.Status);
                AddFormField(formData, "Vaccinated", model.Vaccinated.ToString());
                AddFormField(formData, "Neutered", model.Neutered.ToString());
                AddFormField(formData, "GoodWithKids", model.GoodWithKids.ToString());
                AddFormField(formData, "GoodWithPets", model.GoodWithPets.ToString());

                // Add photo file
                if (model.Photo != null && model.Photo.Length > 0)
                {
                    _logger.LogInformation($"Photo file provided for update: {model.Photo.FileName}, Size: {model.Photo.Length} bytes, ContentType: {model.Photo.ContentType}");
                    StreamContent fileContent = new StreamContent(model.Photo.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.Photo.ContentType);
                    formData.Add(fileContent, "Photo", model.Photo.FileName);
                }

                string endpoint = $"api/pets/{petId}";
                string fullUrl = $"{_httpClient.BaseAddress}{endpoint}";
                _logger.LogInformation($"Sending PATCH request to: {fullUrl}");

                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
                {
                    Content = formData
                };

                // Add user headers for authentication
                if (_httpContextAccessor?.HttpContext != null)
                {
                    string? userEmail = _httpContextAccessor.HttpContext.Session.GetString("Email");
                    string? userRole = _httpContextAccessor.HttpContext.Session.GetString("Role");
                    
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        request.Headers.Add("X-User-Email", userEmail);
                    }
                    
                    if (!string.IsNullOrEmpty(userRole))
                    {
                        request.Headers.Add("X-User-Role", userRole);
                    }
                }

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                _logger.LogInformation($"Response received: StatusCode={response.StatusCode}, IsSuccessStatusCode={response.IsSuccessStatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Pet {petId} updated successfully with photo");
                    return true;
                }

                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Failed to update pet with photo: StatusCode={response.StatusCode}, Response={errorContent}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating pet with photo via API. Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> DeletePetAsync(string petId)
        {
            _logger.LogInformation($"Service: PetApiService, Method: DeletePetAsync, PetId: {petId}");
            return await DeleteAsync($"api/pets/{petId}", requireAuth: true);
        }

        private static void AddFormField(MultipartFormDataContent formData, string name, string value)
        {
            formData.Add(new StringContent(value), name);
        }
    }
}
