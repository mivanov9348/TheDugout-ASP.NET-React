using System.Security.Claims;

namespace TheDugout.Services.User.Interfaces
{

    public interface IUserContextService
    {
        int? GetUserId(ClaimsPrincipal user);
    }
}
