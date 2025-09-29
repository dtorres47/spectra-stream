using System.Collections.Concurrent;
using SpectraStream.Api.Models;

namespace SpectraStream.Api.Services
{
    public class RequestService
    {
        private readonly object _lock = new();
        private int _reqSeq = 0;
        private readonly List<RequestItem> _reqQueue = new();
        private readonly ConcurrentDictionary<int, RequestItem> _reqActive = new();

        private static string DigitsOnly(string input)
        {
            return new string(input.Where(char.IsDigit).ToArray());
        }

        private static string MaskPhone(string digits)
        {
            if (string.IsNullOrEmpty(digits)) return "";
            if (digits.Length == 10) return $"***-***-{digits.Substring(6)}";
            if (digits.Length == 11 && digits.StartsWith("1")) return $"1-***-***-{digits.Substring(7)}";

            var last4 = digits.Length > 4 ? digits[^4..] : digits;
            return $"***-{last4}";
        }

        public RequestItem Submit(string board, string phone, string note)
        {
            var item = new RequestItem
            {
                Board = board.Trim(),
                Phone = DigitsOnly(phone),
                MaskedPhone = MaskPhone(DigitsOnly(phone)),
                Note = note.Trim(),
                Status = "pending",
                CreatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            lock (_lock)
            {
                _reqSeq++;
                item.Id = _reqSeq;
                _reqQueue.Add(item);
            }

            return item;
        }

        public IEnumerable<RequestItem> ListPending()
        {
            lock (_lock)
            {
                return _reqQueue.Where(it => it.Status == "pending").ToList();
            }
        }

        public IEnumerable<RequestItem> ListActive()
        {
            return _reqActive.Values.ToList();
        }

        public RequestItem? Approve(int id)
        {
            lock (_lock)
            {
                var item = _reqQueue.FirstOrDefault(it => it.Id == id);
                if (item == null || item.Status != "pending") return null;

                item.Status = "approved";
                _reqActive[id] = item;
                return item;
            }
        }

        public bool Reject(int id)
        {
            lock (_lock)
            {
                var item = _reqQueue.FirstOrDefault(it => it.Id == id);
                if (item == null || item.Status != "pending") return false;

                item.Status = "rejected";
                return true;
            }
        }

        public bool Complete(int id)
        {
            return _reqActive.TryRemove(id, out _);
        }

        // State helpers
        public List<RequestItem> GetPendingRequests()
        {
            lock (_lock)
            {
                return new List<RequestItem>(_reqQueue);
            }
        }

        public List<RequestItem> GetActiveRequests()
        {
            return _reqActive.Values.ToList();
        }

        public int GetNextId()
        {
            lock (_lock)
            {
                return _reqSeq;
            }
        }

        public void SetState(List<RequestItem> pending, List<RequestItem> active, int seq)
        {
            lock (_lock)
            {
                _reqQueue.Clear();
                _reqQueue.AddRange(pending);
                _reqActive.Clear();
                foreach (var it in active)
                {
                    _reqActive[it.Id] = it;
                }
                _reqSeq = seq;
            }
        }
    }
}
