using System.Text.Json.Serialization;

namespace BlazorDemoApp.Models
{
    public class PublicHoliday
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("localName")]
        public string LocalName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public DateTime ParsedDate => DateTime.TryParse(Date, out var d) ? d : DateTime.MinValue;
    }
}
