using Microsoft.AspNetCore.Mvc;
using Pet.API.Models.Entities;
using Pet.API.Models.Enums;
using Pet.API.Repositories.Interfaces;
using PetEntity = Pet.API.Models.Entities.Pet;

namespace Pet.API.Controllers
{
    [Route("api/adoptions")]
    [ApiController]
    public class AdoptionsController : BaseController
    {
        private readonly IAdoptionRepository _adoptionRepository;
        private readonly IPetRepository _petRepository;
        private readonly ILogger<AdoptionsController> _logger;

        public AdoptionsController(
            IAdoptionRepository adoptionRepository,
            IPetRepository petRepository,
            ILogger<AdoptionsController> logger) : base(logger)
        {
            _adoptionRepository = adoptionRepository;
            _petRepository = petRepository;
            _logger = logger;
        }

        // GET: api/adoptions
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Adoption>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Adoption>>> GetAllAdoptions()
        {
            return await GetAllAsync(
                _adoptionRepository.GetAllAsync,
                "Adoptions");
        }

        // GET: api/adoptions/{id}
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(Adoption), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Adoption>> GetAdoptionById([FromRoute] string id)
        {
            return await GetByIdAsync<Adoption>(
                id,
                _adoptionRepository.GetByIdAsync,
                "Adoption");
        }

        // GET: api/adoptions/user/{userId}
        [HttpGet]
        [Route("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<Adoption>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAdoptionsByUserId([FromRoute] string userId)
        {
            try
            {
                IEnumerable<Adoption> adoptions = await _adoptionRepository.GetByUserIdAsync(userId);
                return Ok(adoptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching adoptions for user {userId}");
                return StatusCode(500, new { message = "Error fetching adoptions", error = ex.Message });
            }
        }

        // GET: api/adoptions/pet/{petId}
        [HttpGet]
        [Route("pet/{petId}")]
        [ProducesResponseType(typeof(IEnumerable<Adoption>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAdoptionsByPetId([FromRoute] string petId)
        {
            try
            {
                IEnumerable<Adoption> adoptions = await _adoptionRepository.GetByPetIdAsync(petId);
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
        [ProducesResponseType(typeof(Adoption), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAdoption([FromBody] Adoption adoption)
        {
            try
            {
                // Verify pet exists
                PetEntity? pet = await _petRepository.GetByIdAsync(adoption.PetId);
                if (pet == null)
                {
                    return BadRequest(new { message = "Pet not found" });
                }

                adoption.PetName = pet.Name;
                Adoption createdAdoption = await _adoptionRepository.CreateAsync(adoption);

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
        [HttpPut]
        [Route("{id}/status")]
        [ProducesResponseType(typeof(Adoption), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAdoptionStatus([FromRoute] string id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                Adoption updatedAdoption = await _adoptionRepository.UpdateStatusAsync(
                    id,
                    request.Status,
                    request.ReviewedBy,
                    request.ReviewNotes
                );

                // Update pet status based on adoption status
                PetEntity? pet = await _petRepository.GetByIdAsync(updatedAdoption.PetId);
                if (pet != null)
                {
                    switch (request.Status)
                    {
                        case "Approved":
                            pet.Status = PetStatus.Adopted.ToStringValue();
                            await _petRepository.UpdateAsync(pet);
                            break;

                        case "Pending":
                            IEnumerable<Adoption> allAdoptionsPending = await _adoptionRepository.GetByPetIdAsync(pet.PetId);
                            bool hasApprovedAdoptionPending = allAdoptionsPending.Any(a => a.AdoptionId != updatedAdoption.AdoptionId && a.Status == "Approved");
                            
                            if (!hasApprovedAdoptionPending)
                            {
                                pet.Status = PetStatus.Pending.ToStringValue();
                                await _petRepository.UpdateAsync(pet);
                            }
                            break;

                        case "Rejected":
                            IEnumerable<Adoption> allAdoptionsRejected = await _adoptionRepository.GetByPetIdAsync(pet.PetId);
                            bool hasApprovedAdoptionRejected = allAdoptionsRejected.Any(a => a.AdoptionId != updatedAdoption.AdoptionId && a.Status == "Approved");
                            bool hasPendingAdoptionRejected = allAdoptionsRejected.Any(a => a.AdoptionId != updatedAdoption.AdoptionId && a.Status == "Pending");
                            
                            PetStatus petStatusRejected = (hasApprovedAdoptionRejected, hasPendingAdoptionRejected) switch
                            {
                                (true, _) => PetStatus.Adopted,      // Another adoption is approved, keep pet as Adopted
                                (false, true) => PetStatus.Pending,  // There are pending adoptions, set pet to Pending
                                _ => PetStatus.Available              // No other adoptions, make pet Available
                            };
                            
                            pet.Status = petStatusRejected.ToStringValue();
                            await _petRepository.UpdateAsync(pet);
                            break;
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
        public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected
        public string ReviewedBy { get; set; } = string.Empty;
        public string? ReviewNotes { get; set; }
    }
}
