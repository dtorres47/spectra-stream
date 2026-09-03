namespace SpectraStream.Api.Configuration
{
    public class AdminOptions
    {
        public const string SectionName = "Admin";

        /// <summary>
        /// Shared key required to log in to /admin. Set the real value via
        /// environment variable (Admin__SharedKey) in deployment — never
        /// commit the real key to source.
        /// </summary>
        public string SharedKey { get; set; } = string.Empty;
    }
}