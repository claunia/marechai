/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Index.cshtml.cs
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
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace cicm_web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        readonly IEmailSender                _emailSender;
        readonly SignInManager<IdentityUser> _signInManager;
        readonly UserManager<IdentityUser>   _userManager;

        public IndexModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
                          IEmailSender              emailSender)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _emailSender   = emailSender;
        }

        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            string userName    = await _userManager.GetUserNameAsync(user);
            string email       = await _userManager.GetEmailAsync(user);
            string phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel {Email = email, PhoneNumber = phoneNumber};

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if(!ModelState.IsValid) return Page();

            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            string email = await _userManager.GetEmailAsync(user);
            if(Input.Email != email)
            {
                IdentityResult setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if(!setEmailResult.Succeeded)
                {
                    string userId = await _userManager.GetUserIdAsync(user);
                    throw new
                        InvalidOperationException($"Unexpected error occurred setting email for user with ID '{userId}'.");
                }
            }

            string phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if(Input.PhoneNumber != phoneNumber)
            {
                IdentityResult setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if(!setPhoneResult.Succeeded)
                {
                    string userId = await _userManager.GetUserIdAsync(user);
                    throw new
                        InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{userId}'.");
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            if(!ModelState.IsValid) return Page();

            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            string userId      = await _userManager.GetUserIdAsync(user);
            string email       = await _userManager.GetEmailAsync(user);
            string code        = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string callbackUrl = Url.Page("/Account/ConfirmEmail", null, new {userId, code}, Request.Scheme);
            await _emailSender.SendEmailAsync(email, "Confirm your email",
                                              $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }
    }
}