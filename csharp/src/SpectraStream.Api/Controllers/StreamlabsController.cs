using Microsoft.AspNetCore.Mvc;

namespace SpectraStream.Api.Controllers
{
    [ApiController]
    [Route("mock/streamlabs")]
    public class StreamlabsController : ControllerBase
    {
        [HttpGet("donations")]
        public IActionResult GetDonations()
        {
            // Pretend this came from Streamlabs
            var mockResponse = new
            {
                donations = new[]
                {
                    new {
                        id = 1,
                        donor = "Viewer123",
                        amount = 5.00,
                        message = "QST-7F3B Great stream!" // the special string
                    },
                    new {
                        id = 2,
                        donor = "AnotherFan",
                        amount = 10.00,
                        message = "QST-9X1A Keep it up!"
                    }
                }
            };

            return Ok(mockResponse);
        }
    }
}
