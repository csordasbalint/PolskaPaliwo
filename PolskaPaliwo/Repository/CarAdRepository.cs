using Amazon.Runtime.Internal.Transform;
using Microsoft.AspNetCore.Identity;
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





        public List<CarAd> ListRecommendedCars(string userId, string prevIds)
        {
            var carIds = prevIds.Split(',');

            List<CarAd> carAds = new List<CarAd>();
            foreach (var itemId in carIds)
            {
                CarAd newCarAd = GetCarAdById(itemId);
                carAds.Add(newCarAd);
            }

            //1. lépés: TF kiszámítása a user kereséseire
            Dictionary<string, double> termFrequency = CalculateTFInCarAds(carAds);

            Console.WriteLine("TF for Features:");
            foreach (var kvp in termFrequency)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }

            //2. lépés: IDF kiszámítása a user kereséseire
            Dictionary<string, double> inverseDocumentFrequency = CalculateIDFForCarAds(carAds);

            //3. lépés: TF és IDF szorzatának kiszámítása a user kereséseire
            Dictionary<string, double> tfIdf = CalculateTF_IDF(termFrequency, inverseDocumentFrequency);

            //tmp lépés: console kiírás teszt miatt
            Console.WriteLine("TF * IDF for Features:");
            foreach (var kvp in tfIdf)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }

            //4. lépés: TF és IDF szorzatának normalizálása a user kereséseire
            Dictionary<string, double> normalizedTFIDF = PerformL2Normalization(tfIdf);

            //Hasonlóság számításához első vektor, a user keresései alapján
            Dictionary<string, double> userSearchVector = normalizedTFIDF; //user kereséseinek vektora


            //tmp lépés: console kiírás teszt miatt
            Console.WriteLine("\nL2 Normalized TF * IDF for Features:");
            foreach (var kvp in normalizedTFIDF)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value}");
            }


            List<CarAd> allCarAds = GetAllCarAds(); //összes hirdetés kigyűjtése db-ből, majd history Id-k törlése
            //for (int i = allCarAds.Count -1; i>=0; i--)
            //{
            //    if (carAds.Any(adToRemove => adToRemove.Id == allCarAds[i].Id))
            //    {
            //        allCarAds.RemoveAt(i);
            //    }
            //}


            Dictionary<string, double> similarityScores = new Dictionary<string, double>();

            //user kereséseit az összes hirdetéssel összehasonlítja
            foreach (var carAd in allCarAds)
            {
                //egy adott autó vektora
                Dictionary<string, double> carAdVector = GetTFIDFVectorForCarAd(carAd);

                //koszinusz hasonlóság kiszámítása - user keresései és adott autó között
                double similarity = CalculateCosineSimilarity(userSearchVector, carAdVector);
                similarityScores.Add(carAd.Id, similarity);

                //tmp kiírás, ennek egy View() lesz a helye
                Console.WriteLine($"Similarity between user search and car ad ID {carAd.Id}: {similarity}");
            }



            //csak az id-k visszatérésnek
            var topXSimilarCarIds = similarityScores
                .OrderByDescending(kvp => kvp.Value)
                .Take(10) //ajanlasok szama
                .Select(kvp => kvp.Key)
                .ToList();

            List<CarAd> recommendedCars = new List<CarAd>();

            foreach (var id in topXSimilarCarIds)
            {
                CarAd currentCarAd = GetCarAdById(id);
                recommendedCars.Add(currentCarAd);
            }
            int relevantRecommendations = 0;

            foreach (CarAd carAd in recommendedCars)
            {
                if (carAds.Contains(carAd))
                {
                    relevantRecommendations++;
                }
            }

            double precision = (double)relevantRecommendations / recommendedCars.Count;
            double recall = (double)relevantRecommendations / carAds.Count;
            double F1Score = 2 * (precision * recall) / (precision + recall);
            Console.WriteLine("=========================================================");
            Console.WriteLine("The PRECISION is: " + Math.Round(precision,2));
            Console.WriteLine("The RECALL is: " + Math.Round(recall,2));
            Console.WriteLine("The F1Score is: " + Math.Round(F1Score,2));
            Console.WriteLine("=========================================================");



            //TOP K PRECISION - RECALL
            int k = 4; // top k darab carad vizsgalata
            int relevantRecommendationsInTopK = 0;

            for (int i = 0; i < k && i < recommendedCars.Count; i++)
            {
                if (carAds.Contains(recommendedCars[i]))
                {
                    relevantRecommendationsInTopK++;
                }
            }

            double precisionAtK = (double)relevantRecommendationsInTopK / k;
            double recallAtK = (double)relevantRecommendationsInTopK / carAds.Count;
            Console.WriteLine("=========================================================");
            Console.WriteLine("The PRECISION in TOP K rec is: " + Math.Round(precisionAtK, 2));
            Console.WriteLine("The RECALL in TOP K rec is: " + Math.Round(recallAtK, 2));
            Console.WriteLine("=========================================================");




            HashSet<string> uniqueRecommendedCarIds = new HashSet<string>(recommendedCars.Select(car => car.Id));
            double coverage = Math.Round((double)uniqueRecommendedCarIds.Count / allCarAds.Count,8);
            Console.WriteLine("=========================================================");
            Console.WriteLine("The COVERAGE is: " + coverage.ToString("F7"));
            Console.WriteLine("=========================================================");

            
            return recommendedCars;
        }



        //Minden featurehöz visszaadja, milyen gyakran szerepel az összes featurehöz viszonyítva (TF)
        private Dictionary<string, double> CalculateTF(Dictionary<string, double> featureCounts)
        {
            Dictionary<string, double> termFrequency = new Dictionary<string, double>();

            double totalFeaturesInDocument = featureCounts.Values.Sum(); //összes feature száma

            foreach (var kvp in featureCounts)
            {
                string feature = kvp.Key;  //feature, amit eppen vizsgalok
                double frequency = kvp.Value; //hanyszor fordult elo

                double tf = (double)frequency / totalFeaturesInDocument;  // TF kiszámítása az adott featurenek -> gyakoriság/összes feature száma
                termFrequency[feature] = tf; //adott kulcs (feature) alapján beállítja valuenak a TF-et
            }

            return termFrequency;
        }


        //Minden featurehöz visszaadja az IDF-et
        private Dictionary<string, double> CalculateIDF(List<Dictionary<string, double>> allFeatureCounts)
        {
            Dictionary<string, double> inverseDocumentFrequency = new Dictionary<string, double>();

            int totalDocuments = allFeatureCounts.Count; //összes előzőleg eltárolt keresés (jelenleg 5)

            foreach (var featureCounts in allFeatureCounts)
            {
                foreach (var kvp in featureCounts)
                {
                    string feature = kvp.Key; //feature, amit eppen vizsgalok

                    //vizsgálat, hogy a feature szerepel-e már az IDFben
                    if (!inverseDocumentFrequency.ContainsKey(feature))
                    {
                        //kiszamolja, hany documentben(carAd) van benne az adott feature -> 5, ha mind az 5 carAdban alfa romeot keresett
                        int documentsContainingFeature = allFeatureCounts.Count(fc => fc.ContainsKey(feature));

                        //kiszamolja az IDF-et (log smoothing kellett, hogy jobban kitunjenek a nagyon azonosak)
                        double idf = Math.Log((double)totalDocuments / (1 + documentsContainingFeature)) + 1;
                        inverseDocumentFrequency[feature] = idf; ////adott kulcs (feature) alapján beállítja valuenak az IDF-et
                    }
                }
            }

            return inverseDocumentFrequency;
        }


        //visszaadja TF és IDF szorzatát adott featurehöz
        private Dictionary<string, double> CalculateTF_IDF(Dictionary<string, double> termFrequency, Dictionary<string, double> inverseDocumentFrequency)
        {
            Dictionary<string, double> tfIdf = new Dictionary<string, double>();

            foreach (var kvp in termFrequency)
            {
                string feature = kvp.Key; //feature, amit eppen vizsgalok
                double tf = kvp.Value;    //adott feature TF-e

                //ellenorzes, hogy a vizsgalt feature benne van-e már az IDF dictionaryben
                if (inverseDocumentFrequency.ContainsKey(feature))
                {
                    double idf = inverseDocumentFrequency[feature]; //idf megkapja az IDF dictionaryből az adott featurenél lévő értéket
                    double tfidf = tf * idf; //formula, kettő osszeszorozva
                    tfIdf[feature] = tfidf;  //új dictionaryben adott kulcsot (feature) használva eltárolja a szorzatot 
                }
            }

            //visszaadja a dictionaryt, ami minden featurenél már a TF * IDF értékét tartalmazza 
            return tfIdf;
        }


        //Normalizálja az értékeket a korábban kiszámolt TF * IDF dictionaryben
        private Dictionary<string, double> PerformL2Normalization(Dictionary<string, double> tfIdf)
        {
            Dictionary<string, double> normalizedTFIDF = new Dictionary<string, double>();

            //összes featurehöz tartozó TF*IDF értékek négyzetének összege
            double sumOfSquares = tfIdf.Values.Sum(value => value * value);
            double l2NormalizationFactor = Math.Sqrt(sumOfSquares); //előző érték gyökét vesszük

            foreach (var kvp in tfIdf)
            {
                string feature = kvp.Key; //feature,amit eppen vizsgalok
                double tfidf = kvp.Value; //adott feature TF*IDF értéke

                
                double normalizedValue = tfidf / l2NormalizationFactor; //adott feature értékének normalizálása 
                normalizedTFIDF[feature] = normalizedValue;             //output dictionaryben eltároljuk az új, normalizált értéket
            }

            return normalizedTFIDF; //az output a már normalizált értékeket tartalmazó dictionary
        }


        //kiszámítja a koszinusz-hasonlóságot két TF-IDF vektor között
        private double CalculateCosineSimilarity(Dictionary<string, double> vector1, Dictionary<string, double> vector2)
        {
            //dotProduct (skaláris szorzat) kiszámítása
            double dotProduct = vector1.Select(kvp => kvp.Value * (vector2.ContainsKey(kvp.Key) ? vector2[kvp.Key] : 0)).Sum();

            //vektorok nagyságának kiszámítása
            double magnitude1 = Math.Sqrt(vector1.Values.Sum(value => value * value));
            double magnitude2 = Math.Sqrt(vector2.Values.Sum(value => value * value));

            //formula a koszinusz hasonlósághoz, vagyis megadja a bezárt szög nagyságát
            double cosineSimilarity = dotProduct / (magnitude1 * magnitude2);

            return cosineSimilarity; //double értékkel tér vissza, ami megmutatja mennyire hasonló a 2 vektor
        }




        //A user korabbi hirdetéseiből kinyeri milyen featureöket tartalmaz, majd kiszámítja rá a TF-et
        private Dictionary<string, double> CalculateTFInCarAds(List<CarAd> carAds) //paraméterként kapja a user korábbi kereséseit
        {
            carAds.RemoveAll(item => item == null);
            Dictionary<string, double> featureCounts = new Dictionary<string, double>();


            foreach (var carAd in carAds)
            {
                //megadott featureök, amiket figyelembe fog venni
                var features = new List<string>
                {
                    $"Brand_{carAd.Brand}",
                    $"Model_{carAd.Model}",
                    $"Currency_{carAd.Currency}",
                    $"Condition_{carAd.Condition}",
                    $"Year_{carAd.ProductionYear}",
                    $"Transmission_{carAd.Transmission}",
                    $"FuelType_{carAd.FuelType}",
                    $"Drive_{carAd.Drive}",
                    $"Type_{carAd.Type}",
                    $"Colour_{carAd.Colour}",
                    $"DoorsNumber_{carAd.DoorsNumber}",
                    $"OriginCountry_{carAd.OriginCountry}",
                    $"Location{carAd.Location}",
                    $"FirstOwner_{carAd.FirstOwner}",
                    $"RegistrationYear_{carAd.RegistrationYear}"
            };

                if (carAd.Features != null)
                {
                    foreach (var feature in carAd.Features)
                    {
                        features.Add($"Feature_{feature}");
                    }
                }


                #region binning
                //tulajdonságok sávosan elosztva

                //ár
                int price = Convert.ToInt32(carAd.Price);
                string priceBin = price switch
                {
                    _ when price <= 800000 => "Price_VeryLow",
                    _ when price <= 3500000 => "Price_Low",
                    _ when price <= 10000000 => "Price_Medium",
                    _ when price <= 20000000 => "Price_High",
                    _ => "Price_VeryHigh"
                };
                features.Add(priceBin);


                //megtett km
                int mileage = Convert.ToInt32(carAd.Mileage);
                string mileageBin = mileage switch
                {
                    _ when mileage <= 15000 => "Mileage_VeryLow",
                    _ when mileage <= 50000 => "Mileage_Low",
                    _ when mileage <= 180000 => "Mileage_Medium",
                    _ when mileage <= 300000 => "Mileage_High",
                    _ => "Mileage_VeryHigh"
                };
                features.Add(mileageBin);


                //loero
                int power = Convert.ToInt32(carAd.Power);
                string powerBin = power switch
                {
                    _ when power <= 45 => "Power_VeryLow",
                    _ when power <= 95 => "Power_Low",
                    _ when power <= 160 => "Power_Medium",
                    _ when power <= 300 => "Power_High",
                    _ => "Power_VeryHigh"
                };
                features.Add(powerBin);


                //ccm
                string engineSizeBin;
                if (carAd.EngineSize == "E")
                {
                    engineSizeBin = "EngineSize_Electric";
                }
                else
                {
                    int engineSize = Convert.ToInt32(carAd.EngineSize);
                    engineSizeBin = engineSize switch
                    {
                        _ when engineSize <= 800 => "EngineSize_VeryLow",
                        _ when engineSize <= 1450 => "EngineSize_Low",
                        _ when engineSize <= 2400 => "EngineSize_Medium",
                        _ when engineSize <= 3800 => "EngineSize_High",
                        _ => "EngineSize_VeryHigh"
                    };
                }
                features.Add(engineSizeBin);
                #endregion binning


                //egy feature előfordulását számolja (1 előfordulás = 1 növelés)
                //foreach (var feature in features)
                //{
                //    if (featureCounts.ContainsKey(feature))
                //    {
                //        featureCounts[feature]++;
                //    }
                //    else
                //    {
                //        featureCounts[feature] = 1;
                //    }
                //}

                //egy feature előfordulását számolja (1 előfordulás = 1 növelés, extrak eseten 0.5)
                foreach (var feature in features)
                {
                    double weight = feature.StartsWith("Feature_") ? 0.5 : 1; //extraknak csak 0.5

                    if (featureCounts.ContainsKey(feature))
                    {
                        featureCounts[feature] += weight; //megfelelo influence mertek
                    }
                    else
                    {
                        featureCounts[feature] = 1;
                    }
                }
            }

            //Egy dictionaryt ad át a CalculateTF()-nek, ami visszaadja, hogy a kinyert featureöknek mennyi a TF-e
            return CalculateTF(featureCounts);
        }


        //IDF a user korábbi kereséseire
        private Dictionary<string, double> CalculateIDFForCarAds(List<CarAd> carAds)
        {
            List<Dictionary<string, double>> allFeatureCounts = new List<Dictionary<string, double>>();

            foreach (var carAd in carAds)
            {
                Dictionary<string, double> featureCounts = new Dictionary<string, double>();
                var features = new List<string>
                {
                    $"Brand_{carAd.Brand}",
                    $"Model_{carAd.Model}",
                    $"Currency_{carAd.Currency}",
                    $"Condition_{carAd.Condition}",
                    $"Year_{carAd.ProductionYear}",
                    $"Transmission_{carAd.Transmission}",
                    $"FuelType_{carAd.FuelType}",
                    $"Drive_{carAd.Drive}",
                    $"Type_{carAd.Type}",
                    $"Colour_{carAd.Colour}",
                    $"DoorsNumber_{carAd.DoorsNumber}",
                    $"OriginCountry_{carAd.OriginCountry}",
                    $"Location{carAd.Location}",
                    $"FirstOwner_{carAd.FirstOwner}",
                    $"RegistrationYear_{carAd.RegistrationYear}"
                };

                if (carAd.Features != null)
                {
                    foreach (var feature in carAd.Features)
                    {
                        features.Add($"Feature_{feature}");
                    }
                }



                #region binning
                //tulajdonságok sávosan elosztva

                //ár
                int price = Convert.ToInt32(carAd.Price);
                string priceBin = price switch
                {
                    _ when price <= 800000 => "Price_VeryLow",
                    _ when price <= 3500000 => "Price_Low",
                    _ when price <= 10000000 => "Price_Medium",
                    _ when price <= 20000000 => "Price_High",
                    _ => "Price_VeryHigh"
                };
                features.Add(priceBin);


                //megtett km
                int mileage = Convert.ToInt32(carAd.Mileage);
                string mileageBin = mileage switch
                {
                    _ when mileage <= 15000 => "Mileage_VeryLow",
                    _ when mileage <= 50000 => "Mileage_Low",
                    _ when mileage <= 180000 => "Mileage_Medium",
                    _ when mileage <= 300000 => "Mileage_High",
                    _ => "Mileage_VeryHigh"
                };
                features.Add(mileageBin);


                //loero
                int power = Convert.ToInt32(carAd.Power);
                string powerBin = power switch
                {
                    _ when power <= 45 => "Power_VeryLow",
                    _ when power <= 95 => "Power_Low",
                    _ when power <= 160 => "Power_Medium",
                    _ when power <= 300 => "Power_High",
                    _ => "Power_VeryHigh"
                };
                features.Add(powerBin);


                //ccm
                string engineSizeBin;
                if (carAd.EngineSize == "E")
                {
                    engineSizeBin = "EngineSize_Electric";
                }
                else
                {
                    int engineSize = Convert.ToInt32(carAd.EngineSize);
                    engineSizeBin = engineSize switch
                    {
                        _ when engineSize <= 800 => "EngineSize_VeryLow",
                        _ when engineSize <= 1450 => "EngineSize_Low",
                        _ when engineSize <= 2400 => "EngineSize_Medium",
                        _ when engineSize <= 3800 => "EngineSize_High",
                        _ => "EngineSize_VeryHigh"
                    };
                }
                features.Add(engineSizeBin);
                #endregion binning

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

                allFeatureCounts.Add(featureCounts);
            }

            return CalculateIDF(allFeatureCounts);
        }


        //adott carAd-ra visszaadja a normalizált értéket
        private Dictionary<string, double> GetTFIDFVectorForCarAd(CarAd carAd)
        {
            Dictionary<string, double> featureCounts = new Dictionary<string, double>();

            //megadott featureöket veszi fiygelembe
            var features = new List<string>
            {
                $"Brand_{carAd.Brand}",
                $"Model_{carAd.Model}",
                $"Currency_{carAd.Currency}",
                $"Condition_{carAd.Condition}",
                $"Year_{carAd.ProductionYear}",
                $"Transmission_{carAd.Transmission}",
                $"FuelType_{carAd.FuelType}",
                $"Drive_{carAd.Drive}",
                $"Type_{carAd.Type}",
                $"Colour_{carAd.Colour}",
                $"DoorsNumber_{carAd.DoorsNumber}",
                $"OriginCountry_{carAd.OriginCountry}",
                $"Location{carAd.Location}",
                $"FirstOwner_{carAd.FirstOwner}",
                $"RegistrationYear_{carAd.RegistrationYear}"
            };

            if (carAd.Features != null)
            {
                foreach (var feature in carAd.Features)
                {
                    features.Add($"Feature_{feature}");
                }
            }

            #region binning
            //tulajdonságok sávosan elosztva

            //ár
            int price = Convert.ToInt32(carAd.Price);
            string priceBin = price switch
            {
                _ when price <= 800000 => "Price_VeryLow",
                _ when price <= 3500000 => "Price_Low",
                _ when price <= 10000000 => "Price_Medium",
                _ when price <= 20000000 => "Price_High",
                _ => "Price_VeryHigh"
            };
            features.Add(priceBin);


            //megtett km
            int mileage = Convert.ToInt32(carAd.Mileage);
            string mileageBin = mileage switch
            {
                _ when mileage <= 15000 => "Mileage_VeryLow",
                _ when mileage <= 50000 => "Mileage_Low",
                _ when mileage <= 180000 => "Mileage_Medium",
                _ when mileage <= 300000 => "Mileage_High",
                _ => "Mileage_VeryHigh"
            };
            features.Add(mileageBin);


            //loero
            int power = Convert.ToInt32(carAd.Power);
            string powerBin = power switch
            {
                _ when power <= 45 => "Power_VeryLow",
                _ when power <= 95 => "Power_Low",
                _ when power <= 160 => "Power_Medium",
                _ when power <= 300 => "Power_High",
                _ => "Power_VeryHigh"
            };
            features.Add(powerBin);


            //ccm
            string engineSizeBin;
            if (carAd.EngineSize == "E")
            {
                engineSizeBin = "EngineSize_Electric";
            }
            else
            {
                int engineSize = Convert.ToInt32(carAd.EngineSize);
                engineSizeBin = engineSize switch
                {
                    _ when engineSize <= 800 => "EngineSize_VeryLow",
                    _ when engineSize <= 1450 => "EngineSize_Low",
                    _ when engineSize <= 2400 => "EngineSize_Medium",
                    _ when engineSize <= 3800 => "EngineSize_High",
                    _ => "EngineSize_VeryHigh"
                };
            }
            features.Add(engineSizeBin);
            #endregion binning

            //foreach (var feature in features)
            //{
            //    if (featureCounts.ContainsKey(feature))
            //    {
            //        featureCounts[feature]++;
            //    }
            //    else
            //    {
            //        featureCounts[feature] = 1;
            //    }
            //}

            //egy feature előfordulását számolja (1 előfordulás = 1 növelés, extrak eseten 0.5)
            foreach (var feature in features)
            {
                double weight = feature.StartsWith("Feature_") ? 0.5 : 1; //extraknak csak 0.5

                if (featureCounts.ContainsKey(feature))
                {
                    featureCounts[feature] += weight; //megfelelo influence mertek
                }
                else
                {
                    featureCounts[feature] = 1;
                }
            }

            Dictionary<string, double> singleCarAdFeatureCount = featureCounts; //featureCounts egyből


            //TF kiszámítása (1.lépés)
            Dictionary<string, double> singleCarAdTF = CalculateTF(featureCounts);

            //IDF kiszámítása (2.lépés)
            Dictionary<string, double> singleCarAdIDF = CalculateIDF(new List<Dictionary<string, double>> { singleCarAdFeatureCount });

            //TF * IDF (3. lépés)
            Dictionary<string, double> singleCarAdTFIDF = CalculateTF_IDF(singleCarAdTF, singleCarAdIDF);

            //normalizálás (4.lépés)
            Dictionary<string, double> normalizedTFIDF = PerformL2Normalization(singleCarAdTFIDF);

            return normalizedTFIDF; //normalizált értékű dictionaryvel tér vissza
        }


        //testing laptop setup with a test commit
    }
}
