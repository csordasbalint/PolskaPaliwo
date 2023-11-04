using Microsoft.AspNetCore.Mvc;
using PolskaPaliwo.Models;
using PolskaPaliwo.Repository;
using System.Diagnostics;

namespace PolskaPaliwo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICarAdRepository _carAdRepository;

        public HomeController(ILogger<HomeController> logger, ICarAdRepository ICarAdRepository)
        {
            _logger = logger;
            _carAdRepository = ICarAdRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var carAds = _carAdRepository.GetAllCarAds();
            return View(carAds);
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