using SpectraStream.Api.Models;

namespace SpectraStream.Api.Services
{
    /// <summary>
    /// Holds the live overlay queue of purchased quests. Mutable runtime state,
    /// guarded by a lock (matching the existing QuestService concurrency style).
    /// Depends on the catalog to resolve objective IDs into a snapshot at enqueue
    /// time. Does not broadcast — the controller orchestrates SignalR after a
    /// successful add/remove (composition point stays in the controller).
    /// </summary>
    public class QuestQueueService : IQuestQueueService
    {
        private readonly object _lock = new();
        private readonly List<QueuedQuest> _queue = new();
        private readonly IQuestCatalogService _catalog;

        public QuestQueueService(IQuestCatalogService catalog)
        {
            _catalog = catalog;
        }

        public QueuedQuest Enqueue(PresetQuest quest, string supporter)
        {
            // Resolve outside the lock — catalog is read-only and self-synchronized.
            var objectives = _catalog.ResolveObjectives(quest);

            var instance = new QueuedQuest
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                QuestId = quest.Id,
                Title = quest.Title,
                Supporter = supporter,
                Objectives = objectives,
                CreatedAt = DateTimeOffset.UtcNow
            };

            lock (_lock)
            {
                _queue.Add(instance);
            }

            return instance;
        }

        public bool Remove(string instanceId)
        {
            lock (_lock)
            {
                var idx = _queue.FindIndex(q => q.InstanceId == instanceId);
                if (idx < 0) return false;
                _queue.RemoveAt(idx);
                return true;
            }
        }

        public List<QueuedQuest> ListQueue()
        {
            lock (_lock)
            {
                // Return a copy so callers can't mutate internal state or trip the
                // enumerator if the queue changes mid-iteration. Oldest first (insertion order).
                return new List<QueuedQuest>(_queue);
            }
        }
    }
}