using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace PolskaPaliwo.Models
{
    public class CarAd
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string? ContentType { get; set; }
        public byte[]? Data { get; set; }

        public int? Price { get; set; }
        public string Currency { get; set; }
        public string Condition { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int? ProductionYear { get; set; }
        public int? Mileage { get; set; }
        public int? Power { get; set; }
        public int? EngineSize { get; set; }
        public string FuelType { get; set; }
        public string Drive { get; set; }
        public string Transmission { get; set; }
        public string Type { get; set; }
        public int? DoorsNumber { get; set; }
        public string Colour { get; set; }
        public string OriginCountry { get; set; }
        public string FirstOwner { get; set; }
        public int? RegistrationYear { get; set; }
        public string Location { get; set; }
        public string[]? Features { get; set; }
        public string CreatorId { get; set; }
    }
}
