namespace SpectraStream.Api.Services
{
    /// <summary>
    /// Bounded set of recently-seen Ko-fi message_ids, for webhook dedupe.
    /// Ko-fi retries the same message_id until it gets a 200, so we only need
    /// to remember recent ids — this caps memory by evicting the oldest once full.
    /// Thread-safe.
    /// </summary>
    public class SeenMessageTracker
    {
        private readonly object _lock = new();
        private readonly int _capacity;
        private readonly HashSet<string> _set = new();
        private readonly Queue<string> _order = new();

        public SeenMessageTracker(int capacity = 512)
        {
            _capacity = capacity;
        }

        /// <summary>
        /// Records the id. Returns true if it's new (process it), false if we've
        /// already seen it (a retry — skip).
        /// </summary>
        public bool TryAdd(string messageId)
        {
            if (string.IsNullOrEmpty(messageId)) return true; // no id to dedupe on; don't block

            lock (_lock)
            {
                if (!_set.Add(messageId)) return false; // already present

                _order.Enqueue(messageId);

                if (_order.Count > _capacity)
                {
                    var evicted = _order.Dequeue();
                    _set.Remove(evicted);
                }
                return true;
            }
        }
    }
}