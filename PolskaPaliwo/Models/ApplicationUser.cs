using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolskaPaliwo.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        private List<string> _searchResults;

        [NotMapped] // Exclude from database mapping
        public List<string> SearchResults
        {
            get => _searchResults ??= new List<string>();
            set => _searchResults = value;
        }

        // Serialized property for database storage
        public string SerializedSearchResults
        {
            get => JsonConvert.SerializeObject(SearchResults);
            set => SearchResults = value != null ? JsonConvert.DeserializeObject<List<string>>(value) : new List<string>();
        }
    }
}
