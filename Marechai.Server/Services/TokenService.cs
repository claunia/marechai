using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Marechai.Server.Services;

public sealed class TokenService
{
    public string CreateToken(IdentityUser user, IList<string> roles)
    {
        JwtSecurityToken token        = CreateJwtToken(CreateClaims(user, roles), CreateSigningCredentials());
        var              tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }

    JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials) =>
        new("apiWithAuthBackend", "apiWithAuthBackend", claims, expires: null, signingCredentials: credentials);

    List<Claim> CreateClaims(IdentityUser user, IList<string> roles)
    {
        try
        {
            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, "TokenForTheApiWithAuth"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat,
                    EpochTime.GetIntDate(DateTime.UtcNow).ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Sid, user.Id), new(ClaimTypes.Name, user.UserName), new(ClaimTypes.Email, user.Email)
            ];

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            return claims;
        }
        catch(Exception e)
        {
            Console.WriteLine(e);

            throw;
        }
    }

    SigningCredentials CreateSigningCredentials() =>
        new(new SymmetricSecurityKey("!SomethingSecret!!SomethingSecret!!SomethingSecret!!SomethingSecret!"u8
                                        .ToArray()),
            SecurityAlgorithms.HmacSha256);
}