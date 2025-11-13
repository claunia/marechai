using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Marechai.Areas.Identity.Pages.Account.Manage;

public class TwoFactorAuthenticationModel
(
    UserManager<ApplicationUser>          userManager,
    SignInManager<ApplicationUser>        signInManager,
    ILogger<TwoFactorAuthenticationModel> logger
) : PageModel
{
    const    string                                AUTHENICATOR_URI_FORMAT = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}";
    readonly ILogger<TwoFactorAuthenticationModel> _logger                 = logger;

    public bool HasAuthenticator { get; set; }

    public int RecoveryCodesLeft { get; set; }

    [BindProperty]
    public bool Is2faEnabled { get; set; }

    public bool IsMachineRemembered { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGet()
    {
        ApplicationUser user = await userManager.GetUserAsync(User);

        if(user == null) return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

        HasAuthenticator    = await userManager.GetAuthenticatorKeyAsync(user) != null;
        Is2faEnabled        = await userManager.GetTwoFactorEnabledAsync(user);
        IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user);
        RecoveryCodesLeft   = await userManager.CountRecoveryCodesAsync(user);

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        ApplicationUser user = await userManager.GetUserAsync(User);

        if(user == null) return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

        await signInManager.ForgetTwoFactorClientAsync();

        StatusMessage =
            "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";

        return RedirectToPage();
    }
}