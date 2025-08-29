using System.Security.Claims;

namespace TheDugout.Services.User
{

    public interface IUserContextService
    {
        int? GetUserId(ClaimsPrincipal user);
    }
}
