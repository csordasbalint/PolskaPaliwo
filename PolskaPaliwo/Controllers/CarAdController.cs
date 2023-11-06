using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MongoDB.Driver;
using Newtonsoft.Json;
using PolskaPaliwo.Models;
using PolskaPaliwo.Repository;
using System.Runtime.Serialization.Formatters.Binary;

namespace PolskaPaliwo.Controllers
{
    public class CarAdController : Controller
    {
        private readonly ICarAdRepository _carAdRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CarAdController(ICarAdRepository ICarAdRepository, UserManager<ApplicationUser> userManager)
        {
            _carAdRepository = ICarAdRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var carAds = _carAdRepository.GetAllCarAds();
            return Ok(carAds); // Return cars as JSON for testing
        }


        [HttpGet]
        public IActionResult UpdateToGenerateForm(string id)
        {
            CarAd carAdToUpdate = _carAdRepository.GetCarAdById(id);
            return View("UpdateFormView", carAdToUpdate);
        }

        [HttpPost]
        public IActionResult Update(CarAd carAd)
        {
            string json = HttpContext.Session.GetString("SearchResults");
            if (json != null)
            {
                List<CarAd> searchResults = JsonConvert.DeserializeObject<List<CarAd>>(json);

                int index = searchResults.FindIndex(c => c.Id == carAd.Id);
                if (index != -1)
                {
                    searchResults[index] = carAd;
                }

                HttpContext.Session.SetString("SearchResults", JsonConvert.SerializeObject(searchResults));
                _carAdRepository.UpdateCarAd(carAd);

                return View("SearchResultsView", searchResults);
            }
            return View("Index");
        }



        [HttpPost]
        public IActionResult SearchForCarAds(CarAd carAd)
        {
            var searchResults = _carAdRepository.SearchForCarAds(carAd).ToList();
            if (searchResults.Count != 0) //better 0 test needed
            {
                string json = JsonConvert.SerializeObject(searchResults);
                HttpContext.Session.SetString("SearchResults", json);

                return View("SearchResultsView", searchResults);
            }
            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        public IActionResult Details(string id)
        {
            var carAd = _carAdRepository.GetCarAdById(id);
            return View("DetailedCarAdvertisementView", carAd);
        }



        [HttpGet]
        public IActionResult DeleteToGenerateForm(string id)
        {
            var carAd = _carAdRepository.GetCarAdById(id);
            return View("DeleteComfirmationView", carAd);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            string json = HttpContext.Session.GetString("SearchResults");
            if (json != null)
            {
                List<CarAd> searchResults = JsonConvert.DeserializeObject<List<CarAd>>(json);
                searchResults.Remove(searchResults.Where(x => x.Id == id).First());

                HttpContext.Session.SetString("SearchResults", JsonConvert.SerializeObject(searchResults));
                _carAdRepository.DeleteCarAd(id);

                return View("SearchResultsView", searchResults);
            }
            return RedirectToAction("Index");
        }



        [HttpGet]
        public IActionResult CreateToGenerateForm()
        {
            return View("CreateFormView");
        }


        [HttpPost]
        public IActionResult Create(CarAd carAd)
        {
            carAd.CreatorId = _userManager.GetUserId(this.User);
            _carAdRepository.CreateCarAd(carAd);
            return RedirectToAction("Index", "Home");
        }







        [HttpGet]
        public IActionResult ResultsForPrevious()
        {
            string json = HttpContext.Session.GetString("SearchResults");
            if (json != null)
            {
                List<CarAd> searchResults = JsonConvert.DeserializeObject<List<CarAd>>(json);
                return View("SearchResultsView", searchResults);
            }
            return View("Index");
        }







    }
}
