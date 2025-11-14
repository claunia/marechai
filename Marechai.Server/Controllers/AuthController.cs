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

using System.Linq;
using System.Threading.Tasks;
using Marechai.Data.Models;
using Marechai.Database.Models;
using Marechai.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Marechai.Server.Controllers;

[ApiController]
[Route("auth")]
public class AuthController
    (UserManager<ApplicationUser> userManager, MarechaiContext context, TokenService tokenService) : ControllerBase
{
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(AuthResponse),
                          StatusCodes.Status200OK,
                          Description =
                              "Authenticates a user with email and password, returning an access token if successful.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [Produces("application/json")]
    [Consumes("application/json")]
    public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);

        ApplicationUser managedUser = await userManager.FindByEmailAsync(request.Email);

        if(managedUser == null) return Unauthorized("Bad credentials");

        bool isPasswordValid = await userManager.CheckPasswordAsync(managedUser, request.Password);

        if(!isPasswordValid) return Unauthorized("Bad credentials");

        ApplicationUser userInDb = context.Users.FirstOrDefault(u => u.Email == request.Email);

        if(userInDb is null) return Unauthorized();

        string accessToken = tokenService.CreateToken(userInDb, await userManager.GetRolesAsync(managedUser));
        await context.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            Message   = "",
            Succeeded = true,
            Token     = accessToken
        });
    }
}