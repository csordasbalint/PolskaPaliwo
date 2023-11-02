using PolskaPaliwo.Models;

namespace PolskaPaliwo.Repository
{
    public interface ICarAdRepository
    {
        List<CarAd> GetAllCarAds();
        CarAd GetCarById(string id);
        void CreateCar(CarAd carAd);
        void UpdateCar(string id, CarAd carAd);
        void DeleteCar(string id);
    }
}
