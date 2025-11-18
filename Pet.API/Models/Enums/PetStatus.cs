namespace Pet.API.Models.Enums
{
    public enum PetStatus
    {
        Available,
        Pending,
        Adopted,
        MedicalHold
    }

    public static class PetStatusExtensions
    {
        public static string ToStringValue(this PetStatus status)
        {
            return status switch
            {
                PetStatus.Available => "Available",
                PetStatus.Pending => "Pending",
                PetStatus.Adopted => "Adopted",
                PetStatus.MedicalHold => "MedicalHold",
                _ => "Available"
            };
        }

        public static PetStatus FromString(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return PetStatus.Available;

            return status.ToLower() switch
            {
                "available" => PetStatus.Available,
                "pending" => PetStatus.Pending,
                "adopted" => PetStatus.Adopted,
                "medicalhold" => PetStatus.MedicalHold,
                _ => PetStatus.Available
            };
        }
    }
}

