using System.Text.Json;
using SpectraStream.Api.Models;

namespace SpectraStream.Api.Services
{
    /// <summary>
    /// Loads the quest catalog from wwwroot/config/quest-catalog.json at startup
    /// and serves read-only access. Registered as a singleton, so a throw in this
    /// constructor fails app startup (fail-fast) rather than failing lazily on a
    /// request — the overlay is never left running against an empty catalog.
    /// </summary>
    public class QuestCatalogService : IQuestCatalogService
    {
        private readonly Dictionary<string, PresetQuest> _quests = new();
        private readonly Dictionary<string, Objective> _objectives = new();

        public QuestCatalogService(IWebHostEnvironment env)
        {
            var filePath = Path.Combine(env.WebRootPath, "config", "quest-catalog.json");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("quest-catalog.json not found", filePath);

            using var stream = File.OpenRead(filePath);
            var catalog = JsonSerializer.Deserialize<QuestCatalog>(stream)
                ?? throw new InvalidOperationException("quest-catalog.json deserialized to null.");

            foreach (var objective in catalog.Objectives)
                _objectives[objective.Id] = objective;

            foreach (var quest in catalog.Quests)
                _quests[quest.Id] = quest;

            // Fail fast: a catalog with no quests is a deployment error, not a valid state.
            if (_quests.Count == 0)
                throw new InvalidOperationException("quest-catalog.json contains no quests.");
        }

        public IReadOnlyCollection<PresetQuest> GetQuests() => _quests.Values;

        public PresetQuest? GetQuest(string id) =>
            _quests.TryGetValue(id, out var quest) ? quest : null;

        public Objective? GetObjective(string id) =>
            _objectives.TryGetValue(id, out var objective) ? objective : null;

        public List<Objective> ResolveObjectives(PresetQuest quest)
        {
            var resolved = new List<Objective>(quest.ObjectiveIds.Count);
            foreach (var id in quest.ObjectiveIds)
            {
                if (_objectives.TryGetValue(id, out var objective))
                    resolved.Add(objective);
                // Unknown ids are skipped rather than throwing — a typo'd objective
                // id shouldn't take down a live purchase.
            }
            return resolved;
        }

        public IReadOnlyCollection<StorefrontQuest> GetStorefront()
        {
            var result = new List<StorefrontQuest>(_quests.Count);
            foreach (var quest in _quests.Values)
            {
                result.Add(new StorefrontQuest
                {
                    Id = quest.Id,
                    Token = quest.Token,
                    Title = quest.Title,
                    Description = quest.Description,
                    PriceCents = quest.PriceCents,
                    Objectives = ResolveObjectives(quest)
                });
            }
            return result;
        }

        public QuestMatchResult MatchMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return QuestMatchResult.Miss();

            // First token to appear anywhere in the message wins. Case-insensitive,
            // ordinal (no culture quirks). Token identification only — no title fallback.
            PresetQuest? earliest = null;
            var earliestIndex = int.MaxValue;

            foreach (var quest in _quests.Values)
            {
                if (string.IsNullOrEmpty(quest.Token)) continue;

                var idx = message.IndexOf(quest.Token, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0 && idx < earliestIndex)
                {
                    earliestIndex = idx;
                    earliest = quest;
                }
            }

            return earliest is null
                ? QuestMatchResult.Miss()
                : QuestMatchResult.Hit(earliest, "token");
        }
    }
}