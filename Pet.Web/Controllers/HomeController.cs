using Microsoft.AspNetCore.Mvc;
using Pet.Web.Models;
using Pet.Web.Models.ViewModels;
using Pet.Web.Services.Interfaces;
using System.Diagnostics;

namespace Pet.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPetApiService _petApiService;

        public HomeController(ILogger<HomeController> logger, IPetApiService petApiService)
        {
            _logger = logger;
            _petApiService = petApiService;
        }

        public async Task<IActionResult> Index()
        {
            var allPets = await _petApiService.GetAllPetsAsync();
           
            return View(allPets);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}