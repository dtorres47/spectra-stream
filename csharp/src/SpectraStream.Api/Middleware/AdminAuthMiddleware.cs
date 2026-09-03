using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using SpectraStream.Api.Controllers;

namespace SpectraStream.Api.Middleware
{
    /// <summary>
    /// Guards /admin (page + static file) and admin API routes.
    /// Valid cookie passes; anything else gets 404 — never 401,
    /// so unauthenticated callers can't confirm the routes exist.
    /// </summary>
    public class AdminAuthMiddleware
    {
        private static readonly TimeSpan MaxCookieAge = TimeSpan.FromDays(30);

        private readonly RequestDelegate _next;
        private readonly IDataProtector _protector;
        private readonly ILogger<AdminAuthMiddleware> _logger;

        public AdminAuthMiddleware(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<AdminAuthMiddleware> logger)
        {
            _next = next;
            _protector = dataProtectionProvider.CreateProtector(AdminAuthController.ProtectorPurpose);
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!IsGuarded(context.Request.Path) || HasValidCookie(context))
            {
                await _next(context);
                return;
            }

            _logger.LogWarning("Unauthenticated request to guarded path: {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }

        private static bool IsGuarded(PathString path)
        {
            // Login endpoint must stay reachable or nobody can ever log in.
            if (path.StartsWithSegments("/api/admin/login"))
                return false;

            return path.StartsWithSegments("/admin")          // page route + anything under it
                || path.Equals("/admin.html", StringComparison.OrdinalIgnoreCase) // raw static file
                || path.StartsWithSegments("/api/admin")      // future admin APIs born guarded
                || path.StartsWithSegments("/api/queue/remove"); // currently wide open
        }

        private bool HasValidCookie(HttpContext context)
        {
            if (!context.Request.Cookies.TryGetValue(AdminAuthController.CookieName, out var token))
                return false;

            try
            {
                var issuedAt = DateTimeOffset.Parse(
                    _protector.Unprotect(token), null, DateTimeStyles.RoundtripKind);

                return DateTimeOffset.UtcNow - issuedAt <= MaxCookieAge;
            }
            catch
            {
                // Tampered, signed with old keys, or unparseable — treat as no cookie.
                return false;
            }
        }
    }
}