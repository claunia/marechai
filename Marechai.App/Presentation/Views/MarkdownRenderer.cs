#nullable enable

using System;
using System.Linq;
using Markdig;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using Block = Markdig.Syntax.Block;
using Markdown = Markdig.Markdown;

namespace Marechai.App.Presentation.Views;

/// <summary>
///     Renders markdown directly into native XAML elements (TextBlock/Span/Hyperlink/Image trees) instead of going
///     through HTML and an embedded WebView. Uno's WebView2 doesn't reliably composite on every desktop backend
///     (it renders nothing on Linux/X11), so this avoids depending on a native embedded browser at all - it works
///     identically on every Uno target. <c>DisableHtml()</c> means literal HTML in the source is never interpreted,
///     just shown as plain text, so there's no markup-injection surface to sanitize.
/// </summary>
internal static class MarkdownRenderer
{
    static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    public static UIElement Render(string? markdown)
    {
        var root = new StackPanel { Spacing = 8 };

        if(string.IsNullOrWhiteSpace(markdown)) return root;

        MarkdownDocument document = Markdown.Parse(markdown, _pipeline);

        foreach(Block block in document) AppendBlock(root, block);

        return root;
    }

    static void AppendBlock(Panel container, Block block)
    {
        switch(block)
        {
            case HeadingBlock heading:
                container.Children.Add(BuildHeading(heading));
                break;

            case QuoteBlock quote:
                container.Children.Add(BuildQuote(quote));
                break;

            case ListBlock list:
                container.Children.Add(BuildList(list));
                break;

            case FencedCodeBlock or CodeBlock:
                container.Children.Add(BuildCodeBlock((LeafBlock)block));
                break;

            case ThematicBreakBlock:
                container.Children.Add(new Border
                {
                    Height          = 1,
                    Opacity         = 0.4,
                    Margin          = new Thickness(0, 4, 0, 4),
                    BorderThickness = new Thickness(1),
                    BorderBrush     = ThemeBrush("TextFillColorSecondaryBrush")
                });

                break;

            case Table table:
                container.Children.Add(BuildTable(table));
                break;

            case FootnoteGroup footnotes:
                container.Children.Add(BuildFootnotes(footnotes));
                break;

            case LeafBlock { Inline: not null } leaf:
                container.Children.Add(BuildParagraph(leaf.Inline));
                break;
        }
    }

    static TextBlock BuildHeading(HeadingBlock heading)
    {
        var textBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontWeight   = FontWeights.SemiBold,
            FontSize = heading.Level switch
            {
                1 => 24,
                2 => 20,
                3 => 18,
                4 => 16,
                5 => 15,
                _ => 14
            }
        };

        if(heading.Inline is not null) AppendInlines(textBlock.Inlines, heading.Inline);

        return textBlock;
    }

    static TextBlock BuildParagraph(ContainerInline? inline)
    {
        var textBlock = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 14 };

        if(inline is not null) AppendInlines(textBlock.Inlines, inline);

        return textBlock;
    }

    static Border BuildQuote(QuoteBlock quote)
    {
        var inner = new StackPanel { Spacing = 4 };

        foreach(Block child in quote) AppendBlock(inner, child);

        return new Border
        {
            BorderBrush     = ThemeBrush("TextFillColorSecondaryBrush"),
            BorderThickness = new Thickness(3, 0, 0, 0),
            Padding         = new Thickness(8, 0, 0, 0),
            Child           = inner
        };
    }

    static StackPanel BuildList(ListBlock list)
    {
        var panel = new StackPanel { Spacing = 4 };
        int index = list.IsOrdered && int.TryParse(list.OrderedStart, out int start) ? start : 1;

        foreach(Block item in list)
        {
            if(item is not ListItemBlock listItem) continue;

            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };

            row.Children.Add(new TextBlock
            {
                Text = list.IsOrdered ? $"{index}." : "•", FontSize = 14, Margin = new Thickness(8, 0, 0, 0)
            });

            var content = new StackPanel { Spacing = 4 };

            foreach(Block child in listItem) AppendBlock(content, child);

            row.Children.Add(content);
            panel.Children.Add(row);
            index++;
        }

        return panel;
    }

    static Border BuildCodeBlock(LeafBlock code)
    {
        var textBlock = new TextBlock
        {
            Text         = code.Lines.ToString(),
            FontFamily   = new FontFamily("Consolas"),
            FontSize     = 13,
            TextWrapping = TextWrapping.NoWrap
        };

        return new Border
        {
            Background   = ThemeBrush("CardBackgroundFillColorDefaultBrush"),
            Padding      = new Thickness(8),
            CornerRadius = new CornerRadius(4),
            Child = new ScrollViewer
            {
                Content                      = textBlock,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility   = ScrollBarVisibility.Disabled
            }
        };
    }

    static Grid BuildTable(Table table)
    {
        var grid = new Grid();

        for(int c = 0; c < table.ColumnDefinitions.Count; c++) grid.ColumnDefinitions.Add(new ColumnDefinition());

        int rowIndex = 0;

        foreach(TableRow row in table.OfType<TableRow>())
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            int columnIndex = 0;

            foreach(TableCell cell in row.OfType<TableCell>())
            {
                var textBlock = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Margin       = new Thickness(4),
                    FontWeight   = row.IsHeader ? FontWeights.SemiBold : FontWeights.Normal
                };

                foreach(Block cellBlock in cell)
                    if(cellBlock is ParagraphBlock { Inline: not null } paragraph)
                        AppendInlines(textBlock.Inlines, paragraph.Inline);

                Grid.SetRow(textBlock, rowIndex);
                Grid.SetColumn(textBlock, columnIndex);
                grid.Children.Add(textBlock);
                columnIndex++;
            }

            rowIndex++;
        }

        return grid;
    }

    static StackPanel BuildFootnotes(FootnoteGroup group)
    {
        var panel = new StackPanel { Spacing = 4, Margin = new Thickness(0, 8, 0, 0) };

        panel.Children.Add(new Border
        {
            Height          = 1,
            Opacity         = 0.4,
            Margin          = new Thickness(0, 0, 0, 4),
            BorderThickness = new Thickness(1),
            BorderBrush     = ThemeBrush("TextFillColorSecondaryBrush")
        });

        foreach(Block block in group)
        {
            if(block is not Footnote footnote) continue;

            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };

            row.Children.Add(new TextBlock
            {
                Text = $"{footnote.Order}.", FontSize = 12, Opacity = 0.7, Margin = new Thickness(8, 0, 0, 0)
            });

            var content = new StackPanel { Spacing = 4 };

            foreach(Block child in footnote) AppendBlock(content, child);

            row.Children.Add(content);
            panel.Children.Add(row);
        }

        return panel;
    }

    static void AppendInlines(InlineCollection target, ContainerInline container)
    {
        foreach(Markdig.Syntax.Inlines.Inline inline in container) AppendInline(target, inline);
    }

    static void AppendInline(InlineCollection target, Markdig.Syntax.Inlines.Inline inline)
    {
        switch(inline)
        {
            case LiteralInline literal:
                target.Add(new Run { Text = literal.Content.ToString() });
                break;

            case LineBreakInline:
                target.Add(new LineBreak());
                break;

            case EmphasisInline emphasis:
                var span = new Span();

                if(emphasis.DelimiterCount >= 2) span.FontWeight = FontWeights.Bold;
                else span.FontStyle                               = Windows.UI.Text.FontStyle.Italic;

                AppendInlines(span.Inlines, emphasis);
                target.Add(span);
                break;

            case CodeInline code:
                target.Add(new Run { Text = code.Content, FontFamily = new FontFamily("Consolas") });
                break;

            case LinkInline { IsImage: true, Url: not null } image:
                target.Add(new InlineUIContainer
                {
                    Child = new Image
                    {
                        Source  = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(image.Url)),
                        MaxWidth = 480,
                        MaxHeight = 360,
                        Stretch  = Stretch.Uniform
                    }
                });

                break;

            case LinkInline link:
                var hyperlink = new Hyperlink();
                AppendInlines(hyperlink.Inlines, link);

                if(Uri.TryCreate(link.Url, UriKind.Absolute, out Uri? uri))
                    hyperlink.Click += async (_, _) => await Launcher.LaunchUriAsync(uri);

                target.Add(hyperlink);
                break;

            case FootnoteLink footnoteLink:
                target.Add(new Span
                {
                    FontSize = 11,
                    Inlines  = { new Run { Text = $"[{footnoteLink.Footnote?.Order}]" } }
                });

                break;

            case ContainerInline containerInline:
                AppendInlines(target, containerInline);
                break;
        }
    }

    static Brush ThemeBrush(string resourceKey) =>
        (Brush)Application.Current.Resources[resourceKey];
}
