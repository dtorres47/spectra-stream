using Microsoft.AspNetCore.Mvc;
using SpectraStream.Api.Models;
using SpectraStream.Api.Services;
using SpectraStream.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace SpectraStream.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TTSController : ControllerBase
    {
        private readonly TTSService _service;
        private readonly IHubContext<OverlayHub> _hubContext;

        public TTSController(TTSService service, IHubContext<OverlayHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }

        [HttpGet("submit")]
        public IActionResult Submit([FromQuery] string text, [FromQuery] string? voice, [FromQuery] string? donor, [FromQuery] string? msg, [FromQuery] long? amount_cents)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("missing ?text=");

            var item = _service.Submit(text, voice ?? "", donor ?? "", msg ?? "", amount_cents ?? 0);
            return Ok(item);
        }

        [HttpGet("queue")]
        public IActionResult Queue()
        {
            return Ok(_service.ListPending());
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromQuery] int id)
        {
            var item = _service.Find(id);
            if (item == null || item.Status != "pending")
                return NotFound("unknown or not pending");

            item.Status = "approved";

            if (!string.IsNullOrEmpty(item.Donor) || item.AmountCents > 0 || !string.IsNullOrEmpty(item.Msg))
            {
                await _hubContext.Clients.All.SendAsync("ReceiveEvent", new
                {
                    Type = "DONATION",
                    Data = new { donor = item.Donor, amount = item.AmountCents, msg = item.Msg }
                });
            }

            await _hubContext.Clients.All.SendAsync("ReceiveEvent", new
            {
                Type = "TTS_PLAY",
                Data = new { text = item.Text, voice = item.Voice }
            });

            item.Status = "spoken";
            return Ok("ok");
        }

        [HttpPost("reject")]
        public IActionResult Reject([FromQuery] int id)
        {
            var item = _service.Find(id);
            if (item == null || item.Status != "pending")
                return NotFound("unknown or not pending");

            item.Status = "rejected";
            return Ok("ok");
        }
    }
}