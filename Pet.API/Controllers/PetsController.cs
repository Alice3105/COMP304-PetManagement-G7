using Microsoft.AspNetCore.Mvc;
using PetEntity = Pet.API.Models.Entities.Pet;
using Pet.API.Repositories.Interfaces;

namespace Pet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController : ControllerBase
    {
        private readonly IPetRepository _petRepository;

        public PetsController(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        // GET: api/pets
        // Returns all pets
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PetEntity>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PetEntity>>> GetAll()
        {
            var pets = await _petRepository.GetAllAsync();
            return Ok(pets);
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
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Pet id is required.");

            var pet = await _petRepository.GetByIdAsync(id);

            if (pet is null)
                return NotFound();

            return Ok(pet);
        }

        // POST: api/pets
        // Create a new pet
        [HttpPost]
        [ProducesResponseType(typeof(PetEntity), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PetEntity>> Create([FromBody] PetEntity pet)
        {
            if (pet == null)
                return BadRequest("Pet data is required.");

            // Ensure PetId exists (if DynamoDB key is PetId)
            if (string.IsNullOrWhiteSpace(pet.PetId))
            {
                pet.PetId = Guid.NewGuid().ToString();
            }

            var created = await _petRepository.CreateAsync(pet);

            // Returns 201 with Location header: api/pets/{id}
            return CreatedAtAction(nameof(GetById), new { id = created.PetId }, created);
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
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Pet id is required.");

            if (pet == null)
                return BadRequest("Pet data is required.");

            var existing = await _petRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            // Make sure the ids line up
            pet.PetId = id;

            var updated = await _petRepository.UpdateAsync(pet);
            return Ok(updated);
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
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Pet id is required.");

            var existing = await _petRepository.GetByIdAsync(id);
            if (existing is null)
                return NotFound();

            await _petRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
