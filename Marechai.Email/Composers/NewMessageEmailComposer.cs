using System.Globalization;
using Marechai.Email.Resources;
using Marechai.Email.Templates;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Localization;

namespace Marechai.Email.Composers;

/// <summary>
///     High-level helper for sending "you have received a new message" notifications. Centralises the template +
///     subject so that the background <c>MessageNotificationWorker</c> stays small. Subject and body are
///     localized via <see cref="IStringLocalizer{T}" /> based on the current thread's
///     <see cref="CultureInfo.CurrentUICulture" /> &mdash; the worker sets that from the recipient's
///     <c>LastLanguageVisited</c> column before each send.
/// </summary>
public sealed class NewMessageEmailComposer(
    IEmailSender                   emailSender,
    IEmailTemplateRenderer         renderer,
    IStringLocalizer<EmailStrings> localizer)
{
    readonly IEmailSender                   _emailSender = emailSender;
    readonly IEmailTemplateRenderer         _renderer    = renderer;
    readonly IStringLocalizer<EmailStrings> _localizer   = localizer;

    /// <summary>
    ///     Renders <c>Templates/NewMessageEmail.cshtml</c> with the supplied sender + body + URL in the caller's
    ///     active culture and dispatches it via <see cref="IEmailSender" />.
    /// </summary>
    /// <param name="toEmail">Confirmed email address of the recipient.</param>
    /// <param name="senderDisplayName">Display name (or username, or "Marechai" for system messages) of the sender.</param>
    /// <param name="messageBody">Raw plain-text message body, already trimmed/limited if needed.</param>
    /// <param name="messageUrl">Public URL pointing at the conversation page.</param>
    public async Task SendAsync(string toEmail, string senderDisplayName, string messageBody, string messageUrl)
    {
        string subject = string.Format(CultureInfo.CurrentUICulture,
                                       _localizer["NewMessage_Subject"],
                                       senderDisplayName);

        string body = await _renderer.RenderAsync("NewMessageEmail",
                                                  new NewMessageEmailModel
                                                  {
                                                      AppName           = "Marechai",
                                                      SenderDisplayName = senderDisplayName,
                                                      MessageUrl        = messageUrl,
                                                      MessageBody       = messageBody,
                                                      Title             = _localizer["NewMessage_Title"],
                                                      Intro = string.Format(CultureInfo.CurrentUICulture,
                                                                            _localizer["NewMessage_Intro"],
                                                                            senderDisplayName),
                                                      ButtonText   = _localizer["NewMessage_ButtonText"],
                                                      LinkFallback = _localizer["NewMessage_LinkFallback"],
                                                      Footer       = _localizer["NewMessage_Footer"]
                                                  });

        await _emailSender.SendEmailAsync(toEmail, subject, body);
    }
}
