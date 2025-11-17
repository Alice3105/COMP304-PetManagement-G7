using Microsoft.AspNetCore.Mvc;
using Pet.API.Models.Entities;
using Pet.API.Repositories.Interfaces;
using Pet.API.Services.Interfaces;

namespace Pet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController : ControllerBase
    {
        private readonly IPetRepository _petRepository;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<PetsController> _logger;

        // Constructor supporting both simple and enhanced usage
        public PetsController(
            IPetRepository petRepository,
            IFileUploadService fileUploadService,
            ILogger<PetsController> logger)
        {
            _petRepository = petRepository;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        // GET: api/pets
        // Returns all pets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pet>>> GetAll()
        {
            try
            {
                var pets = await _petRepository.GetAllAsync();
                return Ok(pets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all pets");
                return StatusCode(500, new { message = "Error fetching pets", error = ex.Message });
            }
        }

        // GET: api/pets/{id}
        // Returns a single pet by id
        [HttpGet("{id}")]
        public async Task<ActionResult<Pet>> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Pet id is required.");

            try
            {
                var pet = await _petRepository.GetByIdAsync(id);

                if (pet is null)
                    return NotFound();

                return Ok(pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching pet {id}");
                return StatusCode(500, new { message = "Error fetching pet", error = ex.Message });
            }
        }

        // POST: api/pets
        // Create a new pet - supports both JSON and form data with file upload
        [HttpPost]
        public async Task<ActionResult<Pet>> Create([FromBody] Pet pet)
        {
            if (pet == null)
                return BadRequest("Pet data is required.");

            try
            {
                // Ensure PetId exists (if DynamoDB key is PetId)
                if (string.IsNullOrWhiteSpace(pet.PetId))
                {
                    pet.PetId = Guid.NewGuid().ToString();
                }

                var created = await _petRepository.CreateAsync(pet);
                _logger.LogInformation($"Pet created: {created.PetId} - {created.Name}");

                // Returns 201 with Location header: api/pets/{id}
                return CreatedAtAction(nameof(GetById), new { id = created.PetId }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pet");
                return StatusCode(500, new { message = "Error creating pet", error = ex.Message });
            }
        }

        // POST: api/pets/with-photo
        // Create a new pet with photo upload
        [HttpPost("with-photo")]
        public async Task<ActionResult<Pet>> CreateWithPhoto([FromForm] CreatePetRequest request)
        {
            if (request == null)
                return BadRequest("Pet data is required.");

            try
            {
                var pet = new Models.Entities.Pet
                {
                    Name = request.Name,
                    Species = request.Species,
                    Breed = request.Breed,
                    Age = request.Age,
                    Gender = request.Gender,
                    Size = request.Size,
                    Color = request.Color,
                    Description = request.Description,
                    Vaccinated = request.Vaccinated,
                    Neutered = request.Neutered,
                    GoodWithKids = request.GoodWithKids,
                    GoodWithPets = request.GoodWithPets,
                    Status = "Available",
                    IntakeDate = DateTime.UtcNow,
                    PhotoUrls = new List<string>()
                };

                // Ensure PetId exists
                if (string.IsNullOrWhiteSpace(pet.PetId))
                {
                    pet.PetId = Guid.NewGuid().ToString();
                }

                // Upload photo if provided
                if (request.Photo != null && request.Photo.Length > 0)
                {
                    using (var stream = request.Photo.OpenReadStream())
                    {
                        var photoUrl = await _fileUploadService.UploadImageAsync(stream, request.Photo.FileName);
                        pet.PhotoUrls.Add(photoUrl);
                    }
                }

                var created = await _petRepository.CreateAsync(pet);
                _logger.LogInformation($"Pet created with photo: {created.PetId} - {created.Name}");

                return CreatedAtAction(nameof(GetById), new { id = created.PetId }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pet with photo");
                return StatusCode(500, new { message = "Error creating pet", error = ex.Message });
            }
        }

        // PUT: api/pets/{id}
        // Update an existing pet
        [HttpPut("{id}")]
        public async Task<ActionResult<Pet>> Update(string id, [FromBody] Pet pet)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Pet id is required.");

            if (pet == null)
                return BadRequest("Pet data is required.");

            try
            {
                var existing = await _petRepository.GetByIdAsync(id);
                if (existing is null)
                    return NotFound();

                // Make sure the ids line up
                pet.PetId = id;

                var updated = await _petRepository.UpdateAsync(pet);
                _logger.LogInformation($"Pet updated: {updated.PetId} - {updated.Name}");

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating pet {id}");
                return StatusCode(500, new { message = "Error updating pet", error = ex.Message });
            }
        }

        // DELETE: api/pets/{id}
        // Delete a pet
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Pet id is required.");

            try
            {
                var existing = await _petRepository.GetByIdAsync(id);
                if (existing is null)
                    return NotFound();

                await _petRepository.DeleteAsync(id);
                _logger.LogInformation($"Pet deleted: {id}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting pet {id}");
                return StatusCode(500, new { message = "Error deleting pet", error = ex.Message });
            }
        }
    }

    // DTO for creating a pet with file upload
    public class CreatePetRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Vaccinated { get; set; }
        public bool Neutered { get; set; }
        public bool GoodWithKids { get; set; }
        public bool GoodWithPets { get; set; }
        public IFormFile? Photo { get; set; }
    }
}
