namespace SpectraStream.Api.Models
{
    public class QuestState
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Target { get; set; }
        public int Progress { get; set; }
        public string IconUrl { get; set; } = string.Empty;
        public long PriceCents { get; set; }
    }
}