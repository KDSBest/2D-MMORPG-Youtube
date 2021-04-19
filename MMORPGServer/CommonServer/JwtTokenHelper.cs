using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using CommonServer.Configuration;

namespace CommonServer
{
    public static class JwtTokenHelper
    {
        public static string GenerateToken(List<Claim> claims, int expireHours = 48)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var now = DateTime.UtcNow;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = now.AddHours(expireHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(UTF7Encoding.UTF8.GetBytes(SecurityConfiguration.JwtSecret)), SecurityAlgorithms.HmacSha256Signature)
            };

            var stoken = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
            return tokenHandler.WriteToken(stoken);
        }

        public static bool ValidateToken(string token)
		{
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var stoken = tokenHandler.ReadJwtToken(token);

                var now = DateTime.UtcNow;
                if (now >= stoken.ValidFrom && now <= stoken.ValidTo)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetTokenClaim(string token, string claimType)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var stoken = tokenHandler.ReadJwtToken(token);

                var now = DateTime.UtcNow;
                if (now >= stoken.ValidFrom && now <= stoken.ValidTo)
                {
                    return stoken.Claims.First(x => x.Type == claimType).Value;
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
