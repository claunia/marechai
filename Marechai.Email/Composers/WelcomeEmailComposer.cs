using System.Globalization;
using Marechai.Email.Resources;
using Marechai.Email.Templates;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

namespace Marechai.Email.Composers;

/// <summary>
///     High-level helper for sending the welcome email dispatched once after a user successfully confirms
///     their email address. Builds the section URLs from <see cref="IConfiguration" />'s
///     <c>Frontend:BaseUrl</c> so bullet links in the rendered body point at the live web app. Subject and
///     body are localized via <see cref="IStringLocalizer{T}" /> based on the current thread's
///     <see cref="CultureInfo.CurrentUICulture" /> &mdash; the controller sets that from the request's
///     <c>Accept-Language</c> header before each send.
/// </summary>
public sealed class WelcomeEmailComposer(
    IEmailSender                   emailSender,
    IEmailTemplateRenderer         renderer,
    IStringLocalizer<EmailStrings> localizer,
    IConfiguration                 configuration)
{
    readonly IEmailSender                   _emailSender   = emailSender;
    readonly IEmailTemplateRenderer         _renderer      = renderer;
    readonly IStringLocalizer<EmailStrings> _localizer     = localizer;
    readonly IConfiguration                 _configuration = configuration;

    /// <summary>
    ///     Renders <c>Templates/WelcomeEmail.cshtml</c> in the caller's active culture and dispatches it via
    ///     <see cref="IEmailSender" />. <paramref name="displayName" /> is interpolated into the localized
    ///     <c>Welcome_Lead</c> string; pass the user's <c>DisplayName</c> if set, otherwise their
    ///     <c>UserName</c>.
    /// </summary>
    public async Task SendAsync(string toEmail, string displayName)
    {
        string baseUrl = (_configuration["Frontend:BaseUrl"] ?? string.Empty).TrimEnd('/');

        string subject = _localizer["Welcome_Subject"];

        string lead = string.Format(CultureInfo.CurrentUICulture, _localizer["Welcome_Lead"], displayName);

        string body = await _renderer.RenderAsync("WelcomeEmail",
                                                  new WelcomeEmailModel
                                                  {
                                                      AppName          = "Marechai",
                                                      DisplayName      = displayName,
                                                      Subject          = subject,
                                                      Title            = _localizer["Welcome_Title"],
                                                      Lead             = lead,
                                                      Intro            = _localizer["Welcome_Intro"],
                                                      ProfileBullet    = _localizer["Welcome_ProfileBullet"],
                                                      CollectionBullet = _localizer["Welcome_CollectionBullet"],
                                                      SoftwareBullet   = _localizer["Welcome_SoftwareBullet"],
                                                      BooksBullet      = _localizer["Welcome_BooksBullet"],
                                                      Closing          = _localizer["Welcome_Closing"],
                                                      Signoff          = _localizer["Welcome_Signoff"],
                                                      Footer           = _localizer["Welcome_Footer"],
                                                      ProfileUrl       = $"{baseUrl}/profile",
                                                      ComputersUrl     = $"{baseUrl}/computers",
                                                      ConsolesUrl      = $"{baseUrl}/consoles",
                                                      SmartphonesUrl   = $"{baseUrl}/smartphones",
                                                      SoftwareUrl      = $"{baseUrl}/software/all",
                                                      BooksUrl         = $"{baseUrl}/books",
                                                      MagazinesUrl     = $"{baseUrl}/magazines"
                                                  });

        await _emailSender.SendEmailAsync(toEmail, subject, body);
    }
}
