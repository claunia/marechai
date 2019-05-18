/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : ForgotPassword.cshtml.cs
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
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace cicm_web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        readonly IEmailSender              _emailSender;
        readonly UserManager<IdentityUser> _userManager;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if(!ModelState.IsValid) return Page();

            IdentityUser user = await _userManager.FindByEmailAsync(Input.Email);
            if(user == null || !await _userManager.IsEmailConfirmedAsync(user))
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToPage("./ForgotPasswordConfirmation");

            // For more information on how to enable account confirmation and password reset please 
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            string code        = await _userManager.GeneratePasswordResetTokenAsync(user);
            string callbackUrl = Url.Page("/Account/ResetPassword", null, new {code}, Request.Scheme);

            await _emailSender.SendEmailAsync(Input.Email, "Reset Password",
                                              $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }
    }
}