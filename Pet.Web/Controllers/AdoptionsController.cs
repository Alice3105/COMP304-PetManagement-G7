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
            string? userId = HttpContext.Session.GetString("UserId");
            string? role = HttpContext.Session.GetString("Role");

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

            AdoptionViewModel? adoption = await _adoptionApiService.GetAdoptionByIdAsync(id);

            if (adoption == null)
            {
                TempData["Error"] = "Adoption application not found";
                return RedirectToAction(nameof(Index));
            }

            string? userId = HttpContext.Session.GetString("UserId");
            string? role = HttpContext.Session.GetString("Role");

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
            string? userId = HttpContext.Session.GetString("UserId");

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

            PetViewModel? pet = await _petApiService.GetPetByIdAsync(petId);

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
            string? userId = HttpContext.Session.GetString("UserId");
            string? email = HttpContext.Session.GetString("Email");
            string? firstName = HttpContext.Session.GetString("FirstName");
            string? lastName = HttpContext.Session.GetString("LastName");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You must be logged in to adopt a pet";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                PetViewModel? pet = await _petApiService.GetPetByIdAsync(model.PetId);
                ViewBag.PetName = pet?.Name ?? "Unknown";
                ViewBag.PetId = model.PetId;
                return View(model);
            }

            AdoptionViewModel? adoption = await _adoptionApiService.CreateAdoptionAsync(
                model,
                userId,
                email ?? "",
                firstName ?? "",
                lastName ?? ""
            );

            if (adoption == null)
            {
                TempData["Error"] = "Failed to submit adoption application. Please try again.";
                PetViewModel? pet = await _petApiService.GetPetByIdAsync(model.PetId);
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
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Staff" && role != "Admin")
            {
                TempData["Error"] = "Only staff members can approve adoptions";
                return RedirectToAction(nameof(Index));
            }

            string reviewedBy = HttpContext.Session.GetString("Email") ?? "Staff";

            bool success = await _adoptionApiService.UpdateAdoptionStatusAsync(id, "Approved", reviewedBy, reviewNotes);

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
            string? role = HttpContext.Session.GetString("Role");

            if (role != "Staff" && role != "Admin")
            {
                TempData["Error"] = "Only staff members can reject adoptions";
                return RedirectToAction(nameof(Index));
            }

            string reviewedBy = HttpContext.Session.GetString("Email") ?? "Staff";

            bool success = await _adoptionApiService.UpdateAdoptionStatusAsync(id, "Rejected", reviewedBy, reviewNotes);

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
            string? role = HttpContext.Session.GetString("Role");

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

            string reviewedBy = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetString("Email") ?? "Staff";

            bool success = await _adoptionApiService.UpdateAdoptionStatusAsync(id, status, reviewedBy, reviewNotes);

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

        // GET: /Adoptions/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            string? userId = HttpContext.Session.GetString("UserId");
            string? role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You must be logged in to edit an adoption application";
                return RedirectToAction("Login", "Auth");
            }

            AdoptionViewModel? adoption = await _adoptionApiService.GetAdoptionByIdAsync(id);

            if (adoption == null)
            {
                TempData["Error"] = "Adoption application not found";
                return RedirectToAction(nameof(Index));
            }

            // Check authorization: only the owner can edit, and only if status is Pending
            if (adoption.UserId != userId)
            {
                TempData["Error"] = "You are not authorized to edit this application";
                return RedirectToAction(nameof(Index));
            }

            if (adoption.Status != "Pending")
            {
                TempData["Error"] = "You can only edit applications with Pending status";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.PetName = adoption.PetName;
            ViewBag.PetId = adoption.PetId;
            ViewBag.AdoptionId = adoption.AdoptionId;

            // Map AdoptionViewModel to CreateAdoptionViewModel for editing
            CreateAdoptionViewModel model = new CreateAdoptionViewModel
            {
                PetId = adoption.PetId,
                PhoneNumber = adoption.PhoneNumber,
                Address = adoption.Address,
                HousingType = adoption.HousingType,
                HasYard = adoption.HasYard,
                HasOtherPets = adoption.HasOtherPets,
                OtherPetsDescription = adoption.OtherPetsDescription,
                HasChildren = adoption.HasChildren,
                ChildrenAges = adoption.ChildrenAges,
                EmploymentStatus = adoption.EmploymentStatus,
                Reason = adoption.Reason
            };

            return View(model);
        }

        // POST: /Adoptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CreateAdoptionViewModel model)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            string? userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You must be logged in to edit an adoption application";
                return RedirectToAction("Login", "Auth");
            }

            AdoptionViewModel? existingAdoption = await _adoptionApiService.GetAdoptionByIdAsync(id);

            if (existingAdoption == null)
            {
                TempData["Error"] = "Adoption application not found";
                return RedirectToAction(nameof(Index));
            }

            // Check authorization: only the owner can edit
            if (existingAdoption.UserId != userId)
            {
                TempData["Error"] = "You are not authorized to edit this application";
                return RedirectToAction(nameof(Index));
            }

            // Check status: only Pending applications can be edited
            if (existingAdoption.Status != "Pending")
            {
                TempData["Error"] = "You can only edit applications with Pending status";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PetName = existingAdoption.PetName;
                ViewBag.PetId = existingAdoption.PetId;
                return View(model);
            }

            bool success = await _adoptionApiService.UpdateAdoptionAsync(id, model);

            if (!success)
            {
                TempData["Error"] = "Failed to update adoption application. Please try again.";
                ViewBag.PetName = existingAdoption.PetName;
                ViewBag.PetId = existingAdoption.PetId;
                return View(model);
            }

            TempData["Success"] = "Adoption application updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Adoptions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            string? userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You must be logged in to delete an adoption application";
                return RedirectToAction("Login", "Auth");
            }

            AdoptionViewModel? adoption = await _adoptionApiService.GetAdoptionByIdAsync(id);

            if (adoption == null)
            {
                TempData["Error"] = "Adoption application not found";
                return RedirectToAction(nameof(Index));
            }

            // Check authorization: only the owner can delete
            if (adoption.UserId != userId)
            {
                TempData["Error"] = "You are not authorized to delete this application";
                return RedirectToAction(nameof(Index));
            }

            // Check status: only Pending applications can be deleted
            if (adoption.Status != "Pending")
            {
                TempData["Error"] = "You can only delete applications with Pending status";
                return RedirectToAction(nameof(Details), new { id });
            }

            bool success = await _adoptionApiService.DeleteAdoptionAsync(id);

            if (!success)
            {
                TempData["Error"] = "Failed to delete adoption application. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Success"] = "Adoption application deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
