using Microsoft.AspNetCore.Mvc;
using SpectraStream.Api.Services;

namespace SpectraStream.Api.Controllers
{
    /// <summary>Public read-only storefront data: quests with objectives resolved.</summary>
    [ApiController]
    [Route("api/storefront")]
    public class StorefrontController : ControllerBase
    {
        private readonly IQuestCatalogService _catalog;

        public StorefrontController(IQuestCatalogService catalog)
        {
            _catalog = catalog;
        }

        [HttpGet]
        public IActionResult Get() => Ok(_catalog.GetStorefront());
    }
}