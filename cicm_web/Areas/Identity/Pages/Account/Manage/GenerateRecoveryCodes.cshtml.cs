/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : GenerateRecoveryCodes.cshtml.cs
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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace cicm_web.Areas.Identity.Pages.Account.Manage
{
    public class GenerateRecoveryCodesModel : PageModel
    {
        readonly ILogger<GenerateRecoveryCodesModel> _logger;
        readonly UserManager<IdentityUser>           _userManager;

        public GenerateRecoveryCodesModel(UserManager<IdentityUser>           userManager,
                                          ILogger<GenerateRecoveryCodesModel> logger)
        {
            _userManager = userManager;
            _logger      = logger;
        }

        [TempData]
        public string[] RecoveryCodes { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            bool isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if(isTwoFactorEnabled) return Page();

            string userId = await _userManager.GetUserIdAsync(user);
            throw new
                InvalidOperationException($"Cannot generate recovery codes for user with ID '{userId}' because they do not have 2FA enabled.");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            if(user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            bool   isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            string userId             = await _userManager.GetUserIdAsync(user);
            if(!isTwoFactorEnabled)
                throw new
                    InvalidOperationException($"Cannot generate recovery codes for user with ID '{userId}' as they do not have 2FA enabled.");

            IEnumerable<string> recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            RecoveryCodes = recoveryCodes.ToArray();

            _logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);
            StatusMessage = "You have generated new recovery codes.";
            return RedirectToPage("./ShowRecoveryCodes");
        }
    }
}