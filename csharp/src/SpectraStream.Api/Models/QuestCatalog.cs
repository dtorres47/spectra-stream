namespace SpectraStream.Api.Models
{
    /// <summary>Root object that quest-catalog.json deserializes into.</summary>
    public class QuestCatalog
    {
        public List<Objective> Objectives { get; set; } = new();
        public List<PresetQuest> Quests { get; set; } = new();
    }
}