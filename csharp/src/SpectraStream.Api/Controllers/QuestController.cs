using Microsoft.AspNetCore.Mvc;
using SpectraStream.Api.Models;
using SpectraStream.Api.Services;
using SpectraStream.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace SpectraStream.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestController : ControllerBase
    {
        private readonly QuestService _service;
        private readonly CatalogService _catalog;
        private readonly IHubContext<OverlayHub> _hubContext;

        public QuestController(QuestService service, CatalogService catalog, IHubContext<OverlayHub> hubContext)
        {
            _service = service;
            _catalog = catalog;
            _hubContext = hubContext;
        }

        [HttpGet("add")]
        public IActionResult Add([FromQuery] string id)
        {
            var quest = _catalog.GetQuest(id);
            if (quest == null) return NotFound("unknown quest id");

            var qs = _service.UpsertQuest(quest);
            _hubContext.Clients.All.SendAsync("ReceiveEvent", new { Type = "QUEST_UPSERT", Data = qs });
            return Ok("Quest upserted");
        }

        [HttpGet("active")]
        public IActionResult Active()
        {
            return Ok(_service.ListActiveQuests());
        }

        [HttpPost("inc")]
        public async Task<IActionResult> Increment([FromQuery] string id)
        {
            var qs = _service.Increment(id);
            if (qs == null) return NotFound("unknown active quest id");

            await _hubContext.Clients.All.SendAsync("ReceiveEvent", new { Type = "QUEST_UPSERT", Data = qs });
            return Ok("ok");
        }

        [HttpPost("reset")]
        public async Task<IActionResult> Reset([FromQuery] string id)
        {
            var qs = _service.Reset(id);
            if (qs == null) return NotFound("unknown active quest id");

            await _hubContext.Clients.All.SendAsync("ReceiveEvent", new { Type = "QUEST_UPSERT", Data = qs });
            return Ok("ok");
        }

        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromQuery] string id)
        {
            if (!_service.Remove(id)) return NotFound("unknown active quest id");

            await _hubContext.Clients.All.SendAsync("ReceiveEvent", new { Type = "QUEST_REMOVE", Data = new { id } });
            return Ok("ok");
        }
    }
}