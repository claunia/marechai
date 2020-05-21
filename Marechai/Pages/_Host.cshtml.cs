using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Marechai.Pages
{
    public class HostModel : PageModel
    {
        public void OnGet() => HttpContext.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                                                                   CookieRequestCultureProvider.
                                                                       MakeCookieValue(new
                                                                                           RequestCulture(CultureInfo.CurrentCulture,
                                                                                                          CultureInfo.
                                                                                                              CurrentUICulture)));
    }
}