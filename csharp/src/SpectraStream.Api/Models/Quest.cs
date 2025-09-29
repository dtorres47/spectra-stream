namespace SpectraStream.Api.Models
{
    public class Quest
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public long PriceCents { get; set; }
        public string IconUrl { get; set; } = string.Empty;
        public int Target { get; set; } = 1;
    }
}