namespace SpectraStream.Api.Models
{
    /// <summary>
    /// Outcome of parsing a Ko-fi message for a quest. Either a preset was
    /// matched (by token first, then name fallback) or nothing matched.
    /// </summary>
    public class QuestMatchResult
    {
        public bool IsMatch { get; set; }

        /// <summary>The matched preset, or null on a miss.</summary>
        public PresetQuest? Quest { get; set; }

        /// <summary>How it matched — "token" or "name" — for logging/debugging. Empty on a miss.</summary>
        public string MatchedBy { get; set; } = string.Empty;

        public static QuestMatchResult Hit(PresetQuest quest, string matchedBy) =>
            new() { IsMatch = true, Quest = quest, MatchedBy = matchedBy };

        public static QuestMatchResult Miss() => new() { IsMatch = false };
    }
}