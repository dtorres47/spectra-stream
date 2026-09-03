using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpectraStream.Api.Configuration;

namespace SpectraStream.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminAuthController : ControllerBase
    {
        public const string CookieName = "ss_admin";
        public const string ProtectorPurpose = "SpectraStream.AdminAuth";

        private readonly AdminOptions _options;
        private readonly IDataProtector _protector;
        private readonly ILogger<AdminAuthController> _logger;

        public AdminAuthController(
            IOptions<AdminOptions> options,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<AdminAuthController> logger)
        {
            _options = options.Value;
            _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromForm] string key)
        {
            // Fails if no key is configured.
            if (string.IsNullOrEmpty(_options.SharedKey))
            {
                _logger.LogWarning("Admin login attempted but no shared key is configured");
                return NotFound();
            }

            var expected = Encoding.UTF8.GetBytes(_options.SharedKey);
            var provided = Encoding.UTF8.GetBytes(key ?? string.Empty);

            if (!CryptographicOperations.FixedTimeEquals(expected, provided))
            {
                _logger.LogWarning("Admin login failed: wrong key");
                return NotFound();
            }

            // Signed cookie value (timestamp), protected by Data Protection.
            var token = _protector.Protect(DateTimeOffset.UtcNow.ToString("O"));

            Response.Cookies.Append(CookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            _logger.LogInformation("Admin login succeeded");
            return Ok();
        }
    }
}