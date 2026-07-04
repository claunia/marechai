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
using Marechai.Server.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Marechai.Server.Services;

public sealed class TokenService(IConfiguration configuration)
{
    /// <summary>
    ///     Lifetime of short-lived "two-factor pending" tokens. The user has this long after submitting their
    ///     password to deliver a valid second-factor code.
    /// </summary>
    public static readonly TimeSpan Pending2FaLifetime = TimeSpan.FromMinutes(5);

    public string CreateToken(IdentityUser user, IList<string> roles)
    {
        JwtSecurityToken token        = CreateJwtToken(CreateClaims(user, roles), CreateSigningCredentials());
        var              tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }

    JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials) =>
        new(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

    List<Claim> CreateClaims(IdentityUser user, IList<string> roles)
    {
        try
        {
            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id),
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

    /// <summary>
    ///     Issues a short-lived JWT scoped to the "two-factor pending" audience. The token carries only the user id
    ///     and a <c>purpose=2fa-pending</c> claim, has no role claims, and expires after
    ///     <see cref="Pending2FaLifetime" />. The global JwtBearer middleware rejects this audience, so the token
    ///     cannot be used against any normal API endpoint &mdash; it is consumed only by the explicit
    ///     <c>POST /auth/login/two-factor</c> family of endpoints, which call
    ///     <see cref="ValidatePending2FaToken" />.
    /// </summary>
    public string CreatePending2FaToken(string userId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,  userId),
            new(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(DateTime.UtcNow).ToString(CultureInfo.InvariantCulture)),
            new("purpose", "2fa-pending")
        };

        var token = new JwtSecurityToken(configuration["Jwt:Issuer"],
                                         Audiences.Pending2Fa(configuration["Jwt:Audience"]!), claims,
                                         expires: DateTime.UtcNow.Add(Pending2FaLifetime),
                                         signingCredentials: CreateSigningCredentials());

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    ///     Validates a token previously issued by <see cref="CreatePending2FaToken" />. Returns the user id on
    ///     success, or <see langword="null" /> if the token is malformed, expired, signed with the wrong key, or
    ///     missing the expected audience / purpose claim.
    /// </summary>
    public string ValidatePending2FaToken(string token)
    {
        if(string.IsNullOrWhiteSpace(token)) return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();

            var parameters = new TokenValidationParameters
            {
                ClockSkew                = TimeSpan.Zero,
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = configuration["Jwt:Issuer"],
                ValidAudience            = Audiences.Pending2Fa(configuration["Jwt:Audience"]!),
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
            };

            ClaimsPrincipal principal = handler.ValidateToken(token, parameters, out _);

            string purpose = principal.FindFirstValue("purpose");
            if(purpose != "2fa-pending") return null;

            return principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
        catch(Exception)
        {
            return null;
        }
    }
}