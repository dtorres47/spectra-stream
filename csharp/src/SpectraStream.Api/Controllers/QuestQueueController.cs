using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SpectraStream.Api.Hubs;
using SpectraStream.Api.Services;

namespace SpectraStream.Api.Controllers
{
    /// <summary>
    /// Live queue endpoints for the overlay (list) and Toby's admin (remove).
    /// Controller orchestrates the broadcast after a successful remove.
    /// </summary>
    [ApiController]
    [Route("api/queue")]
    public class QuestQueueController : ControllerBase
    {
        private readonly IQuestQueueService _queue;
        private readonly IHubContext<OverlayHub> _hub;

        public QuestQueueController(IQuestQueueService queue, IHubContext<OverlayHub> hub)
        {
            _queue = queue;
            _hub = hub;
        }

        /// <summary>Current queue, oldest first. Overlay calls this on load to sync.</summary>
        [HttpGet]
        public IActionResult List() => Ok(_queue.ListQueue());

        /// <summary>Remove a completed quest by instance id and notify overlays.</summary>
        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromQuery] string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                return BadRequest("missing instanceId");

            if (!_queue.Remove(instanceId))
                return NotFound();

            await _hub.Clients.All.SendAsync("ReceiveEvent",
                new { Type = "QUEST_REMOVE", Data = new { InstanceId = instanceId } });

            return Ok();
        }
    }
}