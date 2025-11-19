namespace Pet.API.Models.DTOs
{
    // Request DTOs
    public class CreateMedicalRecordRequest
    {
        public string PetId { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;
        public string VeterinarianId { get; set; } = string.Empty;
        public string VeterinarianName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VaccineName { get; set; } = string.Empty;
        public DateTime? NextDueDate { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateMedicalRecordRequest
    {
        public string PetId { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public string VeterinarianId { get; set; } = string.Empty;
        public string VeterinarianName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VaccineName { get; set; } = string.Empty;
        public DateTime? NextDueDate { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateMedicalRecordPatchRequest
    {
        public string? PetId { get; set; }
        public string? RecordType { get; set; }
        public DateTime? RecordDate { get; set; }
        public string? VeterinarianId { get; set; }
        public string? VeterinarianName { get; set; }
        public string? Description { get; set; }
        public string? VaccineName { get; set; }
        public DateTime? NextDueDate { get; set; }
        public decimal? Cost { get; set; }
        public string? Notes { get; set; }
    }

    // Response DTOs
    public class MedicalRecordResponse
    {
        public string RecordId { get; set; } = string.Empty;
        public string PetId { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public string VeterinarianId { get; set; } = string.Empty;
        public string VeterinarianName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VaccineName { get; set; } = string.Empty;
        public DateTime? NextDueDate { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

