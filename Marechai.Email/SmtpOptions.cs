namespace Marechai.Email;

/// <summary>
///     Strongly-typed binding for the <c>Smtp</c> configuration section. Read by
///     <see cref="SmtpEmailSender" /> via <c>IOptions&lt;SmtpOptions&gt;</c>.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    /// <summary>
    ///     SMTP server hostname. When empty or <see langword="null" />, <see cref="SmtpEmailSender" /> falls back
    ///     to logging the rendered email instead of attempting to deliver it &mdash; useful for local development
    ///     without a real SMTP server.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>SMTP server port. Defaults to 587 (submission with STARTTLS).</summary>
    public int Port { get; set; } = 587;

    /// <summary>SMTP username. When empty, the sender does not call <c>AuthenticateAsync</c>.</summary>
    public string? Username { get; set; }

    /// <summary>SMTP password. Read directly from configuration in dev; in prod prefer environment variables.</summary>
    public string? Password { get; set; }

    /// <summary>
    ///     When <see langword="true" /> (default), the sender connects with <c>SecureSocketOptions.StartTls</c>;
    ///     when <see langword="false" />, it connects with <c>SecureSocketOptions.None</c> (e.g. for Mailpit on
    ///     localhost).
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>The "From" address that appears on outgoing messages.</summary>
    public string FromAddress { get; set; } = "no-reply@marechai.local";

    /// <summary>Display name for the "From" address.</summary>
    public string FromName { get; set; } = "Marechai";
}
