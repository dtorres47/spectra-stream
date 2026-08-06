using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SpectraStream.Api.Configuration;
using SpectraStream.Api.Hubs;
using SpectraStream.Api.Models;
using SpectraStream.Api.Services;

namespace SpectraStream.Api.Controllers
{
    /// <summary>
    /// Receives Ko-fi webhook POSTs. Composition point: verify -> dedupe -> match
    /// -> enqueue -> broadcast. Returns 200 for anything genuinely from Ko-fi
    /// (even if not actionable) so Ko-fi stops retrying; non-200 only for malformed
    /// requests.
    /// </summary>
    [ApiController]
    [Route("api/kofi")]
    public class KofiWebhookController : ControllerBase
    {
        private readonly IQuestCatalogService _catalog;
        private readonly IQuestQueueService _queue;
        private readonly IHubContext<OverlayHub> _hub;
        private readonly SeenMessageTracker _seen;
        private readonly string _expectedToken;
        private readonly ILogger<KofiWebhookController> _logger;

        public KofiWebhookController(
            IQuestCatalogService catalog,
            IQuestQueueService queue,
            IHubContext<OverlayHub> hub,
            SeenMessageTracker seen,
            IOptions<KofiOptions> options,
            ILogger<KofiWebhookController> logger)
        {
            _catalog = catalog;
            _queue = queue;
            _hub = hub;
            _seen = seen;
            _expectedToken = options.Value.VerificationToken;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromForm] string data)
        {
            // 1. Basic shape: Ko-fi sends JSON in a form field named "data".
            if (string.IsNullOrWhiteSpace(data))
                return BadRequest("missing data field");

            KofiWebhookPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<KofiWebhookPayload>(data);
            }
            catch (JsonException)
            {
                return BadRequest("malformed data payload");
            }

            if (payload is null)
                return BadRequest("empty data payload");

            // 2. Verify it's really from Ko-fi. Mismatch -> 200 so Ko-fi doesn't retry.
            if (string.IsNullOrEmpty(_expectedToken) ||
                !string.Equals(payload.VerificationToken, _expectedToken, StringComparison.Ordinal))
            {
                _logger.LogWarning("Ko-fi webhook rejected: verification token mismatch.");
                return Ok();
            }

            string messageId = payload.MessageId;
            string message = payload.Message;

            // 3. Dedupe on message_id (Ko-fi retries the same id until it gets a 200).
            if (!_seen.TryAdd(messageId))
            {
                _logger.LogInformation($"Ko-fi webhook {messageId} already processed; skipping.", messageId);
                return Ok();
            }

            // 4. Match the message text to a preset quest (token-first).
            var match = _catalog.MatchMessage(message);
            if (!match.IsMatch || match.Quest is null)
                return Ok(); // a tip with no quest token — nothing to do

            // 5. Enqueue and 6. broadcast. Supporter hidden if the tip isn't public.
            var supporter = payload.IsPublic ? payload.FromName : "Anonymous";
            var queued = _queue.Enqueue(match.Quest, supporter);

            await _hub.Clients.All.SendAsync("ReceiveEvent",
                new { Type = "QUEST_UPSERT", Data = queued });

            _logger.LogInformation(
                $"Enqueued quest {queued.QuestId} for {supporter} from Ko-fi message {messageId}.",
                queued.QuestId, supporter, messageId);

            return Ok();
        }
    }
}