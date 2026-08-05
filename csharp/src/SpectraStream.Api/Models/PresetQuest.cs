using System.Text.Json.Serialization;

namespace SpectraStream.Api.Models
{
    /// <summary>
    /// A purchasable catalog entry (a single quest or a package). Immutable template.
    /// References objectives by ID so the same objective can be reused across quests.
    /// Named "PresetQuest" to avoid clashing with the legacy counter-based Quest model.
    /// </summary>
    public class PresetQuest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>The token a viewer pastes into their Ko-fi message, e.g. "[SS:2WK]".</summary>
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Display only. This app handles no money.</summary>
        [JsonPropertyName("price_cents")]
        public int PriceCents { get; set; }

        [JsonPropertyName("objective_ids")]
        public List<string> ObjectiveIds { get; set; } = new();
    }
}