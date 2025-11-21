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
            _logger.LogInformation($"PetsController.Create POST called. Model valid: {ModelState.IsValid}, Pet Name: {model?.Name}");
            
            if (model == null)
            {
                _logger.LogWarning("Create pet called with null model");
                TempData["Error"] = "Invalid pet data.";
                return RedirectToAction(nameof(Index));
            }
            
            // Clear validation errors for medical record fields if CreateMedicalRecord is false
            // This ensures optional medical record fields don't prevent pet creation
            if (!model.CreateMedicalRecord)
            {
                ModelState.Remove(nameof(model.MedicalRecordType));
                ModelState.Remove(nameof(model.MedicalRecordDate));
                ModelState.Remove(nameof(model.MedicalRecordDescription));
                ModelState.Remove(nameof(model.MedicalRecordVaccineName));
                ModelState.Remove(nameof(model.MedicalRecordNextDueDate));
                ModelState.Remove(nameof(model.MedicalRecordCost));
                ModelState.Remove(nameof(model.MedicalRecordNotes));
            }
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value?.Errors.Select(e => e.ErrorMessage) })
                    .ToList();
                
                _logger.LogWarning($"Model validation failed. Field errors:");
                foreach (var error in errors)
                {
                    _logger.LogWarning($"  Field '{error.Field}': {string.Join(", ", error.Errors ?? Enumerable.Empty<string>())}");
                }
                
                return View(model);
            }

            _logger.LogInformation($"Calling _petApiService.CreatePetAsync for pet: {model.Name}");
            PetViewModel? createdPet = await _petApiService.CreatePetAsync(model);

            if (createdPet == null)
            {
                _logger.LogWarning($"Failed to create pet. CreatePetAsync returned null.");
                TempData["Error"] = "Failed to create pet. Please try again.";
                return View(model);
            }
            
            _logger.LogInformation($"Pet created successfully: {createdPet.PetId} - {createdPet.Name}");

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
                    Cost = model.MedicalRecordCost ?? 0,
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

            // Fetch medical records for this pet
            List<MedicalRecordViewModel> medicalRecords = await _medicalRecordApiService.GetMedicalRecordsByPetIdAsync(id);
            ViewBag.MedicalRecords = medicalRecords;

            return View(pet);
        }

        // POST: /Pets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("Staff", "Admin")]
        public async Task<IActionResult> Edit(string id, PetViewModel model, IFormCollection form)
        {
            if (id != model.PetId)
            {
                TempData["Error"] = "Invalid pet ID";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                // Fetch medical records for this pet
                List<MedicalRecordViewModel> medicalRecords = await _medicalRecordApiService.GetMedicalRecordsByPetIdAsync(id);
                ViewBag.MedicalRecords = medicalRecords;
                return View(model);
            }

            bool success = await _petApiService.UpdatePetAsync(id, model);

            if (!success)
            {
                TempData["Error"] = "Failed to update pet. Please try again.";
                // Fetch medical records for this pet
                List<MedicalRecordViewModel> medicalRecords = await _medicalRecordApiService.GetMedicalRecordsByPetIdAsync(id);
                ViewBag.MedicalRecords = medicalRecords;
                return View(model);
            }

            // Check if a new medical record should be created
            bool createMedicalRecord = form.ContainsKey("CreateMedicalRecord") && form["CreateMedicalRecord"].ToString() == "true";
            string? medicalRecordType = form.ContainsKey("MedicalRecordType") ? form["MedicalRecordType"].ToString() : null;

            if (createMedicalRecord && !string.IsNullOrWhiteSpace(medicalRecordType))
            {
                string? userId = HttpContext.Session.GetString("UserId");
                string firstName = HttpContext.Session.GetString("FirstName") ?? "Staff";
                string lastName = HttpContext.Session.GetString("LastName") ?? "Member";
                string veterinarianName = $"Dr. {firstName} {lastName}";

                DateTime? recordDate = null;
                if (form.ContainsKey("MedicalRecordDate") && DateTime.TryParse(form["MedicalRecordDate"].ToString(), out DateTime parsedDate))
                {
                    recordDate = parsedDate;
                }

                DateTime? nextDueDate = null;
                if (form.ContainsKey("MedicalRecordNextDueDate") && DateTime.TryParse(form["MedicalRecordNextDueDate"].ToString(), out DateTime parsedNextDue))
                {
                    nextDueDate = parsedNextDue;
                }

                decimal? cost = null;
                if (form.ContainsKey("MedicalRecordCost") && decimal.TryParse(form["MedicalRecordCost"].ToString(), out decimal parsedCost))
                {
                    cost = parsedCost;
                }

                MedicalRecordViewModel medicalRecord = new MedicalRecordViewModel
                {
                    PetId = model.PetId,
                    PetName = model.Name,
                    RecordType = medicalRecordType ?? "",
                    RecordDate = recordDate ?? DateTime.UtcNow,
                    VeterinarianId = userId ?? "",
                    VeterinarianName = veterinarianName,
                    Description = form.ContainsKey("MedicalRecordDescription") ? form["MedicalRecordDescription"].ToString() ?? "" : "",
                    VaccineName = form.ContainsKey("MedicalRecordVaccineName") ? form["MedicalRecordVaccineName"].ToString() ?? "" : "",
                    NextDueDate = nextDueDate,
                    Cost = cost ?? 0,
                    Notes = form.ContainsKey("MedicalRecordNotes") ? form["MedicalRecordNotes"].ToString() ?? "" : ""
                };

                MedicalRecordViewModel? createdRecord = await _medicalRecordApiService.CreateMedicalRecordAsync(medicalRecord);
                if (createdRecord != null)
                {
                    TempData["Success"] = $"Pet '{model.Name}' and medical record updated successfully!";
                }
                else
                {
                    TempData["Success"] = $"Pet '{model.Name}' updated successfully, but failed to create medical record.";
                }
            }
            else
            {
                TempData["Success"] = $"Pet '{model.Name}' updated successfully!";
            }

            return RedirectToAction(nameof(Details), new { id = model.PetId });
        }

        // POST: /Pets/UpdateMedicalRecord
        [HttpPost]
        [SessionAuthorize("Staff", "Admin")]
        public async Task<IActionResult> UpdateMedicalRecord([FromBody] UpdateMedicalRecordRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.RecordId) || request.Model == null)
            {
                return Json(new { success = false, message = "Invalid request data." });
            }

            bool success = await _medicalRecordApiService.UpdateMedicalRecordAsync(request.RecordId, request.Model);

            if (!success)
            {
                return Json(new { success = false, message = "Failed to update medical record. Please try again." });
            }

            return Json(new { success = true, message = "Medical record updated successfully!" });
        }

        // POST: /Pets/DeleteMedicalRecord
        [HttpPost]
        [SessionAuthorize("Staff", "Admin")]
        public async Task<IActionResult> DeleteMedicalRecord([FromBody] DeleteMedicalRecordRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.RecordId))
            {
                return Json(new { success = false, message = "Medical Record id is required." });
            }

            bool success = await _medicalRecordApiService.DeleteMedicalRecordAsync(request.RecordId);

            if (!success)
            {
                return Json(new { success = false, message = "Failed to delete medical record. Please try again." });
            }

            return Json(new { success = true, message = "Medical record deleted successfully!" });
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

    public class UpdateMedicalRecordRequest
    {
        public string RecordId { get; set; } = string.Empty;
        public MedicalRecordViewModel Model { get; set; } = new();
    }

    public class DeleteMedicalRecordRequest
    {
        public string RecordId { get; set; } = string.Empty;
    }
}
