using SpectraStream.Api.Models;

namespace SpectraStream.Api.Services
{
    /// <summary>
    /// Read-only access to the quest catalog (loaded from quest-catalog.json):
    /// preset quests, their shared objectives, storefront projections, and
    /// matching a Ko-fi message to a preset.
    /// </summary>
    public interface IQuestCatalogService
    {
        /// <summary>All preset quests.</summary>
        IReadOnlyCollection<PresetQuest> GetQuests();

        /// <summary>A single preset by id, or null if not found.</summary>
        PresetQuest? GetQuest(string id);

        /// <summary>A single objective by id, or null if not found.</summary>
        Objective? GetObjective(string id);

        /// <summary>
        /// Storefront projection: every preset with its objective IDs resolved
        /// to full objectives. This is what the public storefront reads.
        /// </summary>
        IReadOnlyCollection<StorefrontQuest> GetStorefront();

        /// <summary>
        /// Resolve a preset's objective IDs to full objectives.
        /// Unknown IDs are skipped. Used when enqueuing a purchased quest.
        /// </summary>
        List<Objective> ResolveObjectives(PresetQuest quest);

        /// <summary>
        /// Match a Ko-fi message to a preset — token first, then name fallback.
        /// Returns a miss if nothing matches.
        /// </summary>
        QuestMatchResult MatchMessage(string message);
    }
}