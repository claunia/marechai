using System.Globalization;
using Marechai.Email.Resources;
using Marechai.Email.Templates;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Localization;

namespace Marechai.Email.Composers;

/// <summary>
///     High-level helper for sending 2FA-related emails. Centralises the template + subject so that
///     <c>AuthController</c> stays small. Subject and body are localized via
///     <see cref="IStringLocalizer{T}" /> based on the current thread's
///     <see cref="CultureInfo.CurrentUICulture" /> &mdash; the controller sets that from the request's
///     <c>Accept-Language</c> header before each send.
/// </summary>
public sealed class TwoFactorEmailComposer(
    IEmailSender                  emailSender,
    IEmailTemplateRenderer        renderer,
    IStringLocalizer<EmailStrings> localizer)
{
    readonly IEmailSender                  _emailSender = emailSender;
    readonly IEmailTemplateRenderer        _renderer    = renderer;
    readonly IStringLocalizer<EmailStrings> _localizer   = localizer;

    /// <summary>
    ///     Renders <c>Templates/TwoFactorCodeEmail.cshtml</c> with the supplied 6-digit code in the caller's
    ///     active culture and dispatches it via <see cref="IEmailSender" />.
    /// </summary>
    public async Task SendLoginCodeAsync(string toEmail, string code, int expiresInMinutes = 5)
    {
        string subject = _localizer["TwoFactor_Subject"];

        string body = await _renderer.RenderAsync("TwoFactorCodeEmail",
                                                  new TwoFactorCodeEmailModel
                                                  {
                                                      AppName          = "Marechai",
                                                      Code             = code,
                                                      ExpiresInMinutes = expiresInMinutes,
                                                      Title            = _localizer["TwoFactor_Title"],
                                                      Intro            = _localizer["TwoFactor_Intro"],
                                                      Expiry = string.Format(CultureInfo.CurrentUICulture,
                                                                             _localizer["TwoFactor_Expiry"],
                                                                             expiresInMinutes),
                                                      Footer = _localizer["TwoFactor_Footer"]
                                                  });

        await _emailSender.SendEmailAsync(toEmail, subject, body);
    }
}
