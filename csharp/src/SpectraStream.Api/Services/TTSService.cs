using SpectraStream.Api.Models;
using System.Collections.Concurrent;

namespace SpectraStream.Api.Services
{
    public class TTSService
    {
        private readonly object _lock = new();
        private int _ttsSeq = 0;
        private readonly List<TTSItem> _ttsQueue = new();

        public TTSItem Submit(string text, string voice, string donor, string msg, long amountCents)
        {
            var item = new TTSItem
            {
                Text = text,
                Voice = voice,
                Donor = donor,
                Msg = msg,
                AmountCents = amountCents,
                CreatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Status = "pending"
            };

            lock (_lock)
            {
                _ttsSeq++;
                item.Id = _ttsSeq;
                _ttsQueue.Add(item);
            }

            return item;
        }

        public IEnumerable<TTSItem> ListPending()
        {
            lock (_lock)
            {
                return _ttsQueue.Where(it => it.Status == "pending").ToList();
            }
        }

        public TTSItem? Find(int id)
        {
            lock (_lock)
            {
                return _ttsQueue.FirstOrDefault(it => it.Id == id);
            }
        }

        public List<TTSItem> GetQueue()
        {
            lock (_lock)
            {
                return new List<TTSItem>(_ttsQueue);
            }
        }

        public int GetNextId()
        {
            lock (_lock)
            {
                return _ttsSeq;
            }
        }

        public void SetState(List<TTSItem> queue, int seq)
        {
            lock (_lock)
            {
                _ttsQueue.Clear();
                _ttsQueue.AddRange(queue);
                _ttsSeq = seq;
            }
        }
    }
}