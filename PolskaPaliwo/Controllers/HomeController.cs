using Microsoft.AspNetCore.Authorization;
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
        public IActionResult Index(int page = 1)
        {
            int pageSize = 12;
            
            var carAds = _carAdRepository.GetAllCarAds();

            var paginatedCarAds = carAds.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)carAds.Count / pageSize);
            ViewBag.CarAds = paginatedCarAds;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Privacy()
        {
            return View("Privacy");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}