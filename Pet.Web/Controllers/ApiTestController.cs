using Microsoft.AspNetCore.Mvc;
using Pet.Web.Services.Interfaces;
using System.Text.Json;

namespace Pet.Web.Controllers
{
    /// <summary>
    /// Controller for testing API integration
    /// This controller demonstrates that the web app correctly consumes the API
    /// </summary>
    public class ApiTestController : Controller
    {
        private readonly IPetApiService _petApiService;
        private readonly IAuthApiService _authApiService;
        private readonly IAdoptionApiService _adoptionApiService;
        private readonly IMedicalRecordApiService _medicalRecordApiService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiTestController> _logger;

        public ApiTestController(
            IPetApiService petApiService,
            IAuthApiService authApiService,
            IAdoptionApiService adoptionApiService,
            IMedicalRecordApiService medicalRecordApiService,
            IConfiguration configuration,
            ILogger<ApiTestController> logger)
        {
            _petApiService = petApiService;
            _authApiService = authApiService;
            _adoptionApiService = adoptionApiService;
            _medicalRecordApiService = medicalRecordApiService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Displays API integration status and configuration
        /// GET: /ApiTest
        /// </summary>
        public IActionResult Index()
        {
            var config = new
            {
                ApiBaseUrl = _configuration["PetApiBaseUrl"] ?? "Not configured",
                ApiKey = _configuration["ApigeeApiKey"] != null ? "***Configured***" : "Not configured",
                ServicesRegistered = new
                {
                    PetApiService = _petApiService != null,
                    AuthApiService = _authApiService != null,
                    AdoptionApiService = _adoptionApiService != null,
                    MedicalRecordApiService = _medicalRecordApiService != null
                }
            };

            ViewBag.Configuration = config;
            return View();
        }

        /// <summary>
        /// Tests API connectivity by attempting to fetch pets
        /// GET: /ApiTest/TestConnectivity
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TestConnectivity()
        {
            try
            {
                _logger.LogInformation("Testing API connectivity...");
                var pets = await _petApiService.GetAllPetsAsync();
                
                return Json(new
                {
                    success = true,
                    message = "API connectivity test successful",
                    petsCount = pets?.Count ?? 0,
                    apiBaseUrl = _configuration["PetApiBaseUrl"],
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API connectivity test failed");
                return Json(new
                {
                    success = false,
                    message = $"API connectivity test failed: {ex.Message}",
                    apiBaseUrl = _configuration["PetApiBaseUrl"],
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Tests all API endpoints
        /// GET: /ApiTest/TestEndpoints
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TestEndpoints()
        {
            var results = new List<object>();

            // Test Pets API
            try
            {
                var pets = await _petApiService.GetAllPetsAsync();
                results.Add(new
                {
                    endpoint = "GET /api/pets",
                    status = "success",
                    message = $"Retrieved {pets?.Count ?? 0} pets"
                });
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    endpoint = "GET /api/pets",
                    status = "error",
                    message = ex.Message
                });
            }

            // Test Adoptions API
            try
            {
                var adoptions = await _adoptionApiService.GetAllAdoptionsAsync();
                results.Add(new
                {
                    endpoint = "GET /api/adoptions",
                    status = "success",
                    message = $"Retrieved {adoptions?.Count ?? 0} adoptions"
                });
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    endpoint = "GET /api/adoptions",
                    status = "error",
                    message = ex.Message
                });
            }

            return Json(new
            {
                success = true,
                results = results,
                apiBaseUrl = _configuration["PetApiBaseUrl"],
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Returns API configuration information
        /// GET: /ApiTest/Configuration
        /// </summary>
        [HttpGet]
        public IActionResult Configuration()
        {
            var config = new
            {
                apiBaseUrl = _configuration["PetApiBaseUrl"] ?? "Not configured",
                apiKeyConfigured = !string.IsNullOrEmpty(_configuration["ApigeeApiKey"]),
                services = new
                {
                    petApiService = _petApiService != null ? "Registered" : "Not registered",
                    authApiService = _authApiService != null ? "Registered" : "Not registered",
                    adoptionApiService = _adoptionApiService != null ? "Registered" : "Not registered",
                    medicalRecordApiService = _medicalRecordApiService != null ? "Registered" : "Not registered"
                },
                endpoints = new
                {
                    pets = new[]
                    {
                        "GET /api/pets",
                        "GET /api/pets/{id}",
                        "POST /api/pets",
                        "PUT /api/pets/{id}",
                        "PATCH /api/pets/{id}",
                        "DELETE /api/pets/{id}"
                    },
                    adoptions = new[]
                    {
                        "GET /api/adoptions",
                        "GET /api/adoptions/{id}",
                        "GET /api/adoptions/user/{userId}",
                        "POST /api/adoptions",
                        "PUT /api/adoptions/{id}",
                        "PATCH /api/adoptions/{id}/status",
                        "DELETE /api/adoptions/{id}"
                    },
                    medicalRecords = new[]
                    {
                        "GET /api/medicalrecords/pet/{petId}",
                        "POST /api/medicalrecords",
                        "PUT /api/medicalrecords/{id}",
                        "DELETE /api/medicalrecords/{id}"
                    },
                    auth = new[]
                    {
                        "POST /api/auth/register",
                        "POST /api/auth/login",
                        "GET /api/auth/users",
                        "PUT /api/auth/users/{userId}",
                        "DELETE /api/auth/users/{userId}",
                        "PATCH /api/auth/password"
                    }
                }
            };

            return Json(config);
        }
    }
}

