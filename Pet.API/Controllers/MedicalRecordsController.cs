using Microsoft.AspNetCore.Mvc;
using Pet.API.Models.Entities;
using Pet.API.Repositories.Interfaces;

namespace Pet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordsController : BaseController
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly ILogger<MedicalRecordsController> _logger;

        public MedicalRecordsController(IMedicalRecordRepository medicalRecordRepository, ILogger<MedicalRecordsController> logger)
        {
            _medicalRecordRepository = medicalRecordRepository;
            _logger = logger;
        }

        // GET: api/medicalrecords/pet/{petId}
        // Returns all medical records for a specific pet
        [HttpGet("pet/{petId}")]
        [ProducesResponseType(typeof(IEnumerable<MedicalRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MedicalRecord>>> GetByPetId(string petId)
        {
            if (string.IsNullOrWhiteSpace(petId))
                return BadRequest(new { message = "Pet id is required." });

            try
            {
                List<MedicalRecord> records = await _medicalRecordRepository.GetByPetIdAsync(petId);
                return Ok(records);
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
        [ProducesResponseType(typeof(MedicalRecord), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalRecord>> GetById(string id)
        {
            return await GetByIdAsync(
                id,
                _medicalRecordRepository.GetByIdAsync,
                "Medical Record",
                _logger);
        }

        // GET: api/medicalrecords
        // Returns all medical records
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MedicalRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MedicalRecord>>> GetAll()
        {
            return await GetAllAsync<MedicalRecord>(
                async () => await _medicalRecordRepository.GetAllAsync(),
                "Medical Record",
                _logger);
        }

        // POST: api/medicalrecords
        // Creates a new medical record
        [HttpPost]
        [ProducesResponseType(typeof(MedicalRecord), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalRecord>> Create([FromBody] MedicalRecord record)
        {
            return await CreateAsync(
                record,
                _medicalRecordRepository.CreateAsync,
                r => r.RecordId,
                "Medical Record",
                nameof(GetById),
                _logger,
                r => !string.IsNullOrWhiteSpace(r.PetId) && !string.IsNullOrWhiteSpace(r.RecordType));
        }
    }
}

