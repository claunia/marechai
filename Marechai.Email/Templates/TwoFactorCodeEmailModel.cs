namespace Marechai.Email.Templates;

/// <summary>
///     Model for <c>Templates/TwoFactorCodeEmail.cshtml</c>. All user-visible strings are pre-localized by
///     <see cref="Composers.TwoFactorEmailComposer" /> using <c>IStringLocalizer&lt;Resources.EmailStrings&gt;</c>
///     so the Razor template stays culture-agnostic.
/// </summary>
public sealed class TwoFactorCodeEmailModel
{
    public string AppName          { get; set; } = "Marechai";
    public string Code             { get; set; } = string.Empty;
    public int    ExpiresInMinutes { get; set; } = 5;

    /// <summary>Localized email title (h1).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Localized intro sentence beneath the title.</summary>
    public string Intro { get; set; } = string.Empty;

    /// <summary>Localized expiry sentence (already formatted with <see cref="ExpiresInMinutes" />).</summary>
    public string Expiry { get; set; } = string.Empty;

    /// <summary>Localized "ignore if you didn't try to sign in" footer.</summary>
    public string Footer { get; set; } = string.Empty;
}
