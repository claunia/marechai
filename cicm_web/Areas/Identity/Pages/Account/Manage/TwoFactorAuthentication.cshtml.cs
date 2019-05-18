/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : TwoFactorAuthentication.cshtml.cs
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
    public class TwoFactorAuthenticationModel : PageModel
    {
        const string AuthenicatorUriFormat =
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}";
        readonly ILogger<TwoFactorAuthenticationModel> _logger;
        readonly SignInManager<IdentityUser>           _signInManager;

        readonly UserManager<IdentityUser> _userManager;

        public TwoFactorAuthenticationModel(UserManager<IdentityUser>             userManager,
                                            SignInManager<IdentityUser>           signInManager,
                                            ILogger<TwoFactorAuthenticationModel> logger)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _logger        = logger;
        }

        public bool HasAuthenticator { get; set; }

        public int RecoveryCodesLeft { get; set; }

        [BindProperty]
        public bool Is2faEnabled { get; set; }

        public bool IsMachineRemembered { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            HasAuthenticator    = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            Is2faEnabled        = await _userManager.GetTwoFactorEnabledAsync(user);
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft   = await _userManager.CountRecoveryCodesAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            await _signInManager.ForgetTwoFactorClientAsync();
            StatusMessage =
                "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
            return RedirectToPage();
        }
    }
}