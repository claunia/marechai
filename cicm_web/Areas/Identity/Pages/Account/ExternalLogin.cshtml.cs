/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ExternalLogin.cshtml.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     ASP.NET Identify management
//
// --[ License ] --------------------------------------------------------------
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
// ----------------------------------------------------------------------------
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace cicm_web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        readonly ILogger<ExternalLoginModel> _logger;
        readonly SignInManager<IdentityUser> _signInManager;
        readonly UserManager<IdentityUser>   _userManager;

        public ExternalLoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
                                  ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager   = userManager;
            _logger        = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult OnGetAsync() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            string redirectUrl = Url.Page("./ExternalLogin", "Callback", new {returnUrl});
            AuthenticationProperties properties =
                _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if(remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl});
            }

            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if(info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl});
            }

            // Sign in the user with this external login provider if the user already has a login.
            SignInResult result =
                await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);
            if(result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name,
                                       info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if(result.IsLockedOut) return RedirectToPage("./Lockout");

            // If the user does not have an account, then ask the user to create an account.
            ReturnUrl     = returnUrl;
            LoginProvider = info.LoginProvider;
            if(info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                Input = new InputModel {Email = info.Principal.FindFirstValue(ClaimTypes.Email)};
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if(info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl});
            }

            if(ModelState.IsValid)
            {
                IdentityUser   user   = new IdentityUser {UserName = Input.Email, Email = Input.Email};
                IdentityResult result = await _userManager.CreateAsync(user);
                if(result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if(result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach(IdentityError error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }

            LoginProvider = info.LoginProvider;
            ReturnUrl     = returnUrl;
            return Page();
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }
    }
}