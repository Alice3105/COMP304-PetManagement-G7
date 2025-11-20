using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Pet.API.Models.Entities;
using Pet.API.Models.DTOs;
using Pet.API.Repositories.Interfaces;
using PetEntity = Pet.API.Models.Entities.Pet;

namespace Pet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordsController : BaseController
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly IPetRepository _petRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<MedicalRecordsController> _logger;

        public MedicalRecordsController(
            IMedicalRecordRepository medicalRecordRepository,
            IPetRepository petRepository,
            IMapper mapper,
            ILogger<MedicalRecordsController> logger) : base(logger)
        {
            _medicalRecordRepository = medicalRecordRepository;
            _petRepository = petRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/medicalrecords/pet/{petId}
        // Returns all medical records for a specific pet
        [HttpGet("pet/{petId}")]
        [ProducesResponseType(typeof(IEnumerable<MedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MedicalRecordResponse>>> GetByPetId(string petId)
        {
            _logger.LogInformation($"Endpoint: GetByPetId, Method: GET, PetId: {petId}");
            if (string.IsNullOrWhiteSpace(petId))
                return BadRequest(new { message = "Pet id is required." });

            try
            {
                IEnumerable<MedicalRecord> records = await _medicalRecordRepository.GetByPetIdAsync(petId);
                IEnumerable<MedicalRecordResponse> recordResponses = _mapper.Map<IEnumerable<MedicalRecordResponse>>(records);
                return Ok(recordResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching medical records for pet {petId}");
                return StatusCode(500, new { message = "Error fetching medical records", error = ex.Message });
            }
        }

        // GET: api/medicalrecords/{id}
        // Returns a single medical record by id
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MedicalRecordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalRecordResponse>> GetById(string id)
        {
            _logger.LogInformation($"Endpoint: GetById, Method: GET, RecordId: {id}");
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Medical Record id is required." });

            try
            {
                MedicalRecord? record = await _medicalRecordRepository.GetByIdAsync(id);
                if (record == null)
                    return NotFound(new { message = "Medical Record not found" });

                MedicalRecordResponse recordResponse = _mapper.Map<MedicalRecordResponse>(record);
                return Ok(recordResponse);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error fetching Medical Record {id}");
                return StatusCode(500, new { message = "Error fetching Medical Record", error = ex.Message });
            }
        }

        // GET: api/medicalrecords
        // Returns all medical records
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MedicalRecordResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MedicalRecordResponse>>> GetAll()
        {
            _logger.LogInformation($"Endpoint: GetAll, Method: GET");
            try
            {
                IEnumerable<MedicalRecord> records = await _medicalRecordRepository.GetAllAsync();
                IEnumerable<MedicalRecordResponse> recordResponses = _mapper.Map<IEnumerable<MedicalRecordResponse>>(records);
                return Ok(recordResponses);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching all Medical Records");
                return StatusCode(500, new { message = "Error fetching Medical Records", error = ex.Message });
            }
        }

        // POST: api/medicalrecords
        // Creates a new medical record
        [HttpPost]
        [ProducesResponseType(typeof(MedicalRecordResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalRecordResponse>> Create([FromBody] CreateMedicalRecordRequest request)
        {
            _logger.LogInformation($"Endpoint: Create, Method: POST");
            if (request == null)
                return BadRequest(new { message = "Medical Record data is required." });

            if (string.IsNullOrWhiteSpace(request.PetId) || string.IsNullOrWhiteSpace(request.RecordType))
                return BadRequest(new { message = "PetId and RecordType are required." });

            try
            {
                // Verify pet exists and get pet name
                PetEntity? pet = await _petRepository.GetByIdAsync(request.PetId);
                if (pet == null)
                {
                    return BadRequest(new { message = "Pet not found" });
                }

                // Map DTO to Entity
                MedicalRecord record = _mapper.Map<MedicalRecord>(request);
                record.PetName = pet.Name;
                
                MedicalRecord createdRecord = await _medicalRecordRepository.CreateAsync(record);
                
                // Map Entity to Response DTO
                MedicalRecordResponse recordResponse = _mapper.Map<MedicalRecordResponse>(createdRecord);
                
                Logger.LogInformation($"Medical Record created: {createdRecord.RecordId}");
                return CreatedAtAction(nameof(GetById), new { id = createdRecord.RecordId }, recordResponse);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating Medical Record");
                return StatusCode(500, new { message = "Error creating Medical Record", error = ex.Message });
            }
        }

        // PUT: api/medicalrecords/{id}
        // Updates all fields of an existing medical record
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MedicalRecordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalRecordResponse>> Update(string id, [FromBody] UpdateMedicalRecordRequest request)
        {
            _logger.LogInformation($"Endpoint: Update, Method: PUT, RecordId: {id}");
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Medical Record id is required." });

            if (request == null)
                return BadRequest(new { message = "Medical Record data is required." });

            if (string.IsNullOrWhiteSpace(request.PetId) || string.IsNullOrWhiteSpace(request.RecordType))
                return BadRequest(new { message = "PetId and RecordType are required." });

            try
            {
                MedicalRecord? existingRecord = await _medicalRecordRepository.GetByIdAsync(id);
                if (existingRecord == null)
                    return NotFound(new { message = "Medical Record not found" });

                PetEntity? pet = await _petRepository.GetByIdAsync(request.PetId);
                if (pet == null)
                {
                    return BadRequest(new { message = "Pet not found" });
                }

                // Map DTO to Entity, preserving RecordId and CreatedDate
                MedicalRecord record = _mapper.Map<MedicalRecord>(request);
                record.RecordId = id;
                record.PetName = pet.Name;
                record.CreatedDate = existingRecord.CreatedDate;
                record.UpdatedDate = DateTime.UtcNow;

                MedicalRecord updatedRecord = await _medicalRecordRepository.UpdateAsync(record);

                // Map Entity to Response DTO
                MedicalRecordResponse recordResponse = _mapper.Map<MedicalRecordResponse>(updatedRecord);

                Logger.LogInformation($"Medical Record {id} updated");
                return Ok(recordResponse);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error updating Medical Record {id}");
                return StatusCode(500, new { message = "Error updating Medical Record", error = ex.Message });
            }
        }

        // DELETE: api/medicalrecords/{id}
        // Deletes a medical record
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"Endpoint: Delete, Method: DELETE, RecordId: {id}");
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Medical Record id is required." });

            try
            {
                MedicalRecord? existingRecord = await _medicalRecordRepository.GetByIdAsync(id);
                if (existingRecord == null)
                    return NotFound(new { message = "Medical Record not found" });

                await _medicalRecordRepository.DeleteAsync(id);

                Logger.LogInformation($"Medical Record {id} deleted");
                return NoContent();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error deleting Medical Record {id}");
                return StatusCode(500, new { message = "Error deleting Medical Record", error = ex.Message });
            }
        }
    }
}

