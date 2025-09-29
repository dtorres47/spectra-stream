using SpectraStream.Api.Models;
using System.Collections.Concurrent;

namespace SpectraStream.Api.Services
{
    public class QuestService
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, QuestState> _activeQuests = new();

        public QuestState UpsertQuest(Quest quest)
        {
            lock (_lock)
            {
                if (!_activeQuests.TryGetValue(quest.Id, out var qs))
                {
                    qs = new QuestState
                    {
                        Id = quest.Id,
                        Name = quest.Name,
                        Target = quest.Target > 0 ? quest.Target : 1,
                        Progress = 0,
                        IconUrl = quest.IconUrl,
                        PriceCents = quest.PriceCents
                    };
                    _activeQuests[quest.Id] = qs;
                }
                else
                {
                    qs.Name = quest.Name;
                    if (quest.Target > 0) qs.Target = quest.Target;
                    qs.IconUrl = quest.IconUrl;
                    qs.PriceCents = quest.PriceCents;
                    if (qs.Progress > qs.Target) qs.Progress = qs.Target;
                }
                return qs;
            }
        }

        public List<QuestState> ListActiveQuests()
        {
            lock (_lock)
            {
                return _activeQuests.Values.Select(q => new QuestState
                {
                    Id = q.Id,
                    Name = q.Name,
                    Target = q.Target,
                    Progress = q.Progress,
                    IconUrl = q.IconUrl,
                    PriceCents = q.PriceCents
                }).ToList();
            }
        }

        public QuestState? Increment(string id)
        {
            lock (_lock)
            {
                if (_activeQuests.TryGetValue(id, out var qs) && qs.Progress < qs.Target)
                {
                    qs.Progress++;
                    return qs;
                }
                return null;
            }
        }

        public QuestState? Reset(string id)
        {
            lock (_lock)
            {
                if (_activeQuests.TryGetValue(id, out var qs))
                {
                    qs.Progress = 0;
                    return qs;
                }
                return null;
            }
        }

        public bool Remove(string id)
        {
            lock (_lock)
            {
                return _activeQuests.Remove(id);
            }
        }

        public void SetState(List<QuestState> quests)
        {
            lock (_lock)
            {
                _activeQuests.Clear();
                foreach (var q in quests)
                {
                    _activeQuests[q.Id] = new QuestState
                    {
                        Id = q.Id,
                        Name = q.Name,
                        Target = q.Target,
                        Progress = q.Progress,
                        IconUrl = q.IconUrl,
                        PriceCents = q.PriceCents
                    };
                }
            }
        }
    }
}