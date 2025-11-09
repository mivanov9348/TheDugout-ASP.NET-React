using System.Security.Claims;
using TheDugout.Services.User.Interfaces;

namespace TheDugout.Services.User
{
    public class UserContextService : IUserContextService
    {
        public int? GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? user.FindFirst("sub")?.Value
                              ?? user.FindFirst("id")?.Value;

            return int.TryParse(userIdClaim, out var parsed) ? parsed : null;
        }
    }
}
