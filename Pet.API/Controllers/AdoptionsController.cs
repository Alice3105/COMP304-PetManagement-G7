using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Pet.API.Models.Entities;
using Pet.API.Models.DTOs;
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
        private readonly IMapper _mapper;
        private readonly ILogger<AdoptionsController> _logger;

        public AdoptionsController(
            IAdoptionRepository adoptionRepository,
            IPetRepository petRepository,
            IMapper mapper,
            ILogger<AdoptionsController> logger) : base(logger)
        {
            _adoptionRepository = adoptionRepository;
            _petRepository = petRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/adoptions
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AdoptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AdoptionResponse>>> GetAllAdoptions()
        {
            try
            {
                var adoptions = await _adoptionRepository.GetAllAsync();
                var adoptionResponses = _mapper.Map<IEnumerable<AdoptionResponse>>(adoptions);
                return Ok(adoptionResponses);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching all Adoptions");
                return StatusCode(500, new { message = "Error fetching Adoptions", error = ex.Message });
            }
        }

        // GET: api/adoptions/{id}
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(AdoptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdoptionResponse>> GetAdoptionById([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Adoption id is required." });

            try
            {
                var adoption = await _adoptionRepository.GetByIdAsync(id);
                if (adoption == null)
                    return NotFound(new { message = "Adoption not found" });

                var adoptionResponse = _mapper.Map<AdoptionResponse>(adoption);
                return Ok(adoptionResponse);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error fetching Adoption {id}");
                return StatusCode(500, new { message = "Error fetching Adoption", error = ex.Message });
            }
        }

        // GET: api/adoptions/user/{userId}
        [HttpGet]
        [Route("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<AdoptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAdoptionsByUserId([FromRoute] string userId)
        {
            try
            {
                var adoptions = await _adoptionRepository.GetByUserIdAsync(userId);
                var adoptionResponses = _mapper.Map<IEnumerable<AdoptionResponse>>(adoptions);
                return Ok(adoptionResponses);
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
        [ProducesResponseType(typeof(IEnumerable<AdoptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAdoptionsByPetId([FromRoute] string petId)
        {
            try
            {
                var adoptions = await _adoptionRepository.GetByPetIdAsync(petId);
                var adoptionResponses = _mapper.Map<IEnumerable<AdoptionResponse>>(adoptions);
                return Ok(adoptionResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching adoptions for pet {petId}");
                return StatusCode(500, new { message = "Error fetching adoptions", error = ex.Message });
            }
        }

        // POST: api/adoptions
        [HttpPost]
        [ProducesResponseType(typeof(AdoptionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAdoption([FromBody] CreateAdoptionRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Adoption data is required." });

            try
            {
                // Verify pet exists
                PetEntity? pet = await _petRepository.GetByIdAsync(request.PetId);
                if (pet == null)
                {
                    return BadRequest(new { message = "Pet not found" });
                }

                // Map DTO to Entity
                var adoption = _mapper.Map<Adoption>(request);
                adoption.PetName = pet.Name;

                var createdAdoption = await _adoptionRepository.CreateAsync(adoption);

                // Map Entity to Response DTO
                var adoptionResponse = _mapper.Map<AdoptionResponse>(createdAdoption);

                _logger.LogInformation($"Adoption application created: {createdAdoption.AdoptionId} for pet {pet.Name}");

                return CreatedAtAction(nameof(GetAdoptionById), new { id = createdAdoption.AdoptionId }, adoptionResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating adoption");
                return StatusCode(500, new { message = "Error creating adoption", error = ex.Message });
            }
        }

        // PATCH: api/adoptions/{id}/status
        // Partial update for adoption status (used when staff/admin change adoption status)
        [HttpPatch]
        [Route("{id}/status")]
        [ProducesResponseType(typeof(AdoptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAdoptionStatus([FromRoute] string id, [FromBody] UpdateAdoptionStatusRequest request)
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

                // Map Entity to Response DTO
                var adoptionResponse = _mapper.Map<AdoptionResponse>(updatedAdoption);
                return Ok(adoptionResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating adoption {id} status");
                return StatusCode(500, new { message = "Error updating adoption status", error = ex.Message });
            }
        }

        // PUT: api/adoptions/{id}
        // Updates an adoption application (only editable fields, not status)
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AdoptionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdoptionResponse>> UpdateAdoption(string id, [FromBody] UpdateAdoptionRequest request)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Adoption id is required." });

            if (request == null)
                return BadRequest(new { message = "Adoption data is required." });

            try
            {
                var existingAdoption = await _adoptionRepository.GetByIdAsync(id);
                if (existingAdoption == null)
                    return NotFound(new { message = "Adoption not found" });

                // Map DTO to Entity, preserving non-editable fields
                var adoption = _mapper.Map<Adoption>(request);
                adoption.AdoptionId = id;
                adoption.PetId = existingAdoption.PetId;
                adoption.PetName = existingAdoption.PetName;
                adoption.UserId = existingAdoption.UserId;
                adoption.UserEmail = existingAdoption.UserEmail;
                adoption.UserFirstName = existingAdoption.UserFirstName;
                adoption.UserLastName = existingAdoption.UserLastName;
                adoption.Status = existingAdoption.Status;
                adoption.ApplicationDate = existingAdoption.ApplicationDate;
                adoption.ReviewedDate = existingAdoption.ReviewedDate;
                adoption.ReviewedBy = existingAdoption.ReviewedBy;
                adoption.ReviewNotes = existingAdoption.ReviewNotes;

                var updatedAdoption = await _adoptionRepository.UpdateAsync(adoption);

                // Map Entity to Response DTO
                var adoptionResponse = _mapper.Map<AdoptionResponse>(updatedAdoption);

                Logger.LogInformation($"Adoption {id} updated");
                return Ok(adoptionResponse);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error updating Adoption {id}");
                return StatusCode(500, new { message = "Error updating Adoption", error = ex.Message });
            }
        }

        // DELETE: api/adoptions/{id}
        // Deletes an adoption application
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAdoption(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Adoption id is required." });

            try
            {
                var existingAdoption = await _adoptionRepository.GetByIdAsync(id);
                if (existingAdoption == null)
                    return NotFound(new { message = "Adoption not found" });

                await _adoptionRepository.DeleteAsync(id);

                Logger.LogInformation($"Adoption {id} deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error deleting Adoption {id}");
                return StatusCode(500, new { message = "Error deleting Adoption", error = ex.Message });
            }
        }
    }

}
