using Microsoft.AspNetCore.Mvc;
using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;
using Pet.Web.Attributes;


namespace Pet.Web.Controllers
{
    public class PetsController : Controller
    {
        private readonly IPetApiService _petApiService;
        private readonly IMedicalRecordApiService _medicalRecordApiService;
        private readonly ILogger<PetsController> _logger;

        public PetsController(IPetApiService petApiService, IMedicalRecordApiService medicalRecordApiService, ILogger<PetsController> logger)
        {
            _petApiService = petApiService;
            _medicalRecordApiService = medicalRecordApiService;
            _logger = logger;
        }

        // GET: /Pets
        public async Task<IActionResult> Index()
        {
            List<PetViewModel> pets = await _petApiService.GetAllPetsAsync();
            return View(pets);
        }

        // GET: /Pets/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            PetViewModel? pet = await _petApiService.GetPetByIdAsync(id);

            if (pet == null)
            {
                TempData["Error"] = "Pet not found";
                return RedirectToAction(nameof(Index));
            }

            // Fetch medical records for this pet
            List<MedicalRecordViewModel> medicalRecords = await _medicalRecordApiService.GetMedicalRecordsByPetIdAsync(id);
            ViewBag.MedicalRecords = medicalRecords;

            return View(pet);
        }

        // GET: /Pets/Create
        [SessionAuthorize("Staff", "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Pets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("Staff", "Admin")]
        public async Task<IActionResult> Create(CreatePetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            PetViewModel? createdPet = await _petApiService.CreatePetAsync(model);

            if (createdPet == null)
            {
                TempData["Error"] = "Failed to create pet. Please try again.";
                return View(model);
            }

            if (model.CreateMedicalRecord && !string.IsNullOrWhiteSpace(model.MedicalRecordType))
            {
                string? userId = HttpContext.Session.GetString("UserId");
                string firstName = HttpContext.Session.GetString("FirstName") ?? "Staff";
                string lastName = HttpContext.Session.GetString("LastName") ?? "Member";
                string veterinarianName = $"Dr. {firstName} {lastName}";

                MedicalRecordViewModel medicalRecord = new MedicalRecordViewModel
                {
                    PetId = createdPet.PetId,
                    PetName = createdPet.Name,
                    RecordType = model.MedicalRecordType,
                    RecordDate = model.MedicalRecordDate ?? DateTime.UtcNow,
                    VeterinarianId = userId ?? "",
                    VeterinarianName = veterinarianName,
                    Description = model.MedicalRecordDescription ?? "",
                    VaccineName = model.MedicalRecordVaccineName ?? "",
                    NextDueDate = model.MedicalRecordNextDueDate,
                    Cost = model.MedicalRecordCost,
                    Notes = model.MedicalRecordNotes ?? ""
                };

                MedicalRecordViewModel? createdRecord = await _medicalRecordApiService.CreateMedicalRecordAsync(medicalRecord);
                if (createdRecord != null)
                {
                    TempData["Success"] = $"Pet '{createdPet.Name}' and medical record added successfully!";
                }
                else
                {
                    TempData["Success"] = $"Pet '{createdPet.Name}' added successfully, but failed to create medical record.";
                }
            }
            else
            {
                TempData["Success"] = $"Pet '{createdPet.Name}' added successfully!";
            }

            return RedirectToAction(nameof(Details), new { id = createdPet.PetId });
        }

        // GET: /Pets/Edit/5
        [SessionAuthorize("Staff", "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            PetViewModel? pet = await _petApiService.GetPetByIdAsync(id);

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
        [SessionAuthorize("Staff", "Admin")]
        public async Task<IActionResult> Edit(string id, PetViewModel model)
        {
            if (id != model.PetId)
            {
                TempData["Error"] = "Invalid pet ID";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool success = await _petApiService.UpdatePetAsync(id, model);

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
        [SessionAuthorize("Staff", "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid pet ID";
                return RedirectToAction(nameof(Index));
            }

            bool success = await _petApiService.DeletePetAsync(id);

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
