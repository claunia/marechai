namespace Marechai.Email.Templates;

/// <summary>
///     Model for <c>Templates/NewMessageEmail.cshtml</c>. All user-visible strings are pre-localized by
///     <see cref="Composers.NewMessageEmailComposer" /> using
///     <c>IStringLocalizer&lt;Resources.EmailStrings&gt;</c> so the Razor template stays culture-agnostic.
/// </summary>
public sealed class NewMessageEmailModel
{
    public string AppName { get; set; } = "Marechai";

    /// <summary>Display name (or username, or "Marechai" for system messages) of whoever sent the message.</summary>
    public string SenderDisplayName { get; set; } = string.Empty;

    /// <summary>Public Marechai URL pointing at the conversation page so the recipient can read &amp; reply.</summary>
    public string MessageUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Raw plain-text body of the message, already trimmed if needed. The template is responsible for
    ///     HTML-encoding it and converting newlines to <c>&lt;br&gt;</c>.
    /// </summary>
    public string MessageBody { get; set; } = string.Empty;

    /// <summary>Localized email title (h1).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Localized intro sentence beneath the title (already formatted with the sender's name).</summary>
    public string Intro { get; set; } = string.Empty;

    /// <summary>Localized text for the call-to-action button.</summary>
    public string ButtonText { get; set; } = string.Empty;

    /// <summary>Localized "if the button does not work" preamble for the raw-link fallback.</summary>
    public string LinkFallback { get; set; } = string.Empty;

    /// <summary>Localized footer explaining why the recipient is receiving this email.</summary>
    public string Footer { get; set; } = string.Empty;
}
