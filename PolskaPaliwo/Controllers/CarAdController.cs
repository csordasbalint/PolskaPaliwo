using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MongoDB.Driver;
using PolskaPaliwo.Models;
using PolskaPaliwo.Repository;

namespace PolskaPaliwo.Controllers
{
    public class CarAdController : Controller
    {
        private readonly ICarAdRepository _carAdRepository;

        public CarAdController(ICarAdRepository ICarAdRepository)
        {
            _carAdRepository = ICarAdRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var carAds = _carAdRepository.GetAllCarAds();
            return Ok(carAds); // Return cars as JSON for testing
        }


        [HttpPost]
        public IActionResult SearchForCarAds(CarAd carAd)
        {
            var searchResults = _carAdRepository.SearchForCarAds(carAd).ToList();
            if (searchResults.Count != 0) //better 0 test needed
            {
                return View("SearchResultsView", searchResults);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
