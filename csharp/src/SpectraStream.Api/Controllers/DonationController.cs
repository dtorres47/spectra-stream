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
    }
}