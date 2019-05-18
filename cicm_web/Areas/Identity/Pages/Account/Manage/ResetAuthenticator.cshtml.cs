/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ResetAuthenticator.cshtml.cs
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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace cicm_web.Areas.Identity.Pages.Account.Manage
{
    public class ResetAuthenticatorModel : PageModel
    {
        readonly SignInManager<IdentityUser>      _signInManager;
        readonly ILogger<ResetAuthenticatorModel> _logger;
        readonly UserManager<IdentityUser>        _userManager;

        public ResetAuthenticatorModel(UserManager<IdentityUser>        userManager,
                                       SignInManager<IdentityUser>      signInManager,
                                       ILogger<ResetAuthenticatorModel> logger)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _logger        = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage =
                "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";

            return RedirectToPage("./EnableAuthenticator");
        }
    }
}