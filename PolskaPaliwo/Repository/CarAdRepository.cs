﻿using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Misc;
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

        public CarAd GetCarAdById(string id)
        {
            var filter = Builders<CarAd>.Filter.Eq(x => x.Id, id);
            return _carAds.Find(filter).FirstOrDefault();
        }

        public void CreateCarAd(CarAd carAd)
        {
            _carAds.InsertOne(carAd);
        }

        public void UpdateCarAd(CarAd carAd)
        {
            _carAds.ReplaceOne(c => c.Id == carAd.Id, carAd);
        }

        public void DeleteCarAd(string id)
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
                        filter &= Builders<CarAd>.Filter.Eq(property.Name, int.Parse(stringValue));
                    }
                    else if (property.PropertyType == typeof(string) && !string.IsNullOrEmpty(value.ToString()))
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
                            filter &= Builders<CarAd>.Filter.Eq(property.Name, stringValue);
                        }  
                    }
                    else if (property.PropertyType == typeof(string[]) && (value as string[])?.Any(s => !string.IsNullOrEmpty(s)) == true)
                    {
                        var features = value as string[];
                        var featuresArray = string.Join(", ", features).Split(",").Select(x => x.Trim()).ToArray();
                        for (int i = 0; i < featuresArray.Length; i++)
                        {
                            filter &= Builders<CarAd>.Filter.Eq(property.Name, featuresArray[i]);
                        }
                    }
                }
            }
            //for testing - printing out the filter
            var filterJson = filter.Render(BsonSerializer.SerializerRegistry.GetSerializer<CarAd>(), BsonSerializer.SerializerRegistry);
            Console.WriteLine(filterJson);


            return _carAds.Find(filter).ToList();
        }




        

        public async Task ListRecommendedCars(string userId, string prevIds)
        {
            var carIds = prevIds.Split(',');

            List<CarAd> carAds = new List<CarAd>();
            foreach (var itemId in carIds)
            {
                CarAd newCarAd = GetCarAdById(itemId);

                carAds.Add(newCarAd);
            }

            Dictionary<string, int> featureCounts = CountFeatureOccurrences(carAds);

            // Display feature counts
            Console.WriteLine("Feature Counts:");
            foreach (var kvp in featureCounts)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value} times");
            }
        }








        private Dictionary<string, int> CountFeatureOccurrences(List<CarAd> carAds)
        {
            Dictionary<string, int> featureCounts = new Dictionary<string, int>();

            foreach (var carAd in carAds)
            {
                // Extract relevant features (Brand, Model, Year...)
                var features = new List<string>
            {
                $"Brand_{carAd.Brand}",
                $"Model_{carAd.Model}",
                $"Condition_{carAd.Condition}",
                $"Year_{carAd.ProductionYear}",
                $"Transmission_{carAd.Transmission}",
                $"Type_{carAd.Type}",
                $"Colour_{carAd.Colour}",
                $"DoorsNumber_{carAd.DoorsNumber}",
                $"FirstOwner_{carAd.FirstOwner}",
            };

                // Count feature occurrences
                foreach (var feature in features)
                {
                    if (featureCounts.ContainsKey(feature))
                    {
                        featureCounts[feature]++;
                    }
                    else
                    {
                        featureCounts[feature] = 1;
                    }
                }
            }

            return featureCounts;
        }









        //private method for extracting important parameters


        //private method for vectorizing and normalizing


        //private method for calculating similarity (between previous searches and full db)


        //ranking, giving back the top X ad

    }
}
