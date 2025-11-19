namespace Pet.API.Models.DTOs
{
    // Request DTOs
    public class CreateAdoptionRequest
    {
        public string PetId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserFirstName { get; set; } = string.Empty;
        public string UserLastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string HousingType { get; set; } = string.Empty;
        public bool HasYard { get; set; }
        public bool HasOtherPets { get; set; }
        public string OtherPetsDescription { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public string ChildrenAges { get; set; } = string.Empty;
        public string EmploymentStatus { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateAdoptionStatusRequest
    {
        public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected
        public string ReviewedBy { get; set; } = string.Empty;
        public string? ReviewNotes { get; set; }
    }

    public class UpdateAdoptionRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string HousingType { get; set; } = string.Empty;
        public bool HasYard { get; set; }
        public bool HasOtherPets { get; set; }
        public string OtherPetsDescription { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public string ChildrenAges { get; set; } = string.Empty;
        public string EmploymentStatus { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    // Response DTOs
    public class AdoptionResponse
    {
        public string AdoptionId { get; set; } = string.Empty;
        public string PetId { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserFirstName { get; set; } = string.Empty;
        public string UserLastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string HousingType { get; set; } = string.Empty;
        public bool HasYard { get; set; }
        public bool HasOtherPets { get; set; }
        public string OtherPetsDescription { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public string ChildrenAges { get; set; } = string.Empty;
        public string EmploymentStatus { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime ApplicationDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? ReviewedBy { get; set; }
        public string? ReviewNotes { get; set; }
    }
}

