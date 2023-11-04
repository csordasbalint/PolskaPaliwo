using MongoDB.Bson;
using MongoDB.Driver;
using PolskaPaliwo.Models;

namespace PolskaPaliwo.Repository
{
    public class CarAdRepository : ICarAdRepository
    {
        private readonly IMongoCollection<CarAd> _carAds;

        public CarAdRepository(IMongoDatabase database)
        {
            _carAds = database.GetCollection<CarAd>("CarAdvertisements"); //name of the mongodb collection
        }

        public List<CarAd> GetAllCarAds()
        {
            return _carAds.Find(Builders<CarAd>.Filter.Empty).ToList();
        }

        public CarAd GetCarById(string id)
        {
            var filter = Builders<CarAd>.Filter.Eq(x => x.Id, id);
            return _carAds.Find(filter).FirstOrDefault();
        }

        public void CreateCar(CarAd carAd)
        {
            _carAds.InsertOne(carAd);
        }

        public void UpdateCar(string id, CarAd carAd)
        {
            _carAds.ReplaceOne(c => c.Id == id, carAd);
        }

        public void DeleteCar(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<CarAd>.Filter.Eq("Id", objectId);
            _carAds.DeleteOne(filter);
        }

        public List<CarAd> SearchForCarAds(CarAd carAd)
        {
            var filter = Builders<CarAd>.Filter.Empty;

            foreach (var property in typeof(CarAd).GetProperties())
            {
                var value = property.GetValue(carAd);

                if (value != null)
                {
                    if (property.PropertyType == typeof(int?) && value != null)
                    {
                        string stringValue = value.ToString();
                        if (stringValue.Contains('-'))
                        {
                            var rangeValues = stringValue.Split('-');
                            if (rangeValues.Length == 2)
                            {
                                filter &= Builders<CarAd>.Filter.Gte(property.Name, int.Parse(rangeValues[0]));
                                filter &= Builders<CarAd>.Filter.Lte(property.Name, int.Parse(rangeValues[1]));
                            }
                        }
                        else
                        {
                            filter &= Builders<CarAd>.Filter.Eq(property.Name, int.Parse(stringValue));
                        }
                    }
                    else if (property.PropertyType == typeof(string) && !string.IsNullOrEmpty(value.ToString()))
                    {
                        filter &= Builders<CarAd>.Filter.Eq(property.Name, value);
                    }
                }
            }
            return _carAds.Find(filter).ToList();
        }
    }
}
