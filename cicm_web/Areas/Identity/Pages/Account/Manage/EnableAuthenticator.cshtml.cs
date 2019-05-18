/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : EnableAuthenticator.cshtml.cs
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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace cicm_web.Areas.Identity.Pages.Account.Manage
{
    public class EnableAuthenticatorModel : PageModel
    {
        const string AuthenticatorUriFormat =
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        readonly ILogger<EnableAuthenticatorModel> _logger;
        readonly UrlEncoder                        _urlEncoder;
        readonly UserManager<IdentityUser>         _userManager;

        public EnableAuthenticatorModel(UserManager<IdentityUser> userManager, ILogger<EnableAuthenticatorModel> logger,
                                        UrlEncoder                urlEncoder)
        {
            _userManager = userManager;
            _logger      = logger;
            _urlEncoder  = urlEncoder;
        }

        public string SharedKey { get; set; }

        public string AuthenticatorUri { get; set; }

        [TempData]
        public string[] RecoveryCodes { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            await LoadSharedKeyAndQrCodeUriAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if(!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            // Strip spaces and hypens
            string verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            bool is2faTokenValid =
                await _userManager.VerifyTwoFactorTokenAsync(user,
                                                             _userManager.Options.Tokens.AuthenticatorTokenProvider,
                                                             verificationCode);

            if(!is2faTokenValid)
            {
                ModelState.AddModelError("Input.Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            string userId = await _userManager.GetUserIdAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            StatusMessage = "Your authenticator app has been verified.";

            if(await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                IEnumerable<string> recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                RecoveryCodes = recoveryCodes.ToArray();
                return RedirectToPage("./ShowRecoveryCodes");
            }

            return RedirectToPage("./TwoFactorAuthentication");
        }

        async Task LoadSharedKeyAndQrCodeUriAsync(IdentityUser user)
        {
            // Load the authenticator key & QR code URI to display on the form
            string unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if(string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            SharedKey = FormatKey(unformattedKey);

            string email = await _userManager.GetEmailAsync(user);
            AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
        }

        string FormatKey(string unformattedKey)
        {
            StringBuilder result          = new StringBuilder();
            int           currentPosition = 0;
            while(currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }

            if(currentPosition < unformattedKey.Length) result.Append(unformattedKey.Substring(currentPosition));

            return result.ToString().ToLowerInvariant();
        }

        string GenerateQrCodeUri(string email, string unformattedKey) =>
            string.Format(AuthenticatorUriFormat, _urlEncoder.Encode("cicm_web"), _urlEncoder.Encode(email),
                          unformattedKey);

        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                MinimumLength             = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Verification Code")]
            public string Code { get; set; }
        }
    }
}