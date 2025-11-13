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