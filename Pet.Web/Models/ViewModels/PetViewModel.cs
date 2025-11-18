namespace Pet.Web.Models.ViewModels
{
    public class PetViewModel
    {
        public string PetId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Available";
        public DateTime IntakeDate { get; set; }
        public List<string> PhotoUrls { get; set; } = new List<string>();
        public bool Vaccinated { get; set; }
        public bool Neutered { get; set; }
        public bool GoodWithKids { get; set; }
        public bool GoodWithPets { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class CreatePetViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Vaccinated { get; set; }
        public bool Neutered { get; set; }
        public bool GoodWithKids { get; set; }
        public bool GoodWithPets { get; set; }
        public IFormFile? Photo { get; set; }
        
        // Optional medical record fields
        public bool CreateMedicalRecord { get; set; }
        public string? MedicalRecordType { get; set; }
        public DateTime? MedicalRecordDate { get; set; }
        public string? MedicalRecordDescription { get; set; }
        public string? MedicalRecordVaccineName { get; set; }
        public DateTime? MedicalRecordNextDueDate { get; set; }
        public decimal MedicalRecordCost { get; set; }
        public string? MedicalRecordNotes { get; set; }
    }
}
