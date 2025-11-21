using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Pet.API.Services.Interfaces;
using Pet.API.Models.Enums;
using Pet.API.Repositories.Interfaces;
using Pet.API.Models.Entities;
using PetEntity = Pet.API.Models.Entities.Pet;

namespace Pet.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDynamoDBContext _dynamoDBContext;
        private readonly AmazonDynamoDBClient _client;
        private readonly string _usersTable;
        private readonly ILogger<AuthController> _logger;
        private readonly IAdoptionRepository _adoptionRepository;
        private readonly IPetRepository _petRepository;

        public AuthController(
            IDynamoDBContext dynamoDBContext, 
            ILogger<AuthController> logger,
            IAdoptionRepository adoptionRepository,
            IPetRepository petRepository)
        {
            _dynamoDBContext = dynamoDBContext;
            _client = dynamoDBContext.Client;
            _usersTable = dynamoDBContext.UsersTableName;
            _logger = logger;
            _adoptionRepository = adoptionRepository;
            _petRepository = petRepository;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] SimpleRegisterRequest request)
        {
            _logger.LogInformation($"Endpoint: Register, Method: POST");
            try
            {
                // Check if user already exists
                SimpleUser? existingUser = await GetUserByEmail(request.Email);
                if (existingUser != null)
                    return BadRequest(new { message = "Email already exists" });

                // Validate and normalize role
                string normalizedRole = RoleConstants.NormalizeRole(request.Role);
                
                // Generate new user
                string userId = $"user-{Guid.NewGuid():N}";
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Create user in DynamoDB
                Dictionary<string, AttributeValue> item = new Dictionary<string, AttributeValue>
                {
                    { "UserId", new AttributeValue { S = userId } },
                    { "Email", new AttributeValue { S = request.Email } },
                    { "PasswordHash", new AttributeValue { S = passwordHash } },
                    { "FirstName", new AttributeValue { S = request.FirstName } },
                    { "LastName", new AttributeValue { S = request.LastName } },
                    { "Role", new AttributeValue { S = normalizedRole } },
                    { "CreatedDate", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "IsActive", new AttributeValue { BOOL = true } }
                };

                PutItemRequest putRequest = new PutItemRequest
                {
                    TableName = _usersTable,
                    Item = item
                };

                await _client.PutItemAsync(putRequest);

                _logger.LogInformation($"User {request.Email} registered successfully");

                return Ok(new
                {
                    userId,
                    email = request.Email,
                    firstName = request.FirstName,
                    lastName = request.LastName,
                    role = normalizedRole,
                    message = "User registered successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration failed: {ex.Message}");
                return StatusCode(500, new { message = "Registration failed", error = ex.Message });
            }
        }

        // POST: api/auth/login
        // Authenticates user using email and password only
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] SimpleLoginRequest request)
        {
            _logger.LogInformation($"Endpoint: Login, Method: POST");
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                // Get user by email
                SimpleUser? user = await GetUserByEmail(request.Email);
                if (user == null)
                {
                    _logger.LogWarning($"Login failed - user not found: {request.Email}");
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Verify password using BCrypt
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Login failed - invalid password for: {request.Email}");
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning($"Login failed - account inactive: {user.Email}");
                    return Unauthorized(new { message = "Account is inactive" });
                }

                _logger.LogInformation($"Login successful: {user.Email}");

                return Ok(new
                {
                    userId = user.UserId,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role,
                    message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login failed: {ex.Message}");
                return StatusCode(500, new { message = "Login failed", error = ex.Message });
            }
        }

        // PATCH: api/auth/password
        // Allows all authenticated users to change their own password
        [HttpPatch("password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            _logger.LogInformation($"Endpoint: ChangePassword, Method: PATCH");
            try
            {
                string? userEmail = Request.Headers["X-User-Email"].FirstOrDefault();
                string? userRole = Request.Headers["X-User-Role"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(userRole))
                {
                    _logger.LogWarning("Password change denied - user email or role not provided");
                    return Unauthorized(new { message = "User email and role are required. Provide them in X-User-Email and X-User-Role headers" });
                }

                if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest(new { message = "Current password and new password are required" });
                }

                if (request.NewPassword.Length < 6)
                {
                    return BadRequest(new { message = "New password must be at least 6 characters long" });
                }

                SimpleUser? user = await GetUserByEmail(userEmail);
                if (user == null)
                {
                    _logger.LogWarning($"Password change denied - user not found: {userEmail}");
                    return Unauthorized(new { message = "User not found" });
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning($"Password change denied - account inactive: {user.Email}");
                    return Unauthorized(new { message = "Account is inactive" });
                }

                if (user.Role != userRole)
                {
                    _logger.LogWarning($"Password change denied - role mismatch: Header={userRole}, DB={user.Role}");
                    return Unauthorized(new { message = "Role mismatch" });
                }

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning($"Password change failed - invalid current password for: {userEmail}");
                    return Unauthorized(new { message = "Current password is incorrect" });
                }

                string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                var updateRequest = new UpdateItemRequest
                {
                    TableName = _usersTable,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "UserId", new AttributeValue { S = user.UserId } }
                    },
                    UpdateExpression = "SET PasswordHash = :passwordHash",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":passwordHash", new AttributeValue { S = newPasswordHash } }
                    }
                };

                await _client.UpdateItemAsync(updateRequest);

                _logger.LogInformation($"Password changed successfully for user: {userEmail}");

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Password change failed: {ex.Message}");
                return StatusCode(500, new { message = "Password change failed", error = ex.Message });
            }
        }

        // GET: api/auth/test
        [HttpGet("test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Test()
        {
            _logger.LogInformation($"Endpoint: Test, Method: GET");
            return Ok(new
            {
                message = "Simple Auth controller is working!",
                timestamp = DateTime.UtcNow,
                usesIdentity = false,
                authentication = "Simple Email + Password + BCrypt"
            });
        }

        // GET: api/auth/users
        // Admin only - Returns all users
        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<SimpleUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers()
        {
            _logger.LogInformation($"Endpoint: GetAllUsers, Method: GET");
            try
            {
                var adminCheck = await ValidateAdminAccess();
                if (adminCheck != null)
                    return adminCheck;

                var users = await GetAllUsersAsync();
                var userResponses = users.Select(u => new SimpleUserResponse
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate
                });

                _logger.LogInformation($"Retrieved {users.Count()} users");
                return Ok(userResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching all users: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching users", error = ex.Message });
            }
        }

        // PUT: api/auth/users/{userId}
        // Admin only - Update user profile (password or role)
        [HttpPut("users/{userId}")]
        [ProducesResponseType(typeof(SimpleUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
        {
            _logger.LogInformation($"Endpoint: UpdateUser, Method: PUT, UserId: {userId}");
            try
            {
                // Validate admin access
                var adminCheck = await ValidateAdminAccess();
                if (adminCheck != null)
                    return adminCheck;

                // Get existing user
                var existingUser = await GetUserByIdAsync(userId);
                if (existingUser == null)
                {
                    _logger.LogWarning($"User not found: {userId}");
                    return NotFound(new { message = "User not found" });
                }

                // Validate request
                if (string.IsNullOrWhiteSpace(request.Password) && string.IsNullOrWhiteSpace(request.Role))
                {
                    return BadRequest(new { message = "Either password or role must be provided" });
                }

                // Prepare update expression
                var updateExpression = new List<string>();
                var expressionAttributeNames = new Dictionary<string, string>();
                var expressionAttributeValues = new Dictionary<string, AttributeValue>();

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    updateExpression.Add("#PasswordHash = :passwordHash");
                    expressionAttributeNames.Add("#PasswordHash", "PasswordHash");
                    expressionAttributeValues.Add(":passwordHash", new AttributeValue { S = passwordHash });
                }

                // Update role if provided
                if (!string.IsNullOrWhiteSpace(request.Role))
                {
                    string normalizedRole = RoleConstants.NormalizeRole(request.Role);
                    if (!RoleConstants.IsValidRole(normalizedRole))
                    {
                        return BadRequest(new { message = "Invalid role. Valid roles are: Admin, Staff, Public" });
                    }
                    updateExpression.Add("#Role = :role");
                    expressionAttributeNames.Add("#Role", "Role");
                    expressionAttributeValues.Add(":role", new AttributeValue { S = normalizedRole });
                }

                // Build update request
                var updateRequest = new UpdateItemRequest
                {
                    TableName = _usersTable,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "UserId", new AttributeValue { S = userId } }
                    },
                    UpdateExpression = "SET " + string.Join(", ", updateExpression),
                    ExpressionAttributeNames = expressionAttributeNames,
                    ExpressionAttributeValues = expressionAttributeValues
                };

                await _client.UpdateItemAsync(updateRequest);

                // Get updated user
                var updatedUser = await GetUserByIdAsync(userId);
                if (updatedUser == null)
                {
                    return StatusCode(500, new { message = "User updated but could not retrieve updated data" });
                }

                _logger.LogInformation($"User {userId} updated successfully");

                return Ok(new SimpleUserResponse
                {
                    UserId = updatedUser.UserId,
                    Email = updatedUser.Email,
                    FirstName = updatedUser.FirstName,
                    LastName = updatedUser.LastName,
                    Role = updatedUser.Role,
                    IsActive = updatedUser.IsActive,
                    CreatedDate = updatedUser.CreatedDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user {userId}: {ex.Message}");
                return StatusCode(500, new { message = "Error updating user", error = ex.Message });
            }
        }

        // DELETE: api/auth/users/{userId}
        // Admin only - Delete user profile and all associated adoption applications
        [HttpDelete("users/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            _logger.LogInformation($"Endpoint: DeleteUser, Method: DELETE, UserId: {userId}");
            try
            {
                // Validate admin access
                var adminCheck = await ValidateAdminAccess();
                if (adminCheck != null)
                    return adminCheck;

                // Get existing user
                var existingUser = await GetUserByIdAsync(userId);
                if (existingUser == null)
                {
                    _logger.LogWarning($"User not found: {userId}");
                    return NotFound(new { message = "User not found" });
                }

                // Get all adoptions for this user
                IEnumerable<Adoption> userAdoptions = await _adoptionRepository.GetByUserIdAsync(userId);
                _logger.LogInformation($"Found {userAdoptions.Count()} adoption(s) for user {userId}");

                // Delete each adoption and update pet statuses
                foreach (Adoption adoption in userAdoptions)
                {
                    try
                    {
                        string petId = adoption.PetId;
                        await _adoptionRepository.DeleteAsync(adoption.AdoptionId);
                        _logger.LogInformation($"Deleted adoption {adoption.AdoptionId} for user {userId}");

                        // Update pet status if needed after deletion
                        PetEntity? pet = await _petRepository.GetByIdAsync(petId);
                        if (pet != null)
                        {
                            // Check remaining adoptions for this pet
                            IEnumerable<Adoption> remainingAdoptions = await _adoptionRepository.GetByPetIdAsync(petId);
                            
                            bool hasApprovedAdoption = remainingAdoptions.Any(a => a.Status == "Approved");
                            bool hasPendingAdoption = remainingAdoptions.Any(a => a.Status == "Pending");
                            
                            // Only update status if pet is not Adopted or MedicalHold
                            if (pet.Status != PetStatus.Adopted.ToStringValue() && pet.Status != PetStatus.MedicalHold.ToStringValue())
                            {
                                PetStatus newStatus = (hasApprovedAdoption, hasPendingAdoption) switch
                                {
                                    (true, _) => PetStatus.Adopted,      // Another adoption is approved, set to Adopted
                                    (false, true) => PetStatus.Pending,   // There are pending adoptions, keep as Pending
                                    _ => PetStatus.Available              // No other adoptions, make pet Available
                                };
                                
                                pet.Status = newStatus.ToStringValue();
                                await _petRepository.UpdateAsync(pet);
                                _logger.LogInformation($"Pet {petId} status updated to {newStatus} after adoption deletion for user {userId}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error deleting adoption {adoption.AdoptionId} for user {userId}");
                        // Continue with other adoptions even if one fails
                    }
                }

                // Delete user
                var deleteRequest = new DeleteItemRequest
                {
                    TableName = _usersTable,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "UserId", new AttributeValue { S = userId } }
                    }
                };

                await _client.DeleteItemAsync(deleteRequest);

                _logger.LogInformation($"User {userId} and {userAdoptions.Count()} adoption(s) deleted successfully");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user {userId}: {ex.Message}");
                return StatusCode(500, new { message = "Error deleting user", error = ex.Message });
            }
        }

        #region Helper Methods

        private async Task<SimpleUser?> GetUserByEmail(string email)
        {
            ScanRequest scanRequest = new ScanRequest
            {
                TableName = _usersTable,
                FilterExpression = "Email = :email",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":email", new AttributeValue { S = email } }
                }
            };

            ScanResponse response = await _client.ScanAsync(scanRequest);
            
            if (response.Items.Count == 0)
                return null;

            Dictionary<string, AttributeValue> item = response.Items[0];
            
            string? roleString = item.GetValueOrDefault("Role")?.S;
            string normalizedRole = RoleConstants.NormalizeRole(roleString);
            
            return new SimpleUser
            {
                UserId = item.GetValueOrDefault("UserId")?.S ?? "",
                Email = item.GetValueOrDefault("Email")?.S ?? "",
                PasswordHash = item.GetValueOrDefault("PasswordHash")?.S ?? "",
                FirstName = item.GetValueOrDefault("FirstName")?.S ?? "",
                LastName = item.GetValueOrDefault("LastName")?.S ?? "",
                Role = normalizedRole,
                IsActive = item.GetValueOrDefault("IsActive")?.BOOL ?? true,
                CreatedDate = item.GetValueOrDefault("CreatedDate")?.S
            };
        }


        private async Task<SimpleUser?> GetUserByIdAsync(string userId)
        {
            GetItemRequest getRequest = new GetItemRequest
            {
                TableName = _usersTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "UserId", new AttributeValue { S = userId } }
                }
            };

            GetItemResponse response = await _client.GetItemAsync(getRequest);
            
            if (!response.Item.Any())
                return null;

            Dictionary<string, AttributeValue> item = response.Item;
            
            string? roleString = item.GetValueOrDefault("Role")?.S;
            string normalizedRole = RoleConstants.NormalizeRole(roleString);
            
            return new SimpleUser
            {
                UserId = item.GetValueOrDefault("UserId")?.S ?? "",
                Email = item.GetValueOrDefault("Email")?.S ?? "",
                PasswordHash = item.GetValueOrDefault("PasswordHash")?.S ?? "",
                FirstName = item.GetValueOrDefault("FirstName")?.S ?? "",
                LastName = item.GetValueOrDefault("LastName")?.S ?? "",
                Role = normalizedRole,
                IsActive = item.GetValueOrDefault("IsActive")?.BOOL ?? true,
                CreatedDate = item.GetValueOrDefault("CreatedDate")?.S
            };
        }

        private async Task<IEnumerable<SimpleUser>> GetAllUsersAsync()
        {
            ScanRequest scanRequest = new ScanRequest
            {
                TableName = _usersTable
            };

            ScanResponse response = await _client.ScanAsync(scanRequest);
            
            var users = new List<SimpleUser>();
            
            foreach (var item in response.Items)
            {
                string? roleString = item.GetValueOrDefault("Role")?.S;
                string normalizedRole = RoleConstants.NormalizeRole(roleString);
                
                users.Add(new SimpleUser
                {
                    UserId = item.GetValueOrDefault("UserId")?.S ?? "",
                    Email = item.GetValueOrDefault("Email")?.S ?? "",
                    PasswordHash = item.GetValueOrDefault("PasswordHash")?.S ?? "",
                    FirstName = item.GetValueOrDefault("FirstName")?.S ?? "",
                    LastName = item.GetValueOrDefault("LastName")?.S ?? "",
                    Role = normalizedRole,
                    IsActive = item.GetValueOrDefault("IsActive")?.BOOL ?? true,
                    CreatedDate = item.GetValueOrDefault("CreatedDate")?.S
                });
            }
            
            return users;
        }

        private async Task<IActionResult?> ValidateAdminAccess()
        {
            // Get user email and role from headers (passed from Web project)
            string? userEmail = Request.Headers["X-User-Email"].FirstOrDefault();
            string? userRole = Request.Headers["X-User-Role"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(userRole))
            {
                _logger.LogWarning("Admin access denied - user email or role not provided");
                return Unauthorized(new { message = "User email and role are required. Provide them in X-User-Email and X-User-Role headers" });
            }

            // Get user by email to verify they exist and validate their role
            var user = await GetUserByEmail(userEmail);
            if (user == null)
            {
                _logger.LogWarning($"Admin access denied - user not found: {userEmail}");
                return Unauthorized(new { message = "User not found" });
            }

            if (!user.IsActive)
            {
                _logger.LogWarning($"Admin access denied - account inactive: {user.Email}");
                return Unauthorized(new { message = "Account is inactive" });
            }

            // Verify the role in header matches the role in database
            if (user.Role != userRole)
            {
                _logger.LogWarning($"Admin access denied - role mismatch: Header={userRole}, DB={user.Role}");
                return Unauthorized(new { message = "Role mismatch" });
            }

            // Validate admin role - this is the key check
            if (!RoleConstants.IsAdmin(user.Role))
            {
                _logger.LogWarning($"Admin access denied - insufficient privileges: {user.Email} (Role: {user.Role})");
                return StatusCode(403, new { message = "Admin access required" });
            }

            return null; // Admin access granted
        }

        #endregion
    }

    // Simple DTOs
    public class SimpleRegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Role { get; set; } = RoleConstants.DefaultRole;
    }

    public class SimpleLoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class SimpleUser
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Role { get; set; } = RoleConstants.DefaultRole;
        public bool IsActive { get; set; } = true;
        public string? CreatedDate { get; set; }
    }

    public class SimpleUserResponse
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Role { get; set; } = RoleConstants.DefaultRole;
        public bool IsActive { get; set; } = true;
        public string? CreatedDate { get; set; }
    }

    public class UpdateUserRequest
    {
        public string? Password { get; set; }
        public string? Role { get; set; }
    }

    public class ChangePasswordRequest
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }

    // Extension helper
    public static class DynamoDBExtensions
    {
        public static AttributeValue? GetValueOrDefault(this Dictionary<string, AttributeValue> dict, string key)
        {
            return dict.ContainsKey(key) ? dict[key] : null;
        }
    }
}

