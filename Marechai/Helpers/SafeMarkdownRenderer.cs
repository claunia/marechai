/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System.Linq;
using Ganss.Xss;
using Markdig;
using Microsoft.AspNetCore.Components;

namespace Marechai.Helpers;

/// <summary>
///     Renders user-supplied markdown into safe HTML. Defence-in-depth: <c>Markdig</c> is configured with
///     <c>DisableHtml()</c> so inline HTML in the source is rejected at parse time, then the rendered HTML is run
///     through <see cref="HtmlSanitizer" /> which strips disallowed tags, attributes and URL schemes.
/// </summary>
public static class SafeMarkdownRenderer
{
    static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    static readonly HtmlSanitizer _sanitizer = BuildSanitizer();

    public static MarkupString Render(string markdown)
    {
        if(string.IsNullOrWhiteSpace(markdown)) return new MarkupString(string.Empty);

        string html      = Markdown.ToHtml(markdown, _pipeline);
        string sanitized = _sanitizer.Sanitize(html);

        return new MarkupString(sanitized);
    }

    static HtmlSanitizer BuildSanitizer()
    {
        var s = new HtmlSanitizer();

        s.AllowedTags.Clear();
        foreach(string tag in new[]
                {
                    "p", "br", "hr",
                    "h1", "h2", "h3", "h4", "h5", "h6",
                    "ul", "ol", "li",
                    "blockquote",
                    "pre", "code",
                    "strong", "em", "del", "ins",
                    "a", "img",
                    "table", "thead", "tbody", "tfoot", "tr", "th", "td",
                    "span", "div"
                })
            s.AllowedTags.Add(tag);

        s.AllowedAttributes.Clear();
        foreach(string attr in new[] { "href", "title", "alt", "src", "colspan", "rowspan" })
            s.AllowedAttributes.Add(attr);

        // Drop class/style/id and every event handler. HtmlSanitizer already strips on*= handlers by default; this
        // doubles down by clearing the allow-list explicitly.
        s.AllowedSchemes.Clear();
        foreach(string scheme in new[] { "http", "https", "mailto" }) s.AllowedSchemes.Add(scheme);

        // Allow site-relative URLs (starting with "/") which Markdig encodes as href="..." without a scheme.
        s.AllowedAttributes.Remove("class");
        s.AllowedCssProperties.Clear();
        s.UriAttributes.Clear();
        foreach(string attr in new[] { "href", "src" }) s.UriAttributes.Add(attr);

        return s;
    }
}
