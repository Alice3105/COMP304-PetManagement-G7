namespace Pet.API.Models.Enums
{
    /// <summary>
    /// Contains role name constants and helper methods for role validation.
    /// </summary>
    public static class RoleConstants
    {
        /// <summary>
        /// Admin role name constant
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        /// Staff role name constant
        /// </summary>
        public const string Staff = "Staff";

        /// <summary>
        /// Public role name constant
        /// </summary>
        public const string Public = "Public";

        /// <summary>
        /// Default role assigned to new users
        /// </summary>
        public const string DefaultRole = Public;

        /// <summary>
        /// Array of all valid role names
        /// </summary>
        public static readonly string[] AllRoles = { Admin, Staff, Public };

        /// <summary>
        /// Validates if the provided role string is a valid role.
        /// </summary>
        /// <param name="role">The role string to validate</param>
        /// <returns>True if the role is valid, otherwise false</returns>
        public static bool IsValidRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return false;

            return AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normalizes the role string (case-insensitive) and returns the proper case version.
        /// Returns default role if invalid.
        /// </summary>
        /// <param name="role">The role string to normalize</param>
        /// <returns>The normalized role string, or DefaultRole if invalid</returns>
        public static string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return DefaultRole;

            var roleUpper = role.Trim();
            return roleUpper switch
            {
                var r when r.Equals(Admin, StringComparison.OrdinalIgnoreCase) => Admin,
                var r when r.Equals(Staff, StringComparison.OrdinalIgnoreCase) => Staff,
                var r when r.Equals(Public, StringComparison.OrdinalIgnoreCase) => Public,
                _ => DefaultRole
            };
        }

        /// <summary>
        /// Converts a role string to UserRole enum.
        /// </summary>
        /// <param name="role">The role string to convert</param>
        /// <returns>The corresponding UserRole enum value, or UserRole.Public if invalid</returns>
        public static UserRole ToUserRole(string? role)
        {
            var normalizedRole = NormalizeRole(role);
            return normalizedRole switch
            {
                Admin => UserRole.Admin,
                Staff => UserRole.Staff,
                Public => UserRole.Public,
                _ => UserRole.Public
            };
        }

        /// <summary>
        /// Converts a UserRole enum to its string representation.
        /// </summary>
        /// <param name="role">The UserRole enum value</param>
        /// <returns>The string representation of the role</returns>
        public static string FromUserRole(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => Admin,
                UserRole.Staff => Staff,
                UserRole.Public => Public,
                _ => Public
            };
        }

        /// <summary>
        /// Checks if a role has admin privileges.
        /// </summary>
        /// <param name="role">The role string to check</param>
        /// <returns>True if the role is Admin</returns>
        public static bool IsAdmin(string? role)
        {
            return NormalizeRole(role) == Admin;
        }

        /// <summary>
        /// Checks if a role has staff or admin privileges.
        /// </summary>
        /// <param name="role">The role string to check</param>
        /// <returns>True if the role is Staff or Admin</returns>
        public static bool IsStaffOrAdmin(string? role)
        {
            var normalized = NormalizeRole(role);
            return normalized == Staff || normalized == Admin;
        }
    }
}

