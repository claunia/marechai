using Marechai.Email.Composers;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marechai.Email;

/// <summary>
///     Registers Marechai's email infrastructure: <see cref="SmtpOptions" /> from the configuration's <c>Smtp</c>
///     section, the embedded-resource <see cref="IEmailTemplateRenderer" />, the MailKit-backed
///     <see cref="IEmailSender" /> (with empty-host log-only fallback), the high-level composers, and the
///     <c>AddLocalization</c> services that back <c>IStringLocalizer&lt;EmailStrings&gt;</c> for localized
///     subjects/bodies.
/// </summary>
public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddMarechaiEmail(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<SmtpOptions>(config.GetSection(SmtpOptions.SectionName));

        // Resources/EmailStrings.{,es,de,fr,it,pt-BR}.resx live alongside the EmailStrings marker class
        // (same namespace, same folder), so AddLocalization with the default convention finds them as
        // embedded resources at "Marechai.Email.Resources.EmailStrings". AddLocalization is idempotent
        // &mdash; safe to call from multiple hosts.
        services.AddLocalization();

        services.AddSingleton<IEmailTemplateRenderer, RazorLightEmailTemplateRenderer>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<TwoFactorEmailComposer>();
        services.AddSingleton<PasswordResetEmailComposer>();
        services.AddSingleton<EmailConfirmationEmailComposer>();
        services.AddSingleton<WelcomeEmailComposer>();
        services.AddSingleton<AccountDeletionConfirmationEmailComposer>();

        return services;
    }
}
