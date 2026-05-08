namespace Marechai.Email.Resources;

/// <summary>
///     Marker type used as the generic argument for <c>IStringLocalizer&lt;EmailStrings&gt;</c>. The actual
///     translations live in <c>EmailStrings.{resx,es.resx,de.resx,fr.resx,it.resx,pt-BR.resx}</c> next to this
///     file. Picks up the current thread's <see cref="System.Globalization.CultureInfo.CurrentUICulture" />
///     automatically; <c>CultureScope</c> in the server sets that based on the user's <c>Accept-Language</c>
///     header before each send.
/// </summary>
public sealed class EmailStrings { }
