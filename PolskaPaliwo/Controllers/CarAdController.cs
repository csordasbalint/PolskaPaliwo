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
        public async Task<IActionResult> SearchForCarAds(CarAd carAd)
        {
            var searchResults = _carAdRepository.SearchForCarAds(carAd).ToList();
            if (searchResults.Count != 0) //better 0 test needed
            {
                string json = JsonConvert.SerializeObject(searchResults);
                ViewBag.SearchResults = searchResults; //first viewbag
                HttpContext.Session.SetString("SearchResults", json);

                //await Console.Out.WriteLineAsync(json);


                var userId = _userManager.GetUserId(this.User);
                if (userId != null)
                {
                    var user = await _userManager.FindByIdAsync(userId);

                    List<CarAd> recommendedAds = _carAdRepository.ListRecommendedCars(userId, user.PreviousIds);
                    ViewBag.RecommendedAds = recommendedAds; //second viewbag
                    return View("SearchResultsView");
                }


                return View("SearchResultsView");
            }
            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        public async Task<IActionResult> Details(string id, string source)
        {
            var carAd = _carAdRepository.GetCarAdById(id);
            ViewBag.Source = source;

            var userId = _userManager.GetUserId(this.User);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    if (user.PreviousIds == null)
                    {
                        user.PreviousIds += id.ToString();
                    }
                    else if (user.PreviousIds.Count(x => x == ',') == 4)
                    {
                        int idx = user.PreviousIds.IndexOf(',');
                        user.PreviousIds = user.PreviousIds.Substring(idx + 1);
                        user.PreviousIds += "," + id.ToString();
                    }
                    else
                    {
                        user.PreviousIds += "," + id.ToString();
                    }
                    await _userManager.UpdateAsync(user);
                }
            }
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
        public IActionResult Create(CarAd carAd, IFormFile pictureData)
        {
            using (var stream = pictureData.OpenReadStream())
            {
                byte[] buffer = new byte[pictureData.Length];
                stream.Read(buffer, 0, (int)pictureData.Length);

                carAd.Data = buffer;
                carAd.ContentType = pictureData.ContentType;
            }

            carAd.CreatorId = _userManager.GetUserId(this.User);
            _carAdRepository.CreateCarAd(carAd);
            return RedirectToAction("Index", "Home");
        }


        public IActionResult GetImage(string id) 
        {
            var carAd = _carAdRepository.GetCarAdById(id);
            if (carAd.ContentType.Length > 3)
            {
                return new FileContentResult(carAd.Data, carAd.ContentType);
            }
            return BadRequest();   
        }


        [HttpGet]
        public async Task<IActionResult> ResultsForPrevious()
        {
            string json = HttpContext.Session.GetString("SearchResults");
            if (json != null)
            {
                List<CarAd> searchResults = JsonConvert.DeserializeObject<List<CarAd>>(json);
                ViewBag.SearchResults = searchResults;
                var userId = _userManager.GetUserId(this.User);
                if (userId != null)
                {
                    var user = await _userManager.FindByIdAsync(userId);

                    List<CarAd> recommendedAds = _carAdRepository.ListRecommendedCars(userId, user.PreviousIds);
                    ViewBag.RecommendedAds = recommendedAds; //second viewbag
                    return View("SearchResultsView");
                }
                return View("SearchResultsView");

            }
            return View("Index");
        }




















        [HttpGet]
        public async Task<IActionResult> Recommendation()
        {
            var userId = _userManager.GetUserId(this.User);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);

                List<CarAd> recommendedAds = _carAdRepository.ListRecommendedCars(userId, user.PreviousIds);
                return View("SearchResultsView", recommendedAds);
            }

 
            return Index();
        }

    }
}
