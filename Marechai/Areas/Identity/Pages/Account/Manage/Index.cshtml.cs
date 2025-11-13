using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marechai.Areas.Identity.Pages.Account.Manage;

public class IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    : PageModel
{
    public string Username { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    async Task LoadAsync(ApplicationUser user)
    {
        string userName    = await userManager.GetUserNameAsync(user);
        string phoneNumber = await userManager.GetPhoneNumberAsync(user);

        Username = userName;

        Input = new InputModel
        {
            PhoneNumber = phoneNumber
        };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        ApplicationUser user = await userManager.GetUserAsync(User);

        if(user == null) return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

        await LoadAsync(user);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ApplicationUser user = await userManager.GetUserAsync(User);

        if(user == null) return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");

        if(!ModelState.IsValid)
        {
            await LoadAsync(user);

            return Page();
        }

        string phoneNumber = await userManager.GetPhoneNumberAsync(user);

        if(Input.PhoneNumber != phoneNumber)
        {
            IdentityResult setPhoneResult = await userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);

            if(!setPhoneResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set phone number.";

                return RedirectToPage();
            }
        }

        await signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";

        return RedirectToPage();
    }

    public class InputModel
    {
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
    }
}