using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FCG.Catalog.Api.Shared;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(subject, out var userId)
            ? userId
            : null;
    }

    public static string? GetEmail(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(JwtRegisteredClaimNames.Email);
}
