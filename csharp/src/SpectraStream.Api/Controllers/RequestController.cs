using Microsoft.AspNetCore.Mvc;
using SpectraStream.Api.Models;
using SpectraStream.Api.Services;
using SpectraStream.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace SpectraStream.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly RequestService _service;
        private readonly IHubContext<OverlayHub> _hubContext;

        public RequestController(RequestService service, IHubContext<OverlayHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }

        [HttpGet("submit")]
        public IActionResult Submit([FromQuery] string? board, [FromQuery] string? phone, [FromQuery] string? note)
        {
            if (string.IsNullOrWhiteSpace(board) && string.IsNullOrWhiteSpace(phone))
                return BadRequest("provide at least ?board= or ?phone=");

            var item = _service.Submit(board ?? "", phone ?? "", note ?? "");
            return Ok(item);
        }

        [HttpGet("queue")]
        public IActionResult Queue()
        {
            return Ok(_service.ListPending());
        }

        [HttpGet("active")]
        public IActionResult Active()
        {
            return Ok(_service.ListActive());
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromQuery] int id)
        {
            var item = _service.Approve(id);
            if (item == null) return NotFound("unknown or not pending");

            await _hubContext.Clients.All.SendAsync("ReceiveEvent", new
            {
                Type = "REQUEST_ADD",
                Data = new { id = item.Id, board = item.Board, masked_phone = item.MaskedPhone, note = item.Note }
            });

            return Ok("ok");
        }

        [HttpPost("reject")]
        public IActionResult Reject([FromQuery] int id)
        {
            if (!_service.Reject(id)) return NotFound("unknown or not pending");
            return Ok("ok");
        }

        [HttpPost("complete")]
        public async Task<IActionResult> Complete([FromQuery] int id)
        {
            if (!_service.Complete(id)) return NotFound("unknown active request id");

            await _hubContext.Clients.All.SendAsync("ReceiveEvent", new
            {
                Type = "REQUEST_REMOVE",
                Data = new { id }
            });

            return Ok("ok");
        }
    }
}