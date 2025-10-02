using Microsoft.AspNetCore.Mvc;
using SpectraStream.Api.Models;
using SpectraStream.Api.Services;

namespace SpectraStream.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationController : ControllerBase
    {
        private readonly DonationService _service;

        public DonationController(DonationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> RecordDonation([FromBody] Donation donation)
        {
            if (donation == null)
                return BadRequest("Invalid payload");

            await _service.AppendDonationAsync(donation);
            return NoContent();
        }

        [HttpGet("specialstring")]
        public async Task<IActionResult> GetSpecialString()
        {
            var special = await _service.GetSpecialStringAsync();
            return Ok(new { specialString = special ?? "none found" });
        }

        [HttpGet("quest-code")]
        public async Task<IActionResult> GetQuestCode()
        {
            var code = await _service.LookupQuestCodeAsync();
            return Ok(new { specialString = code ?? "none found" });
        }
    }
}