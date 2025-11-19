namespace Pet.API.Models.DTOs
{
    // Request DTOs
    public class CreatePetRequest
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
        public List<string> PhotoUrls { get; set; } = new List<string>();
    }

    public class UpdatePetRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Available";
        public bool Vaccinated { get; set; }
        public bool Neutered { get; set; }
        public bool GoodWithKids { get; set; }
        public bool GoodWithPets { get; set; }
        public List<string> PhotoUrls { get; set; } = new List<string>();
    }

    // Response DTOs
    public class PetResponse
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
}

