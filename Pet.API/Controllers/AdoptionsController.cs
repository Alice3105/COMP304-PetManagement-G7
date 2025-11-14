using Microsoft.AspNetCore.Mvc;
using Pet.API.Models.Entities;
using Pet.API.Repositories.Interfaces;

namespace Pet.API.Controllers
{
    [Route("api/adoptions")]
    [ApiController]
    public class AdoptionsController : ControllerBase
    {
        private readonly IAdoptionRepository _adoptionRepository;
        private readonly IPetRepository _petRepository;
        private readonly ILogger<AdoptionsController> _logger;

        public AdoptionsController(
            IAdoptionRepository adoptionRepository,
            IPetRepository petRepository,
            ILogger<AdoptionsController> logger)
        {
            _adoptionRepository = adoptionRepository;
            _petRepository = petRepository;
            _logger = logger;
        }

        // GET: api/adoptions
        [HttpGet]
        public async Task<IActionResult> GetAllAdoptions()
        {
            try
            {
                var adoptions = await _adoptionRepository.GetAllAsync();
                return Ok(adoptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all adoptions");
                return StatusCode(500, new { message = "Error fetching adoptions", error = ex.Message });
            }
        }

        // GET: api/adoptions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdoptionById(string id)
        {
            try
            {
                var adoption = await _adoptionRepository.GetByIdAsync(id);

                if (adoption == null)
                    return NotFound(new { message = "Adoption not found" });

                return Ok(adoption);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching adoption {id}");
                return StatusCode(500, new { message = "Error fetching adoption", error = ex.Message });
            }
        }

        // GET: api/adoptions/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAdoptionsByUserId(string userId)
        {
            try
            {
                var adoptions = await _adoptionRepository.GetByUserIdAsync(userId);
                return Ok(adoptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching adoptions for user {userId}");
                return StatusCode(500, new { message = "Error fetching adoptions", error = ex.Message });
            }
        }

        // GET: api/adoptions/pet/{petId}
        [HttpGet("pet/{petId}")]
        public async Task<IActionResult> GetAdoptionsByPetId(string petId)
        {
            try
            {
                var adoptions = await _adoptionRepository.GetByPetIdAsync(petId);
                return Ok(adoptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching adoptions for pet {petId}");
                return StatusCode(500, new { message = "Error fetching adoptions", error = ex.Message });
            }
        }

        // POST: api/adoptions
        [HttpPost]
        public async Task<IActionResult> CreateAdoption([FromBody] Adoption adoption)
        {
            try
            {
                // Verify pet exists
                var pet = await _petRepository.GetByIdAsync(adoption.PetId);
                if (pet == null)
                {
                    return BadRequest(new { message = "Pet not found" });
                }

                adoption.PetName = pet.Name;
                var createdAdoption = await _adoptionRepository.CreateAsync(adoption);

                _logger.LogInformation($"Adoption application created: {createdAdoption.AdoptionId} for pet {pet.Name}");

                return CreatedAtAction(nameof(GetAdoptionById), new { id = createdAdoption.AdoptionId }, createdAdoption);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating adoption");
                return StatusCode(500, new { message = "Error creating adoption", error = ex.Message });
            }
        }

        // PUT: api/adoptions/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAdoptionStatus(string id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var updatedAdoption = await _adoptionRepository.UpdateStatusAsync(
                    id,
                    request.Status,
                    request.ReviewedBy,
                    request.ReviewNotes
                );

                // If approved, update pet status to "Adopted"
                if (request.Status == "Approved")
                {
                    var pet = await _petRepository.GetByIdAsync(updatedAdoption.PetId);
                    if (pet != null)
                    {
                        pet.Status = "Adopted";
                        await _petRepository.UpdateAsync(pet);
                    }
                }
                // If rejected, set pet back to "Available"
                else if (request.Status == "Rejected")
                {
                    var pet = await _petRepository.GetByIdAsync(updatedAdoption.PetId);
                    if (pet != null && pet.Status == "Pending")
                    {
                        pet.Status = "Available";
                        await _petRepository.UpdateAsync(pet);
                    }
                }

                _logger.LogInformation($"Adoption {id} status updated to {request.Status} by {request.ReviewedBy}");

                return Ok(updatedAdoption);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating adoption {id} status");
                return StatusCode(500, new { message = "Error updating adoption status", error = ex.Message });
            }
        }
    }

    // DTO for updating adoption status
    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty; // Approved, Rejected
        public string ReviewedBy { get; set; } = string.Empty;
        public string? ReviewNotes { get; set; }
    }
}
