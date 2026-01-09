using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SpectraStream.Api.Hubs;
using SpectraStream.Api.Services;

namespace SpectraStream.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbilityController : ControllerBase
    {
        private readonly CatalogService _catalog;
        private readonly IHubContext<OverlayHub> _hubContext;

        public AbilityController(CatalogService catalog, IHubContext<OverlayHub> hubContext)
        {
            _catalog = catalog;
            _hubContext = hubContext;
        }

        [HttpGet("trigger")]
        public async Task<IActionResult> Trigger([FromQuery] string id)
        {
            var ability = _catalog.GetAbility(id);
            if (ability == null)
                return NotFound("unknown ability id");

            await _hubContext.Clients.All.SendAsync("ReceiveEvent", new
            {
                Type = "ABILITY_FIRE",
                Data = new
                {
                    id = ability.Id,
                    name = ability.Name,
                    sfx_url = ability.SfxUrl,
                    cooldown_ms = ability.CooldownMs,
                    volume = ability.Volume
                }
            });

            return Ok(new { triggered = ability.Id, name = ability.Name });
        }

        [HttpGet]
        public IActionResult List()
        {
            return Ok(_catalog.GetAbilities());
        }
    }
}
