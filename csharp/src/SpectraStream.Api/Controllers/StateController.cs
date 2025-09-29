using Microsoft.AspNetCore.Mvc;
using SpectraStream.Api.Services;
using SpectraStream.Api.Models;
using SpectraStream.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace SpectraStream.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StateController : ControllerBase
    {
        private readonly StateService _stateService;
        private readonly IHubContext<OverlayHub> _hubContext;

        public StateController(StateService stateService, IHubContext<OverlayHub> hubContext)
        {
            _stateService = stateService;
            _hubContext = hubContext;
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] PersistState state)
        {
            await _stateService.SaveStateAsync(state);
            return Ok("ok");
        }

        [HttpPost("rehydrate")]
        public async Task<IActionResult> Rehydrate()
        {
            // Later: broadcast quests, requests, TTS via SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveEvent", new { Type = "STATE_REHYDRATE" });
            return Ok("ok");
        }
    }
}