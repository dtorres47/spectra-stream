using Microsoft.AspNetCore.Mvc;
using SpectraStream.Api.Services;
using SpectraStream.Api.Models;

namespace SpectraStream.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogService _service;

        public CatalogController(CatalogService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetCatalog()
        {
            return Ok(new
            {
                Abilities = _service.GetAbilities(),
                Quests = _service.GetQuests()
            });
        }
    }
}