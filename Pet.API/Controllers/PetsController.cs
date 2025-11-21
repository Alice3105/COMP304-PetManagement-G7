using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Pet.API.Models.Entities;
using Pet.API.Models.DTOs;
using Pet.API.Repositories.Interfaces;
using Pet.API.Services.Interfaces;
using PetEntity = Pet.API.Models.Entities.Pet;

namespace Pet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController : BaseController
    {
        private readonly IPetRepository _petRepository;
        private readonly IFileUploadService _fileUploadService;
        private readonly IMapper _mapper;
        private readonly ILogger<PetsController> _logger;

        public PetsController(IPetRepository petRepository, IFileUploadService fileUploadService, IMapper mapper, ILogger<PetsController> logger) : base(logger)
        {
            _petRepository = petRepository;
            _fileUploadService = fileUploadService;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/pets
        // Returns all pets
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PetResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PetResponse>>> GetAll()
        {
            _logger.LogInformation($"Endpoint: GetAll, Method: GET");
            try
            {
                IEnumerable<PetEntity> pets = await _petRepository.GetAllAsync();
                IEnumerable<PetResponse> petResponses = _mapper.Map<IEnumerable<PetResponse>>(pets);
                return Ok(petResponses);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching all Pets");
                return StatusCode(500, new { message = "Error fetching Pets", error = ex.Message });
            }
        }

        // GET: api/pets/{id}
        // Returns a single pet by id
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PetResponse>> GetById(string id)
        {
            _logger.LogInformation($"Endpoint: GetById, Method: GET, PetId: {id}");
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Pet id is required." });

            try
            {
                PetEntity? pet = await _petRepository.GetByIdAsync(id);
                if (pet == null)
                    return NotFound(new { message = "Pet not found" });

                PetResponse petResponse = _mapper.Map<PetResponse>(pet);
                return Ok(petResponse);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error fetching Pet {id}");
                return StatusCode(500, new { message = "Error fetching Pet", error = ex.Message });
            }
        }

        // POST: api/pets
        // Create a new pet - handles both JSON and multipart form data (with file upload)
        [HttpPost]
        [ProducesResponseType(typeof(PetResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PetResponse>> Create()
        {
            _logger.LogInformation($"Endpoint: Create, Method: POST");
            // Check if request is multipart form data (file upload)
            if (Request.HasFormContentType)
            {
                return await CreateFromFormData();
            }

            // Handle JSON request
            try
            {
                CreatePetRequest? request = null;
                
                // Try to read JSON from body
                Request.EnableBuffering();
                Request.Body.Position = 0;

                using StreamReader reader = new StreamReader(Request.Body, leaveOpen: true);
                string jsonBody = await reader.ReadToEndAsync();
                Request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(jsonBody))
                {
                    request = System.Text.Json.JsonSerializer.Deserialize<CreatePetRequest>(
                        jsonBody,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }

                if (request == null)
                {
                    return BadRequest(new { message = "Pet data is required." });
                }

                PetEntity pet = _mapper.Map<PetEntity>(request);
                pet.PetId = Guid.NewGuid().ToString();

                // Save to database
                PetEntity createdPet = await _petRepository.CreateAsync(pet);
                
                // Map Entity to Response DTO
                PetResponse petResponse = _mapper.Map<PetResponse>(createdPet);
                
                Logger.LogInformation($"Pet created: {createdPet.PetId}");
                return CreatedAtAction(nameof(GetById), new { id = createdPet.PetId }, petResponse);
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing JSON pet data");
                return BadRequest(new { message = "Invalid JSON format.", error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing JSON request");
                return StatusCode(500, new { message = "Error processing request", error = ex.Message });
            }
        }

        // Helper method to create pet from multipart form data
        private async Task<ActionResult<PetResponse>> CreateFromFormData()
        {
            try
            {
                IFormCollection form = await Request.ReadFormAsync();
                
                string name = form["Name"].ToString();
                string species = form["Species"].ToString();
                string breed = form["Breed"].ToString();
                
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(species) || string.IsNullOrWhiteSpace(breed))
                {
                    return BadRequest(new { message = "Name, Species, and Breed are required." });
                }

                if (!int.TryParse(form["Age"].ToString(), out int age))
                    age = 0;

                string gender = form["Gender"].ToString();
                string size = form["Size"].ToString();
                string color = form["Color"].ToString();
                string description = form["Description"].ToString();
                
                bool vaccinated = bool.TryParse(form["Vaccinated"].ToString(), out bool v) && v;
                bool neutered = bool.TryParse(form["Neutered"].ToString(), out bool n) && n;
                bool goodWithKids = bool.TryParse(form["GoodWithKids"].ToString(), out bool gwk) && gwk;
                bool goodWithPets = bool.TryParse(form["GoodWithPets"].ToString(), out bool gwp) && gwp;

                // Create DTO from form data
                CreatePetRequest createRequest = new CreatePetRequest
                {
                    Name = name,
                    Species = species,
                    Breed = breed,
                    Age = age,
                    Gender = gender,
                    Size = size,
                    Color = color,
                    Description = description,
                    Vaccinated = vaccinated,
                    Neutered = neutered,
                    GoodWithKids = goodWithKids,
                    GoodWithPets = goodWithPets,
                    PhotoUrls = new List<string>()
                };

                // Map DTO to Entity
                PetEntity pet = _mapper.Map<PetEntity>(createRequest);
                pet.PetId = Guid.NewGuid().ToString();

                // Handle file upload
                if (form.Files != null && form.Files.Count > 0)
                {
                    IFormFile? photoFile = form.Files["Photo"];
                    if (photoFile != null && photoFile.Length > 0)
                    {
                        try
                        {
                            string folder = $"pets/{pet.PetId}/";
                            string fileName = $"{pet.Name.Replace(" ", "-")}-{Guid.NewGuid()}{Path.GetExtension(photoFile.FileName)}";
                            string photoUrl = await _fileUploadService.UploadImageAsync(photoFile.OpenReadStream(), fileName, folder);
                            pet.PhotoUrls.Add(photoUrl);
                            _logger.LogInformation($"Photo uploaded for pet {pet.PetId}: {photoUrl}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to upload photo for pet {pet.PetId}");
                        }
                    }
                }

                // Save to database
                PetEntity createdPet = await _petRepository.CreateAsync(pet);
                
                // Map Entity to Response DTO
                PetResponse petResponse = _mapper.Map<PetResponse>(createdPet);
                
                Logger.LogInformation($"Pet created: {createdPet.PetId}");
                return CreatedAtAction(nameof(GetById), new { id = createdPet.PetId }, petResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pet with file upload");
                return StatusCode(500, new { message = "Error creating pet", error = ex.Message });
            }
        }

        // PATCH: api/pets/{id}
        // Update pet with multipart form data (supports file upload)
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PetResponse>> PatchWithFile(string id)
        {
            _logger.LogInformation($"Endpoint: PatchWithFile, Method: PATCH, PetId: {id}");
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Pet id is required." });

            if (!Request.HasFormContentType)
                return BadRequest(new { message = "Request must be multipart/form-data." });

            try
            {
                PetEntity? existingPet = await _petRepository.GetByIdAsync(id);
                if (existingPet == null)
                    return NotFound(new { message = "Pet not found" });

                IFormCollection form = await Request.ReadFormAsync();

                // Create new pet entity with updated properties from form data
                PetEntity pet = new PetEntity
                {
                    PetId = existingPet.PetId,
                    Name = !string.IsNullOrWhiteSpace(form["Name"].ToString()) ? form["Name"].ToString()! : existingPet.Name,
                    Species = !string.IsNullOrWhiteSpace(form["Species"].ToString()) ? form["Species"].ToString()! : existingPet.Species,
                    Breed = !string.IsNullOrWhiteSpace(form["Breed"].ToString()) ? form["Breed"].ToString()! : existingPet.Breed,
                    Age = int.TryParse(form["Age"].ToString(), out int age) ? age : existingPet.Age,
                    Gender = !string.IsNullOrWhiteSpace(form["Gender"].ToString()) ? form["Gender"].ToString()! : existingPet.Gender,
                    Size = !string.IsNullOrWhiteSpace(form["Size"].ToString()) ? form["Size"].ToString()! : existingPet.Size,
                    Color = !string.IsNullOrWhiteSpace(form["Color"].ToString()) ? form["Color"].ToString()! : existingPet.Color,
                    Description = !string.IsNullOrWhiteSpace(form["Description"].ToString()) ? form["Description"].ToString()! : existingPet.Description,
                    Status = !string.IsNullOrWhiteSpace(form["Status"].ToString()) ? form["Status"].ToString()! : existingPet.Status,
                    Vaccinated = bool.TryParse(form["Vaccinated"].ToString(), out bool vaccinated) ? vaccinated : existingPet.Vaccinated,
                    Neutered = bool.TryParse(form["Neutered"].ToString(), out bool neutered) ? neutered : existingPet.Neutered,
                    GoodWithKids = bool.TryParse(form["GoodWithKids"].ToString(), out bool goodWithKids) ? goodWithKids : existingPet.GoodWithKids,
                    GoodWithPets = bool.TryParse(form["GoodWithPets"].ToString(), out bool goodWithPets) ? goodWithPets : existingPet.GoodWithPets,
                    IntakeDate = existingPet.IntakeDate,
                    CreatedDate = existingPet.CreatedDate,
                    PhotoUrls = existingPet.PhotoUrls != null ? new List<string>(existingPet.PhotoUrls) : new List<string>()
                };

                // Handle new photo upload - prepend to PhotoUrls list
                if (form.Files != null && form.Files.Count > 0)
                {
                    IFormFile? photoFile = form.Files["Photo"];
                    if (photoFile != null && photoFile.Length > 0)
                    {
                        try
                        {
                            string folder = $"pets/{pet.PetId}/";
                            string fileName = $"{pet.Name.Replace(" ", "-")}-{Guid.NewGuid()}{Path.GetExtension(photoFile.FileName)}";
                            string photoUrl = await _fileUploadService.UploadImageAsync(photoFile.OpenReadStream(), fileName, folder);
                            
                            // Prepend new photo to the beginning of the list
                            pet.PhotoUrls = pet.PhotoUrls.ToList();
                            pet.PhotoUrls.Insert(0, photoUrl);
                            
                            _logger.LogInformation($"New photo uploaded for pet {pet.PetId}: {photoUrl}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to upload photo for pet {pet.PetId}");
                        }
                    }
                }

                pet.UpdatedDate = DateTime.UtcNow;

                PetEntity updatedPet = await _petRepository.UpdateAsync(pet);
                
                // Map Entity to Response DTO
                PetResponse petResponse = _mapper.Map<PetResponse>(updatedPet);
                
                Logger.LogInformation($"Pet {id} updated with PATCH");
                return Ok(petResponse);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error updating Pet {id} with PATCH");
                return StatusCode(500, new { message = "Error updating Pet", error = ex.Message });
            }
        }

        // PUT: api/pets/{id}
        // Update an existing pet
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PetResponse>> Update(string id, [FromBody] UpdatePetRequest request)
        {
            _logger.LogInformation($"Endpoint: Update, Method: PUT, PetId: {id}");
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Pet id is required." });

            if (request == null)
                return BadRequest(new { message = "Pet data is required." });

            try
            {
                PetEntity? existingPet = await _petRepository.GetByIdAsync(id);
                if (existingPet == null)
                    return NotFound(new { message = "Pet not found" });

                // Map DTO to Entity, preserving PetId and timestamps
                PetEntity pet = _mapper.Map<PetEntity>(request);
                pet.PetId = id;
                pet.IntakeDate = existingPet.IntakeDate;
                pet.CreatedDate = existingPet.CreatedDate;
                pet.UpdatedDate = DateTime.UtcNow;
                
                // Preserve existing PhotoUrls if not provided in the update request
                if (pet.PhotoUrls == null || pet.PhotoUrls.Count == 0)
                {
                    pet.PhotoUrls = existingPet.PhotoUrls ?? new List<string>();
                }

                PetEntity updatedPet = await _petRepository.UpdateAsync(pet);
                
                // Map Entity to Response DTO
                PetResponse petResponse = _mapper.Map<PetResponse>(updatedPet);
                
                Logger.LogInformation($"Pet {id} updated");
                return Ok(petResponse);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error updating Pet {id}");
                return StatusCode(500, new { message = "Error updating Pet", error = ex.Message });
            }
        }

        // DELETE: api/pets/{id}
        // Delete a pet
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"Endpoint: Delete, Method: DELETE, PetId: {id}");
            return await DeleteAsync<PetEntity, string>(
                id,
                _petRepository.GetByIdAsync,
                _petRepository.DeleteAsync,
                "Pet");
        }
    }
}
