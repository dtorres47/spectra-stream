namespace SpectraStream.Api.Models
{
    /// <summary>
    /// A live instance of a purchased quest sitting in the overlay queue.
    /// Distinct from <see cref="PresetQuest"/>: the same preset bought twice
    /// produces two QueuedQuests. Objectives are a resolved snapshot (display only);
    /// Toby removes the whole quest in one action, so there is no per-objective state.
    /// </summary>
    public class QueuedQuest
    {
        /// <summary>Unique per purchase.</summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>Which preset this came from.</summary>
        public string QuestId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        /// <summary>Ko-fi from_name of the buyer.</summary>
        public string Supporter { get; set; } = string.Empty;

        /// <summary>Resolved at enqueue time from the preset's objective IDs.</summary>
        public List<Objective> Objectives { get; set; } = new();

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}