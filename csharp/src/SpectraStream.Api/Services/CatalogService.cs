using System.Text.Json;
using SpectraStream.Api.Models;

namespace SpectraStream.Api.Services
{
    public class CatalogService
    {
        private readonly Dictionary<string, Ability> _abilities = new();
        private readonly Dictionary<string, Quest> _quests = new();

        public CatalogService(IWebHostEnvironment env)
        {
            var filePath = Path.Combine(env.WebRootPath, "config", "catalog.json");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("catalog.json not found", filePath);

            using var stream = File.OpenRead(filePath);
            var catalog = JsonSerializer.Deserialize<CatalogFile>(stream) ?? new CatalogFile();

            foreach (var ability in catalog.Abilities)
            {
                _abilities[ability.Id] = ability;
            }

            foreach (var quest in catalog.Quests)
            {
                if (quest.Target <= 0) quest.Target = 1;
                _quests[quest.Id] = quest;
            }
        }

        public IReadOnlyCollection<Ability> GetAbilities() => _abilities.Values;
        public IReadOnlyCollection<Quest> GetQuests() => _quests.Values;

        public Quest? GetQuest(string id)
        {
            return _quests.TryGetValue(id, out var quest) ? quest : null;
        }

        public Ability? GetAbility(string id)
        {
            return _abilities.TryGetValue(id, out var ability) ? ability : null;
        }

        private class CatalogFile
        {
            public List<Ability> Abilities { get; set; } = new();
            public List<Quest> Quests { get; set; } = new();
        }
    }
}