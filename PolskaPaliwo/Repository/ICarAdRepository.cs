using PolskaPaliwo.Models;

namespace PolskaPaliwo.Repository
{
    public interface ICarAdRepository
    {
        List<CarAd> GetAllCarAds();
        CarAd GetCarAdById(string id);
        void CreateCarAd(CarAd carAd);
        void UpdateCarAd(string id, CarAd carAd);
        void DeleteCarAd(string id);
        List<CarAd> SearchForCarAds(CarAd carAd);
    }
}
