/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ExternalLogins.cshtml.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace cicm_web.Areas.Identity.Pages.Account.Manage
{
    public class ExternalLoginsModel : PageModel
    {
        readonly SignInManager<IdentityUser> _signInManager;
        readonly UserManager<IdentityUser>   _userManager;

        public ExternalLoginsModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
        }

        public IList<UserLoginInfo> CurrentLogins { get; set; }

        public IList<AuthenticationScheme> OtherLogins { get; set; }

        public bool ShowRemoveButton { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            CurrentLogins = await _userManager.GetLoginsAsync(user);
            OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                         .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider)).ToList();
            ShowRemoveButton = user.PasswordHash != null || CurrentLogins.Count > 1;
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            IdentityResult result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
            if(!result.Succeeded)
            {
                string userId = await _userManager.GetUserIdAsync(user);
                throw new
                    InvalidOperationException($"Unexpected error occurred removing external login for user with ID '{userId}'.");
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "The external login was removed.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            string redirectUrl = Url.Page("./ExternalLogins", "LinkLoginCallback");
            AuthenticationProperties properties =
                _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl,
                                                                         _userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            ExternalLoginInfo info =
                await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if(info == null)
                throw new
                    InvalidOperationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");

            IdentityResult result = await _userManager.AddLoginAsync(user, info);
            if(!result.Succeeded)
                throw new
                    InvalidOperationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            StatusMessage = "The external login was added.";
            return RedirectToPage();
        }
    }
}