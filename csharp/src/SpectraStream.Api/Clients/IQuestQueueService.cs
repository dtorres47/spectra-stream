using SpectraStream.Api.Models;

namespace SpectraStream.Api.Services
{
    /// <summary>
    /// The live overlay queue: quests purchased via Ko-fi and awaiting Toby.
    /// Holds mutable runtime state (distinct from the read-only catalog).
    /// Whole quests are added and removed; there is no per-objective state.
    /// </summary>
    public interface IQuestQueueService
    {
        /// <summary>
        /// Add a purchased quest to the queue. Objectives are resolved from the
        /// preset at enqueue time and snapshotted onto the instance. Returns the
        /// created instance (with its generated InstanceId).
        /// </summary>
        QueuedQuest Enqueue(PresetQuest quest, string supporter);

        /// <summary>
        /// Remove a quest from the queue by its instance id. Returns true if one
        /// was removed, false if the id wasn't found (e.g. already cleared).
        /// </summary>
        bool Remove(string instanceId);

        /// <summary>All quests currently in the queue, oldest first.</summary>
        List<QueuedQuest> ListQueue();
    }
}