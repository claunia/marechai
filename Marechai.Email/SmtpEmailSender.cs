using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Marechai.Email;

/// <summary>
///     <see cref="IEmailSender" /> implementation that delivers messages via SMTP using MailKit. When the
///     configured <see cref="SmtpOptions.Host" /> is empty, the rendered HTML body is logged at Information level
///     instead so local development without an SMTP server still completes the higher-level workflow (e.g. 2FA
///     enrollment) and the agent / developer can copy the verification code from the log.
/// </summary>
public sealed class SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    readonly SmtpOptions              _options = options.Value;
    readonly ILogger<SmtpEmailSender> _logger  = logger;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if(string.IsNullOrWhiteSpace(_options.Host))
        {
            _logger.LogInformation("[SMTP not configured] To: {Recipient}\nSubject: {Subject}\nBody:\n{Body}", email,
                                   subject, htmlMessage);

            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body    = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

        try
        {
            using var client = new SmtpClient();

            await client.ConnectAsync(_options.Host, _options.Port,
                                      _options.EnableSsl
                                          ? SecureSocketOptions.StartTls
                                          : SecureSocketOptions.None);

            if(!string.IsNullOrEmpty(_options.Username))
                await client.AuthenticateAsync(_options.Username, _options.Password ?? string.Empty);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient} (subject: {Subject})", email, subject);

            throw;
        }
    }
}
