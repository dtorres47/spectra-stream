using System.Text.Json.Serialization;

namespace SpectraStream.Api.Models
{
    /// <summary>
    /// An atomic, reusable challenge. Objectives are defined once and referenced
    /// by many quests via their <see cref="Id"/>. Display-only: no runtime state.
    /// </summary>
    public class Objective
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}