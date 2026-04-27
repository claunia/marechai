/*******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ---------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] -----------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ---------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Marechai.Server.Services;

public sealed class TokenService(IConfiguration configuration)
{
    public string CreateToken(IdentityUser user, IList<string> roles)
    {
        JwtSecurityToken token        = CreateJwtToken(CreateClaims(user, roles), CreateSigningCredentials());
        var              tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }

    JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials) =>
        new(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims, expires: null,
            signingCredentials: credentials);

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
        new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            SecurityAlgorithms.HmacSha256);
}