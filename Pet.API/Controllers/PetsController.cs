using Microsoft.AspNetCore.Mvc;
using PetEntity = Pet.API.Models.Entities.Pet;
using Pet.API.Repositories.Interfaces;
using Pet.API.Services.Interfaces;

namespace Pet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController : BaseController
    {
        private readonly IPetRepository _petRepository;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<PetsController> _logger;

        public PetsController(IPetRepository petRepository, IFileUploadService fileUploadService, ILogger<PetsController> logger) : base(logger)
        {
            _petRepository = petRepository;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        // GET: api/pets
        // Returns all pets
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PetEntity>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PetEntity>>> GetAll()
        {
            return await GetAllAsync(
                _petRepository.GetAllAsync,
                "Pets");
        }

        // GET: api/pets/{id}
        // Returns a single pet by id
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PetEntity), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PetEntity>> GetById(string id)
        {
            return await GetByIdAsync(
                id,
                _petRepository.GetByIdAsync,
                "Pet");
        }

        // POST: api/pets
        // Create a new pet with file upload (multipart form data only)
        [HttpPost]
        [ProducesResponseType(typeof(PetEntity), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PetEntity>> Create()
        {
            try
            {
                if (!Request.HasFormContentType)
                {
                    return BadRequest(new { message = "Request must be multipart/form-data." });
                }

                var form = await Request.ReadFormAsync();
                
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

                var pet = new PetEntity
                {
                    PetId = Guid.NewGuid().ToString(),
                    Name = name,
                    Species = species,
                    Breed = breed,
                    Age = age,
                    Gender = gender,
                    Size = size,
                    Color = color,
                    Description = description,
                    Status = "Available",
                    IntakeDate = DateTime.UtcNow,
                    Vaccinated = vaccinated,
                    Neutered = neutered,
                    GoodWithKids = goodWithKids,
                    GoodWithPets = goodWithPets,
                    CreatedDate = DateTime.UtcNow,
                    PhotoUrls = new List<string>()
                };

                if (form.Files != null && form.Files.Count > 0)
                {
                    var photoFile = form.Files["Photo"];
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

                return await CreateAsync(
                    pet,
                    _petRepository.CreateAsync,
                    p => p.PetId,
                    "Pet",
                    nameof(GetById));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pet with file upload");
                return StatusCode(500, new { message = "Error creating pet", error = ex.Message });
            }
        }

        // PUT: api/pets/{id}
        // Update an existing pet
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PetEntity), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PetEntity>> Update(string id, [FromBody] PetEntity pet)
        {
            return await UpdateAsync(
                id,
                pet,
                _petRepository.GetByIdAsync,
                _petRepository.UpdateAsync,
                (p, petId) => p.PetId = petId,
                "Pet");
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
            return await DeleteAsync<PetEntity, string>(
                id,
                _petRepository.GetByIdAsync,
                _petRepository.DeleteAsync,
                "Pet");
        }
    }
}
