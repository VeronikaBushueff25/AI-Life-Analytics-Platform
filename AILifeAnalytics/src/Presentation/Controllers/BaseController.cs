using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AILifeAnalytics.Controllers
{
    [Authorize]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected Guid UserId
        {
            get
            {
                var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var id))
                    throw new UnauthorizedAccessException("Invalid token: UserId not found.");
                return id;
            }
        }

        protected string UserEmail => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        protected string UserRole => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
