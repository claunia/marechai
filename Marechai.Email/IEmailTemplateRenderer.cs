namespace Marechai.Email;

/// <summary>
///     Renders an embedded Razor (<c>.cshtml</c>) template with a strongly-typed model and returns the resulting
///     HTML string. Templates are looked up by file name (without extension) under
///     <c>Marechai.Email/Templates/</c>.
/// </summary>
public interface IEmailTemplateRenderer
{
    /// <summary>
    ///     Renders the template identified by <paramref name="templateKey" /> with <paramref name="model" /> and
    ///     returns the produced HTML.
    /// </summary>
    /// <param name="templateKey">
    ///     File name of the template under <c>Templates/</c> (without the <c>.cshtml</c> extension), e.g.
    ///     <c>"TwoFactorCodeEmail"</c>.
    /// </param>
    Task<string> RenderAsync<TModel>(string templateKey, TModel model);
}
