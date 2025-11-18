using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Pet.API.Services.Interfaces;
using Pet.API.Models.Enums;

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

        public AuthController(IDynamoDBContext dynamoDBContext, ILogger<AuthController> logger)
        {
            _dynamoDBContext = dynamoDBContext;
            _client = dynamoDBContext.Client;
            _usersTable = dynamoDBContext.UsersTableName;
            _logger = logger;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] SimpleRegisterRequest request)
        {
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
                string apiKey = $"sk_live_{Guid.NewGuid():N}";

                // Create user in DynamoDB
                Dictionary<string, AttributeValue> item = new Dictionary<string, AttributeValue>
                {
                    { "UserId", new AttributeValue { S = userId } },
                    { "Email", new AttributeValue { S = request.Email } },
                    { "PasswordHash", new AttributeValue { S = passwordHash } },
                    { "FirstName", new AttributeValue { S = request.FirstName } },
                    { "LastName", new AttributeValue { S = request.LastName } },
                    { "Role", new AttributeValue { S = normalizedRole } },
                    { "ApiKey", new AttributeValue { S = apiKey } },
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
                    apiKey,
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
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] SimpleLoginRequest request)
        {
            try
            {
                SimpleUser? user = await GetUserByEmail(request.Email);
                
                if (user == null)
                {
                    _logger.LogWarning($"Login failed - user not found: {request.Email}");
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning($"Login failed - account inactive: {request.Email}");
                    return Unauthorized(new { message = "Account is inactive" });
                }

                bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                
                if (!passwordValid)
                {
                    _logger.LogWarning($"Login failed - invalid password: {request.Email}");
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                _logger.LogInformation($"Login successful: {request.Email}");

                return Ok(new
                {
                    userId = user.UserId,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role,
                    apiKey = user.ApiKey,
                    message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login failed: {ex.Message}");
                return StatusCode(500, new { message = "Login failed", error = ex.Message });
            }
        }

        // GET: api/auth/test
        [HttpGet("test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Test()
        {
            return Ok(new
            {
                message = "Simple Auth controller is working!",
                timestamp = DateTime.UtcNow,
                usesIdentity = false,
                authentication = "Simple Email + Password + BCrypt"
            });
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
                ApiKey = item.GetValueOrDefault("ApiKey")?.S ?? "",
                IsActive = item.GetValueOrDefault("IsActive")?.BOOL ?? true
            };
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
        public string ApiKey { get; set; } = "";
        public bool IsActive { get; set; } = true;
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

