namespace Marechai.Email.Templates;

/// <summary>
///     Model for <c>Templates/AccountDeletionConfirmationEmail.cshtml</c>. All user-visible strings are
///     pre-localized by <see cref="Composers.AccountDeletionConfirmationEmailComposer" /> using
///     <c>IStringLocalizer&lt;Resources.EmailStrings&gt;</c> so the Razor template stays culture-agnostic.
/// </summary>
public sealed class AccountDeletionConfirmationEmailModel
{
    public string AppName    { get; set; } = "Marechai";
    public string ConfirmUrl { get; set; } = string.Empty;

    /// <summary>Localized email title (h1).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Localized intro sentence beneath the title (mentions the 30-day grace window).</summary>
    public string Intro { get; set; } = string.Empty;

    /// <summary>Localized text for the call-to-action button.</summary>
    public string ButtonText { get; set; } = string.Empty;

    /// <summary>Localized "if the button does not work" preamble for the raw-link fallback.</summary>
    public string LinkFallback { get; set; } = string.Empty;

    /// <summary>Localized "ignore if you didn't request deletion" footer.</summary>
    public string Footer { get; set; } = string.Empty;
}
