namespace Pet.API.Models.Enums
{
    /// <summary>
    /// Represents the available user roles in the system.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Public user role - basic access
        /// </summary>
        Public = 0,

        /// <summary>
        /// Staff user role - elevated access for pet shelter staff
        /// </summary>
        Staff = 1,

        /// <summary>
        /// Admin user role - full administrative access
        /// </summary>
        Admin = 2
    }
}

