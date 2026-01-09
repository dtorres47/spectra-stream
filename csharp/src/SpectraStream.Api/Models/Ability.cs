using System.Text.Json.Serialization;

namespace SpectraStream.Api.Models
{
    public class Ability
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("price_cents")]
        public long PriceCents { get; set; }
        
        [JsonPropertyName("sfx_url")]
        public string SfxUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("icon_url")]
        public string IconUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("cooldown_ms")]
        public int CooldownMs { get; set; }
        
        [JsonPropertyName("volume")]
        public double Volume { get; set; }
    }
}
