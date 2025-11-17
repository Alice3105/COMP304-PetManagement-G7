using Microsoft.AspNetCore.Mvc;
using Pet.Web.Models.ViewModels;
using Pet.Web.Services;
using System.Text.Json;

namespace Pet.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthApiService _authApiService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthApiService authApiService, ILogger<AuthController> logger)
        {
            _authApiService = authApiService;
            _logger = logger;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Redirect to home if already logged in
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userSession = await _authApiService.LoginAsync(model);

            if (userSession == null)
            {
                TempData["Error"] = "Invalid email or password";
                return View(model);
            }

            // Store user session
            HttpContext.Session.SetString("UserId", userSession.UserId);
            HttpContext.Session.SetString("Email", userSession.Email);
            HttpContext.Session.SetString("FirstName", userSession.FirstName);
            HttpContext.Session.SetString("LastName", userSession.LastName);
            HttpContext.Session.SetString("Role", userSession.Role);        // ?? Role stored here
            HttpContext.Session.SetString("ApiKey", userSession.ApiKey);

            TempData["Success"] = $"Welcome back, {userSession.FirstName}!";
            _logger.LogInformation($"User logged in: {userSession.Email}");

            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            // Redirect to home if already logged in
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
                return View(model);
            }

            var userSession = await _authApiService.RegisterAsync(model);

            if (userSession == null)
            {
                TempData["Error"] = "Registration failed. Email may already be in use.";
                return View(model);
            }

            // Store user session
            HttpContext.Session.SetString("UserId", userSession.UserId);
            HttpContext.Session.SetString("Email", userSession.Email);
            HttpContext.Session.SetString("FirstName", userSession.FirstName);
            HttpContext.Session.SetString("LastName", userSession.LastName);
            HttpContext.Session.SetString("Role", userSession.Role);       // ?? Role stored here too
            HttpContext.Session.SetString("ApiKey", userSession.ApiKey);

            TempData["Success"] = $"Welcome, {userSession.FirstName}! Your account has been created.";
            _logger.LogInformation($"New user registered: {userSession.Email}");

            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            var firstName = HttpContext.Session.GetString("FirstName");

            HttpContext.Session.Clear();

            TempData["Success"] = $"Goodbye, {firstName}! You have been logged out.";
            _logger.LogInformation("User logged out");

            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            // View shown when user lacks proper Staff/Admin role
            return View();
        }
    }
}
