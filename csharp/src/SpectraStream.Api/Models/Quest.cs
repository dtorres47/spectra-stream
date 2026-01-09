using System.Text.Json.Serialization;

namespace SpectraStream.Api.Models
{
    public class Quest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("price_cents")]
        public long PriceCents { get; set; }
        
        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("target")]
        public int Target { get; set; } = 1;
    }
}
