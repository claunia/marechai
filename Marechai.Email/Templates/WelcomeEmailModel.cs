namespace Marechai.Email.Templates;

/// <summary>
///     Model for <c>Templates/WelcomeEmail.cshtml</c>. All user-visible strings are pre-localized by
///     <see cref="Composers.WelcomeEmailComposer" /> using
///     <c>IStringLocalizer&lt;Resources.EmailStrings&gt;</c> so the Razor template stays culture-agnostic. URL
///     fields are pre-built from <c>Frontend:BaseUrl</c> by the composer so that section anchors render as
///     real <c>&lt;a&gt;</c> tags in the rendered email body.
/// </summary>
public sealed class WelcomeEmailModel
{
    public string AppName { get; set; } = "Marechai";

    /// <summary>The recipient's display name (or username fallback). Substituted into <see cref="Lead" />.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Localized email subject (used by composer, not by the Razor template).</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Localized email title (h1).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Localized lead sentence beneath the title (already interpolated with display name).</summary>
    public string Lead { get; set; } = string.Empty;

    /// <summary>Localized intro paragraph framing what Marechai is.</summary>
    public string Intro { get; set; } = string.Empty;

    /// <summary>Bullet text covering profile setup; rendered with a link to <see cref="ProfileUrl" />.</summary>
    public string ProfileBullet { get; set; } = string.Empty;

    /// <summary>Bullet text covering collection building; rendered with links to computers/consoles/smartphones.</summary>
    public string CollectionBullet { get; set; } = string.Empty;

    /// <summary>Bullet text covering software catalogue exploration; rendered with link to <see cref="SoftwareUrl" />.</summary>
    public string SoftwareBullet { get; set; } = string.Empty;

    /// <summary>Bullet text covering books and magazines; rendered with links to <see cref="BooksUrl" /> and <see cref="MagazinesUrl" />.</summary>
    public string BooksBullet { get; set; } = string.Empty;

    /// <summary>Localized closing paragraph.</summary>
    public string Closing { get; set; } = string.Empty;

    /// <summary>Localized signoff line (e.g. "Welcome aboard,").</summary>
    public string Signoff { get; set; } = string.Empty;

    /// <summary>Localized team-attribution footer (e.g. "— The Marechai team").</summary>
    public string Footer { get; set; } = string.Empty;

    public string ProfileUrl     { get; set; } = string.Empty;
    public string ComputersUrl   { get; set; } = string.Empty;
    public string ConsolesUrl    { get; set; } = string.Empty;
    public string SmartphonesUrl { get; set; } = string.Empty;
    public string SoftwareUrl    { get; set; } = string.Empty;
    public string BooksUrl       { get; set; } = string.Empty;
    public string MagazinesUrl   { get; set; } = string.Empty;
}
