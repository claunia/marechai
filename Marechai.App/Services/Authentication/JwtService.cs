using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Marechai.App.Services.Authentication;

public interface IJwtService
{
    IEnumerable<string> GetRoles(string     token);
    string             GetUserId(string    token);
    string             GetUserName(string  token);
    string             GetEmail(string     token);
    bool                IsTokenValid(string token);
}

public sealed class JwtService : IJwtService
{
    /// <inheritdoc />
    public IEnumerable<string> GetRoles(string token)
    {
        if(string.IsNullOrWhiteSpace(token)) return [];

        try
        {
            var              handler  = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <inheritdoc />
    public string GetUserId(string token)
    {
        if(string.IsNullOrWhiteSpace(token)) return null;

        try
        {
            var              handler  = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public string GetUserName(string token)
    {
        if(string.IsNullOrWhiteSpace(token)) return null;

        try
        {
            var              handler  = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public string GetEmail(string token)
    {
        if(string.IsNullOrWhiteSpace(token)) return null;

        try
        {
            var              handler  = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public bool IsTokenValid(string token)
    {
        if(string.IsNullOrWhiteSpace(token)) return false;

        try
        {
            var              handler  = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            // Check if token has expired (if expiration is set)
            if(jwtToken.ValidTo != DateTime.MinValue) return jwtToken.ValidTo > DateTime.UtcNow;

            return true;
        }
        catch
        {
            return false;
        }
    }
}