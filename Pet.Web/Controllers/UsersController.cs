using Microsoft.AspNetCore.Mvc;
using Pet.Web.Attributes;
using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;

namespace Pet.Web.Controllers
{
    [SessionAuthorize("Admin")]
    public class UsersController : Controller
    {
        private readonly IAuthApiService _authApiService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IAuthApiService authApiService, ILogger<UsersController> logger)
        {
            _authApiService = authApiService;
            _logger = logger;
        }

        // GET: /Users
        public async Task<IActionResult> Index()
        {
            List<UserViewModel> users = await _authApiService.GetAllUsersAsync();
            return View(users);
        }

        // GET: /Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            List<UserViewModel> users = await _authApiService.GetAllUsersAsync();
            UserViewModel? user = users.FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction(nameof(Index));
            }

            var updateModel = new UpdateUserViewModel
            {
                Role = "" // Empty means keep current role
            };

            ViewBag.User = user;
            return View(updateModel);
        }

        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdateUserViewModel model)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            if (!ModelState.IsValid)
            {
                List<UserViewModel> users = await _authApiService.GetAllUsersAsync();
                UserViewModel? user = users.FirstOrDefault(u => u.UserId == id);
                if (user != null)
                {
                    ViewBag.User = user;
                }
                return View(model);
            }

            UserViewModel? updatedUser = await _authApiService.UpdateUserAsync(id, model);

            if (updatedUser == null)
            {
                TempData["Error"] = "Failed to update user. Please try again.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            TempData["Success"] = $"User {updatedUser.Email} updated successfully.";
            _logger.LogInformation($"User {id} updated by admin");
            return RedirectToAction(nameof(Index));
        }

        // POST: /Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "User ID is required";
                return RedirectToAction(nameof(Index));
            }

            // Prevent admin from deleting themselves
            string? currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            bool deleted = await _authApiService.DeleteUserAsync(id);

            if (deleted)
            {
                TempData["Success"] = "User deleted successfully.";
                _logger.LogInformation($"User {id} deleted by admin");
            }
            else
            {
                TempData["Error"] = "Failed to delete user. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

