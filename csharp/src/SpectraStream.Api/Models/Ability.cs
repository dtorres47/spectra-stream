namespace SpectraStream.Api.Models
{
    public class Ability
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public long PriceCents { get; set; }
        public string SfxUrl { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int CooldownMs { get; set; }
        public double Volume { get; set; }
    }
}