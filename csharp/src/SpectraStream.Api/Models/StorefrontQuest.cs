namespace SpectraStream.Api.Models
{
    /// <summary>
    /// A preset quest with its objectives resolved from IDs to full objects,
    /// shaped for the public storefront so the client doesn't resolve references itself.
    /// </summary>
    public class StorefrontQuest
    {
        public string Id { get; set; } = string.Empty;

        /// <summary>The token the viewer copies to paste into their Ko-fi message.</summary>
        public string Token { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PriceCents { get; set; }

        /// <summary>Resolved objectives (title + description), not bare IDs.</summary>
        public List<Objective> Objectives { get; set; } = new();
    }
}