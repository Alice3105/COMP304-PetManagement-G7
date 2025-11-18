using Microsoft.AspNetCore.Mvc;
using Pet.API.Models.Entities;
using Pet.API.Repositories.Interfaces;

namespace Pet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public MedicalRecordsController(IMedicalRecordRepository medicalRecordRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
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
                return BadRequest("Pet id is required.");

            var records = await _medicalRecordRepository.GetByPetIdAsync(petId);
            return Ok(records);
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
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Record id is required.");

            var record = await _medicalRecordRepository.GetByIdAsync(id);

            if (record is null)
                return NotFound();

            return Ok(record);
        }

        // GET: api/medicalrecords
        // Returns all medical records
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MedicalRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MedicalRecord>>> GetAll()
        {
            var records = await _medicalRecordRepository.GetAllAsync();
            return Ok(records);
        }

        // POST: api/medicalrecords
        // Creates a new medical record
        [HttpPost]
        [ProducesResponseType(typeof(MedicalRecord), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalRecord>> Create([FromBody] MedicalRecord record)
        {
            if (string.IsNullOrWhiteSpace(record.PetId))
                return BadRequest("Pet ID is required.");

            if (string.IsNullOrWhiteSpace(record.RecordType))
                return BadRequest("Record type is required.");

            var createdRecord = await _medicalRecordRepository.CreateAsync(record);
            return CreatedAtAction(nameof(GetById), new { id = createdRecord.RecordId }, createdRecord);
        }
    }
}

