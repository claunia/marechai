using Marechai.Email.Resources;
using Marechai.Email.Templates;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Localization;

namespace Marechai.Email.Composers;

/// <summary>
///     High-level helper for sending email-confirmation emails. Centralises the template + subject so that
///     <c>AuthController</c> stays small. Subject and body are localized via
///     <see cref="IStringLocalizer{T}" /> based on the current thread's
///     <see cref="System.Globalization.CultureInfo.CurrentUICulture" /> &mdash; the controller sets that from
///     the request's <c>Accept-Language</c> header before each send.
/// </summary>
public sealed class EmailConfirmationEmailComposer(
    IEmailSender                   emailSender,
    IEmailTemplateRenderer         renderer,
    IStringLocalizer<EmailStrings> localizer)
{
    readonly IEmailSender                   _emailSender = emailSender;
    readonly IEmailTemplateRenderer         _renderer    = renderer;
    readonly IStringLocalizer<EmailStrings> _localizer   = localizer;

    /// <summary>
    ///     Renders <c>Templates/EmailConfirmationEmail.cshtml</c> with the supplied confirmation URL in the
    ///     caller's active culture and dispatches it via <see cref="IEmailSender" />.
    /// </summary>
    public async Task SendConfirmLinkAsync(string toEmail, string confirmUrl)
    {
        string subject = _localizer["EmailConfirmation_Subject"];

        string body = await _renderer.RenderAsync("EmailConfirmationEmail",
                                                  new EmailConfirmationEmailModel
                                                  {
                                                      AppName      = "Marechai",
                                                      ConfirmUrl   = confirmUrl,
                                                      Title        = _localizer["EmailConfirmation_Title"],
                                                      Intro        = _localizer["EmailConfirmation_Intro"],
                                                      ButtonText   = _localizer["EmailConfirmation_ButtonText"],
                                                      LinkFallback = _localizer["EmailConfirmation_LinkFallback"],
                                                      Footer       = _localizer["EmailConfirmation_Footer"]
                                                  });

        await _emailSender.SendEmailAsync(toEmail, subject, body);
    }
}
