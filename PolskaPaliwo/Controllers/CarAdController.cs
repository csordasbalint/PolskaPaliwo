using Microsoft.AspNetCore.Mvc;
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
    }
}
