using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace Marechai.App.Presentation.Views;

internal static class RichTextHelpers
{
    private static readonly Regex InlineTagRegex =
        new("""<b>|</b>|<br/>|<a href="(?<href>[^"]*)"(?:\s+target="[^"]*")?>|</a>""", RegexOptions.Compiled);

    public static TextBlock BuildHeaderTextBlock(string header) =>
        new()
        {
            Text       = header,
            FontSize   = 17,
            FontWeight = FontWeights.SemiBold
        };

    public static TextBlock BuildRichTextBlock(string htmlSubsetText)
    {
        var textBlock = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap };
        AddInlinesFromHtmlSubset(textBlock.Inlines, htmlSubsetText);

        return textBlock;
    }

    /// <summary>
    ///     Parses the limited HTML subset used in localized strings
    ///     (&lt;b&gt;, &lt;br/&gt;, &lt;a href="..."&gt;, possibly nested) into TextBlock inlines.
    ///     Hyperlink derives from Span in WinUI, so both nest on the same stack.
    /// </summary>
    public static void AddInlinesFromHtmlSubset(InlineCollection inlines, string text)
    {
        var    openSpans = new Stack<Span>();
        int    lastIndex = 0;

        void Append(Inline inline)
        {
            if(openSpans.Count > 0)
                openSpans.Peek().Inlines.Add(inline);
            else
                inlines.Add(inline);
        }

        foreach(Match match in InlineTagRegex.Matches(text))
        {
            string plainText = text[lastIndex..match.Index];

            if(plainText.Length > 0) Append(new Run { Text = plainText });

            lastIndex = match.Index + match.Length;

            if(match.Value == "<b>")
            {
                var span = new Span { FontWeight = FontWeights.Bold };
                Append(span);
                openSpans.Push(span);
            }
            else if(match.Value == "<br/>")
                Append(new LineBreak());
            else if(match.Value is "</b>" or "</a>")
            {
                if(openSpans.Count > 0) openSpans.Pop();
            }
            else if(match.Groups["href"].Success)
            {
                string href      = match.Groups["href"].Value;
                var    hyperlink = new Hyperlink();
                hyperlink.Click += (_, _) => _ = Launcher.LaunchUriAsync(new Uri(href));
                Append(hyperlink);
                openSpans.Push(hyperlink);
            }
        }

        string remaining = text[lastIndex..];

        if(remaining.Length > 0) Append(new Run { Text = remaining });
    }

    public static Border WrapInCard(UIElement content) =>
        new()
        {
            CornerRadius = new CornerRadius(12),
            Background   = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
            BorderBrush  = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            BorderThickness = new Thickness(1),
            Padding         = new Thickness(16, 20, 16, 20),
            Child           = content
        };
}
