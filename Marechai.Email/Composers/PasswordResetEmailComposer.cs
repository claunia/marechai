using System.Globalization;
using Marechai.Email.Resources;
using Marechai.Email.Templates;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Localization;

namespace Marechai.Email.Composers;

/// <summary>
///     High-level helper for sending password-reset emails. Centralises the template + subject so that
///     <c>AuthController</c> stays small. Subject and body are localized via
///     <see cref="IStringLocalizer{T}" /> based on the current thread's
///     <see cref="CultureInfo.CurrentUICulture" /> &mdash; the controller sets that from the request's
///     <c>Accept-Language</c> header before each send.
/// </summary>
public sealed class PasswordResetEmailComposer(
    IEmailSender                   emailSender,
    IEmailTemplateRenderer         renderer,
    IStringLocalizer<EmailStrings> localizer)
{
    readonly IEmailSender                   _emailSender = emailSender;
    readonly IEmailTemplateRenderer         _renderer    = renderer;
    readonly IStringLocalizer<EmailStrings> _localizer   = localizer;

    /// <summary>
    ///     Renders <c>Templates/PasswordResetEmail.cshtml</c> with the supplied reset URL in the caller's
    ///     active culture and dispatches it via <see cref="IEmailSender" />.
    /// </summary>
    public async Task SendResetLinkAsync(string toEmail, string resetUrl, int expiresInMinutes = 60)
    {
        string subject = _localizer["PasswordReset_Subject"];

        string body = await _renderer.RenderAsync("PasswordResetEmail",
                                                  new PasswordResetEmailModel
                                                  {
                                                      AppName          = "Marechai",
                                                      ResetUrl         = resetUrl,
                                                      ExpiresInMinutes = expiresInMinutes,
                                                      Title            = _localizer["PasswordReset_Title"],
                                                      Intro            = _localizer["PasswordReset_Intro"],
                                                      ButtonText       = _localizer["PasswordReset_ButtonText"],
                                                      LinkFallback     = _localizer["PasswordReset_LinkFallback"],
                                                      Expiry = string.Format(CultureInfo.CurrentUICulture,
                                                                             _localizer["PasswordReset_Expiry"],
                                                                             expiresInMinutes),
                                                      Footer = _localizer["PasswordReset_Footer"]
                                                  });

        await _emailSender.SendEmailAsync(toEmail, subject, body);
    }
}
