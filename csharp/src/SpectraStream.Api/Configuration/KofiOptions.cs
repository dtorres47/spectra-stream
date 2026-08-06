namespace SpectraStream.Api.Configuration
{
    /// <summary>Bound from the "Kofi" section of configuration.</summary>
    public class KofiOptions
    {
        public const string SectionName = "Kofi";

        /// <summary>
        /// Expected value of the webhook's verification_token. Set the real value
        /// via environment variable (Kofi__VerificationToken) in deployment — never
        /// commit the real token to source.
        /// </summary>
        public string VerificationToken { get; set; } = string.Empty;
    }
}