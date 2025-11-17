using Microsoft.AspNetCore.Mvc;
using Pet.Web.Models.ViewModels;
using Pet.Web.Services;

namespace Pet.Web.Controllers
{
    public class PetsController : Controller
    {
        private readonly IPetApiService _petApiService;
        private readonly ILogger<PetsController> _logger;

        public PetsController(IPetApiService petApiService, ILogger<PetsController> logger)
        {
            _petApiService = petApiService;
            _logger = logger;
        }

        // GET: /Pets
        public async Task<IActionResult> Index()
        {
            var pets = await _petApiService.GetAllPetsAsync();
            return View(pets);
        }

        // GET: /Pets/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var pet = await _petApiService.GetPetByIdAsync(id);

            if (pet == null)
            {
                TempData["Error"] = "Pet not found";
                return RedirectToAction(nameof(Index));
            }

            return View(pet);
        }

        // GET: /Pets/Create
        public IActionResult Create()
        {
            // Check if user is logged in and is Staff or Admin
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "Staff" && role != "Admin"))
            {
                TempData["Error"] = "You must be a staff member to add pets";
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // POST: /Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePetViewModel model)
        {
            // Check authorization
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "Staff" && role != "Admin"))
            {
                TempData["Error"] = "You must be a staff member to add pets";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var createdPet = await _petApiService.CreatePetAsync(model);

            if (createdPet == null)
            {
                TempData["Error"] = "Failed to create pet. Please try again.";
                return View(model);
            }

            TempData["Success"] = $"Pet '{createdPet.Name}' added successfully!";
            return RedirectToAction(nameof(Details), new { id = createdPet.PetId });
        }

        // GET: /Pets/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            // Check authorization
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "Staff" && role != "Admin"))
            {
                TempData["Error"] = "You must be a staff member to edit pets";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(id))
                return NotFound();

            var pet = await _petApiService.GetPetByIdAsync(id);

            if (pet == null)
            {
                TempData["Error"] = "Pet not found";
                return RedirectToAction(nameof(Index));
            }

            return View(pet);
        }

        // POST: /Pets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PetViewModel model)
        {
            // Check authorization
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "Staff" && role != "Admin"))
            {
                TempData["Error"] = "You must be a staff member to edit pets";
                return RedirectToAction(nameof(Index));
            }

            if (id != model.PetId)
            {
                TempData["Error"] = "Invalid pet ID";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _petApiService.UpdatePetAsync(id, model);

            if (!success)
            {
                TempData["Error"] = "Failed to update pet. Please try again.";
                return View(model);
            }

            TempData["Success"] = $"Pet '{model.Name}' updated successfully!";
            return RedirectToAction(nameof(Details), new { id = model.PetId });
        }

        // POST: /Pets/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            // Check authorization
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "Staff" && role != "Admin"))
            {
                TempData["Error"] = "You must be a staff member to delete pets";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid pet ID";
                return RedirectToAction(nameof(Index));
            }

            var success = await _petApiService.DeletePetAsync(id);

            if (!success)
            {
                TempData["Error"] = "Failed to delete pet. Please try again.";
            }
            else
            {
                TempData["Success"] = "Pet deleted successfully";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
