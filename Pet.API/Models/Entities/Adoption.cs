namespace Pet.API.Models.Entities
{
    public class Adoption
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
        public string HousingType { get; set; } = string.Empty; // House, Apartment, Condo, etc.
        public bool HasYard { get; set; }
        public bool HasOtherPets { get; set; }
        public string OtherPetsDescription { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public string ChildrenAges { get; set; } = string.Empty;
        public string EmploymentStatus { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty; // Why they want to adopt
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedDate { get; set; }
        public string? ReviewedBy { get; set; }
        public string? ReviewNotes { get; set; }
    }
}
