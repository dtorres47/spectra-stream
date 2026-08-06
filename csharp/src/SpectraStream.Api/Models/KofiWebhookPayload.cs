using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpectraStream.Api.Models
{
    /// <summary>
    /// Ko-fi webhook payload. Arrives as a JSON string inside the form field "data"
    /// (content-type application/x-www-form-urlencoded).
    ///
    /// Full contract is modeled so the shape is documented for future use. Fields
    /// marked "reference only" below are not consumed by current logic — they exist
    /// so the payload can be extended later without re-deriving the contract.
    /// </summary>
    public class KofiWebhookPayload
    {
        // ---- Fields the app currently uses ----

        /// <summary>Our shared secret. Verify before trusting the request.</summary>
        [JsonPropertyName("verification_token")]
        public string VerificationToken { get; init; } = string.Empty;

        /// <summary>Tip, Subscription, Commission, Shop Order (test sends "Donation").</summary>
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        /// <summary>Buyer's display name → QueuedQuest.Supporter.</summary>
        [JsonPropertyName("from_name")]
        public string FromName { get; init; } = string.Empty;

        /// <summary>Free-text message; we parse the quest token out of this.</summary>
        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        /// <summary>Ko-fi requires hiding the message when this is false.</summary>
        [JsonPropertyName("is_public")]
        public bool IsPublic { get; init; }

        #region Reference only: kept to preserve the full payload shape
        /// <summary>Reference only. Unique per webhook; Ko-fi retries with the same id until it gets a 200.</summary>
        [JsonPropertyName("message_id")]
        public string MessageId { get; init; } = string.Empty;

        /// <summary>Reference only.</summary>
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; init; }

        /// <summary>Reference only. Tip amount as a string, e.g. "3.00".</summary>
        [JsonPropertyName("amount")]
        public string Amount { get; init; } = string.Empty;

        /// <summary>Reference only.</summary>
        [JsonPropertyName("url")]
        public string Url { get; init; } = string.Empty;

        /// <summary>Reference only.</summary>
        [JsonPropertyName("email")]
        public string Email { get; init; } = string.Empty;

        /// <summary>Reference only. e.g. "USD".</summary>
        [JsonPropertyName("currency")]
        public string Currency { get; init; } = string.Empty;

        /// <summary>Reference only. True for recurring membership payments.</summary>
        [JsonPropertyName("is_subscription_payment")]
        public bool IsSubscriptionPayment { get; init; }

        /// <summary>Reference only. True only on the first payment of a subscription.</summary>
        [JsonPropertyName("is_first_subscription_payment")]
        public bool IsFirstSubscriptionPayment { get; init; }

        /// <summary>Reference only.</summary>
        [JsonPropertyName("kofi_transaction_id")]
        public string KofiTransactionId { get; init; } = string.Empty;

        /// <summary>
        /// Reference only. Populated for Shop Order payments; otherwise null.
        /// Typed as JsonElement? because the shape was not verified against a live
        /// shop-order payload — inspect via .ValueKind / .EnumerateArray() when needed.
        /// </summary>
        [JsonPropertyName("shop_items")]
        public JsonElement? ShopItems { get; init; }

        /// <summary>Reference only. Membership tier name for subscriptions; otherwise null.</summary>
        [JsonPropertyName("tier_name")]
        public string? TierName { get; init; }

        /// <summary>
        /// Reference only. Populated for shipped Shop Orders; otherwise null.
        /// Typed as JsonElement? — shape not verified against a live payload.
        /// </summary>
        [JsonPropertyName("shipping")]
        public JsonElement? Shipping { get; init; }

        /// <summary>Reference only.</summary>
        [JsonPropertyName("discord_username")]
        public string? DiscordUsername { get; init; }

        /// <summary>Reference only.</summary>
        [JsonPropertyName("discord_userid")]
        public string? DiscordUserId { get; init; }
        #endregion
    }
}