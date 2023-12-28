

using Footballers.Data.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Footballers.DataProcessor.ImportDto
{
    public class ImportTeamsDTO
    {
        [Required]
        [MaxLength(40)]
        [MinLength(3)]
        [RegularExpression(@"[a-zA-Z0-9\s\.-]+")]
        [JsonProperty("Name")]
        public string Name { get; set; }
        [Required]
        [MaxLength(40)]
        [MinLength(2)]
        [JsonProperty("Nationality")]
        public string Nationality { get; set; }
        [Required]
        [JsonProperty("Trophies")]
        public string Trophies { get; set; }

        [JsonProperty("Footballers")]
        public int[] Footballers { get; set; }
    }
}
