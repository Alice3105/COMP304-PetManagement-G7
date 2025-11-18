using Microsoft.AspNetCore.Mvc;
using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;

namespace Pet.Web.Controllers
{
    public class AdoptionsController : Controller
    {
        private readonly IAdoptionApiService _adoptionApiService;
        private readonly IPetApiService _petApiService;
        private readonly ILogger<AdoptionsController> _logger;

        public AdoptionsController(
            IAdoptionApiService adoptionApiService,
            IPetApiService petApiService,
            ILogger<AdoptionsController> logger)
        {
            _adoptionApiService = adoptionApiService;
            _petApiService = petApiService;
            _logger = logger;
        }

        // GET: /Adoptions
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            List<AdoptionViewModel> adoptions;

            if (role == "Staff" || role == "Admin")
            {
                // Staff can see all adoptions
                adoptions = await _adoptionApiService.GetAllAdoptionsAsync();
            }
            else if (!string.IsNullOrEmpty(userId))
            {
                // Users see only their own adoptions
                adoptions = await _adoptionApiService.GetAdoptionsByUserIdAsync(userId);
            }
            else
            {
                TempData["Error"] = "You must be logged in to view adoptions";
                return RedirectToAction("Login", "Auth");
            }

            return View(adoptions);
        }

        // GET: /Adoptions/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var adoption = await _adoptionApiService.GetAdoptionByIdAsync(id);

            if (adoption == null)
            {
                TempData["Error"] = "Adoption application not found";
                return RedirectToAction(nameof(Index));
            }

            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            // Check authorization: owner or staff
            if (adoption.UserId != userId && role != "Staff" && role != "Admin")
            {
                TempData["Error"] = "You are not authorized to view this application";
                return RedirectToAction(nameof(Index));
            }

            return View(adoption);
        }

        // GET: /Adoptions/Create?petId=xxx
        public async Task<IActionResult> Create(string petId)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You must be logged in to adopt a pet";
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(petId))
            {
                TempData["Error"] = "Invalid pet ID";
                return RedirectToAction("Index", "Pets");
            }

            var pet = await _petApiService.GetPetByIdAsync(petId);

            if (pet == null)
            {
                TempData["Error"] = "Pet not found";
                return RedirectToAction("Index", "Pets");
            }

            if (pet.Status != "Available")
            {
                TempData["Error"] = $"Pet '{pet.Name}' is not available for adoption";
                return RedirectToAction("Details", "Pets", new { id = petId });
            }

            ViewBag.PetName = pet.Name;
            ViewBag.PetId = petId;

            return View(new CreateAdoptionViewModel { PetId = petId });
        }

        // POST: /Adoptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAdoptionViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var email = HttpContext.Session.GetString("Email");
            var firstName = HttpContext.Session.GetString("FirstName");
            var lastName = HttpContext.Session.GetString("LastName");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You must be logged in to adopt a pet";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                var pet = await _petApiService.GetPetByIdAsync(model.PetId);
                ViewBag.PetName = pet?.Name ?? "Unknown";
                ViewBag.PetId = model.PetId;
                return View(model);
            }

            var adoption = await _adoptionApiService.CreateAdoptionAsync(
                model,
                userId,
                email ?? "",
                firstName ?? "",
                lastName ?? ""
            );

            if (adoption == null)
            {
                TempData["Error"] = "Failed to submit adoption application. Please try again.";
                var pet = await _petApiService.GetPetByIdAsync(model.PetId);
                ViewBag.PetName = pet?.Name ?? "Unknown";
                ViewBag.PetId = model.PetId;
                return View(model);
            }

            TempData["Success"] = "Adoption application submitted successfully! We'll review it soon.";
            return RedirectToAction(nameof(Details), new { id = adoption.AdoptionId });
        }

        // POST: /Adoptions/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id, string reviewNotes)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Staff" && role != "Admin")
            {
                TempData["Error"] = "Only staff members can approve adoptions";
                return RedirectToAction(nameof(Index));
            }

            var reviewedBy = HttpContext.Session.GetString("Email") ?? "Staff";

            var success = await _adoptionApiService.UpdateAdoptionStatusAsync(id, "Approved", reviewedBy, reviewNotes);

            if (success)
            {
                TempData["Success"] = "Adoption application approved successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to approve adoption application";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Adoptions/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id, string reviewNotes)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Staff" && role != "Admin")
            {
                TempData["Error"] = "Only staff members can reject adoptions";
                return RedirectToAction(nameof(Index));
            }

            var reviewedBy = HttpContext.Session.GetString("Email") ?? "Staff";

            var success = await _adoptionApiService.UpdateAdoptionStatusAsync(id, "Rejected", reviewedBy, reviewNotes);

            if (success)
            {
                TempData["Success"] = "Adoption application rejected";
            }
            else
            {
                TempData["Error"] = "Failed to reject adoption application";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Adoptions/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, string status, string? reviewNotes = null)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Staff" && role != "Admin")
            {
                TempData["Error"] = "Only staff members can update adoption status";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(status) || (status != "Pending" && status != "Approved" && status != "Rejected"))
            {
                TempData["Error"] = "Invalid status. Status must be Pending, Approved, or Rejected";
                return RedirectToAction(nameof(Details), new { id });
            }

            var reviewedBy = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetString("Email") ?? "Staff";

            var success = await _adoptionApiService.UpdateAdoptionStatusAsync(id, status, reviewedBy, reviewNotes);

            if (success)
            {
                TempData["Success"] = $"Adoption application status updated to {status} successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to update adoption application status";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
