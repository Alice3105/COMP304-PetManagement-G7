namespace Pet.API.Models.Entities
{
    public class MedicalRecord
    {
        public string RecordId { get; set; } = string.Empty;
        public string PetId { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty; // Vaccination, General Checkup, Treatment, Follow-up, Surgery
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;
        public string VeterinarianId { get; set; } = string.Empty;
        public string VeterinarianName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VaccineName { get; set; } = string.Empty; // Optional, only for vaccinations
        public DateTime? NextDueDate { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}

