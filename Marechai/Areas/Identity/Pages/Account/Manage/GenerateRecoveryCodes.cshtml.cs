using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Marechai.Areas.Identity.Pages.Account.Manage
{
    public class GenerateRecoveryCodesModel : PageModel
    {
        readonly ILogger<GenerateRecoveryCodesModel> _logger;
        readonly UserManager<ApplicationUser>        _userManager;

        public GenerateRecoveryCodesModel(UserManager<ApplicationUser> userManager,
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
            ApplicationUser user = await _userManager.GetUserAsync(User);

            if(user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            bool isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            if(!isTwoFactorEnabled)
            {
                string userId = await _userManager.GetUserIdAsync(user);

                throw new
                    InvalidOperationException($"Cannot generate recovery codes for user with ID '{userId}' because they do not have 2FA enabled.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            if(user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            bool   isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            string userId             = await _userManager.GetUserIdAsync(user);

            if(!isTwoFactorEnabled)
            {
                throw new
                    InvalidOperationException($"Cannot generate recovery codes for user with ID '{userId}' as they do not have 2FA enabled.");
            }

            IEnumerable<string> recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            RecoveryCodes = recoveryCodes.ToArray();

            _logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);
            StatusMessage = "You have generated new recovery codes.";

            return RedirectToPage("./ShowRecoveryCodes");
        }
    }
}