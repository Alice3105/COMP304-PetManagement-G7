using Microsoft.AspNetCore.Mvc;
using PetEntity = Pet.API.Models.Entities.Pet;
using Pet.API.Repositories.Interfaces;

namespace Pet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController : BaseController
    {
        private readonly IPetRepository _petRepository;
        private readonly ILogger<PetsController> _logger;

        public PetsController(IPetRepository petRepository, ILogger<PetsController> logger)
        {
            _petRepository = petRepository;
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
                "Pets",
                _logger);
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
                "Pet",
                _logger);
        }

        // POST: api/pets
        // Create a new pet
        [HttpPost]
        [ProducesResponseType(typeof(PetEntity), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PetEntity>> Create([FromBody] PetEntity pet)
        {
            // Ensure PetId exists (if DynamoDB key is PetId)
            if (pet != null && string.IsNullOrWhiteSpace(pet.PetId))
            {
                pet.PetId = Guid.NewGuid().ToString();
            }

            return await CreateAsync(
                pet,
                _petRepository.CreateAsync,
                p => p.PetId,
                "Pet",
                nameof(GetById),
                _logger);
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
                "Pet",
                _logger);
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
                "Pet",
                _logger);
        }
    }
}
