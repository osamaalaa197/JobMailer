using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobMailer.Business.Exceptions;

namespace JobMailer.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var rawUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(rawUserId, out var userId))
        {
            throw new UnauthorizedAppException("Invalid session.");
        }

        return userId;
    }
}
