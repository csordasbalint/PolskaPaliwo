using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace PolskaPaliwo.Models
{
    public class CarAd
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Display(Name = "Content type")]
        [Required(ErrorMessage = "Uploading an image is required")]
        public string? ContentType { get; set; }
        public byte[]? Data { get; set; }

        [Display(Name = "Price")]
        [Required(ErrorMessage = "Price is required")]
        public string Price { get; set; }

        [Display(Name = "Currency")]
        [Required(ErrorMessage = "Currency is required")]
        public string Currency { get; set; }

        [Display(Name = "Condition")]
        [Required(ErrorMessage = "Condition is required")]
        public string Condition { get; set; }

        [Display(Name = "Brand")]
        [Required(ErrorMessage = "Brand is required")]
        public string Brand { get; set; }

        [Display(Name = "Model")]
        [Required(ErrorMessage = "Model is required")]
        public string Model { get; set; }

        [Display(Name = "Production year")]
        [Required(ErrorMessage = "Production year is required")]
        public string ProductionYear { get; set; }

        [Display(Name = "Mileage")]
        [Required(ErrorMessage = "Mileage is required")]
        public string Mileage { get; set; }

        [Display(Name = "Power")]
        [Required(ErrorMessage = "Power is required")]
        public string Power { get; set; }

        [Display(Name = "Engine displacement")]
        [Required(ErrorMessage = "Engine displacement is required")]
        public string EngineSize { get; set; }

        [Display(Name = "Fuel type")]
        [Required(ErrorMessage = "Fuel type is required")]
        public string FuelType { get; set; }

        [Display(Name = "Drive")]
        [Required(ErrorMessage = "Drive is required")]
        public string Drive { get; set; }

        [Display(Name = "Transmission")]
        [Required(ErrorMessage = "Transmission is required")]
        public string Transmission { get; set; }

        [Display(Name = "Type")]
        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; }

        [Display(Name = "Number of doors")]
        [Required(ErrorMessage = "Number of doors is required")]
        public string DoorsNumber { get; set; }

        [Display(Name = "Colour")]
        [Required(ErrorMessage = "Colour is required")]
        public string Colour { get; set; }

        [Display(Name = "Origin country")]
        [Required(ErrorMessage = "Origin country is required")]
        public string OriginCountry { get; set; }

        [Display(Name = "First owner")]
        [Required(ErrorMessage = "First owner is required")]
        public string FirstOwner { get; set; }

        [Display(Name = "Registration year")]
        [Required(ErrorMessage = "Registration year is required")]
        public string RegistrationYear { get; set; }

        [Display(Name = "Location")]
        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }

        [Display(Name = "Telephone")]
        [Required(ErrorMessage = "Telephone is required")]
        public string Telephone { get; set; }

        [Display(Name = "Features")]
        public string[]? Features { get; set; }
        public string CreatorId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            CarAd other = (CarAd)obj;
            return Id == other.Id; // Compare based on Id
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}
