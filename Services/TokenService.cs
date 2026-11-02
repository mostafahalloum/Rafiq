using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Rafiq.Services
    {
    public class TokenService : ITokenService
        {
        public string GenerateAccessToken ( IEnumerable<Claim> claims )
            {
            var secretKey = _configuration["Jwt:Key"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signinCredential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokeOptions = new JwtSecurityToken(
                 issuer: "https://rafiq1.runasp.net",
                 audience: "https://rafiq1.runasp.net",
                 claims: claims,
                 expires: DateTime.UtcNow.AddMinutes(15),
                 signingCredentials: signinCredential
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return tokenString;

            }

        public string GenerateRefreshToken ()
            {
            var randomNumber = new byte [32];
            using ( var rng = RandomNumberGenerator.Create() )
                {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
                }
            }

        public ClaimsPrincipal GetPrincipalFromExpiredToken ( string token )
            {
            var tokenValidationParameters = new TokenValidationParameters
                {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("4BB86D85AE74ABD1D96DB199A2D894BB86D85AE74ABD1D96DB199A2D89")),
                ValidateLifetime = false 
                };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if ( jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase) )
                throw new SecurityTokenException("Invalid token");
            return principal;
            }
        }
    }
