using System.Text.Json.Serialization;

namespace SpectraStream.Api.Models
{
    public class QuestCatalog
    {
        [JsonPropertyName("objectives")]
        public List<Objective> Objectives { get; set; } = new();

        [JsonPropertyName("quests")]
        public List<PresetQuest> Quests { get; set; } = new();
    }
}