using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Marechai.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        readonly ILogger<ChangePasswordModel>   _logger;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly UserManager<ApplicationUser>   _userManager;

        public ChangePasswordModel(UserManager<ApplicationUser> userManager,
                                   SignInManager<ApplicationUser> signInManager, ILogger<ChangePasswordModel> logger)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _logger        = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            if(user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            bool hasPassword = await _userManager.HasPasswordAsync(user);

            if(!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }

            ApplicationUser user = await _userManager.GetUserAsync(User);

            if(user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            IdentityResult changePasswordResult =
                await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);

            if(!changePasswordResult.Succeeded)
            {
                foreach(IdentityError error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            return RedirectToPage();
        }

        public class InputModel
        {
            [Required, DataType(DataType.Password), Display(Name = "Current password")]
            public string OldPassword { get; set; }

            [Required,
             StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                          MinimumLength     = 6), DataType(DataType.Password), Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password), Display(Name = "Confirm new password"),
             Compare("NewPassword", ErrorMessage       = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
    }
}