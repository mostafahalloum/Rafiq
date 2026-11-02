using System.Security.Claims;

namespace Rafiq.Services
    {
    public interface ITokenService
        {
        string GenerateAccessToken ( IEnumerable<Claim> claims );
        string GenerateRefreshToken ();
        ClaimsPrincipal GetPrincipalFromExpiredToken ( string token );
        }
    }
