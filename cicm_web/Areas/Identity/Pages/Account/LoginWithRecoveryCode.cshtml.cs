/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : LoginWithRecoveryCode.cshtml.cs
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
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace cicm_web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWithRecoveryCodeModel : PageModel
    {
        readonly ILogger<LoginWithRecoveryCodeModel> _logger;
        readonly SignInManager<IdentityUser>         _signInManager;

        public LoginWithRecoveryCodeModel(SignInManager<IdentityUser>         signInManager,
                                          ILogger<LoginWithRecoveryCodeModel> logger)
        {
            _signInManager = signInManager;
            _logger        = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            IdentityUser user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if(user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if(!ModelState.IsValid) return Page();

            IdentityUser user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if(user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

            string recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

            SignInResult result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if(result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }

            if(result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }

            _logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
            return Page();
        }

        public class InputModel
        {
            [BindProperty]
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Recovery Code")]
            public string RecoveryCode { get; set; }
        }
    }
}