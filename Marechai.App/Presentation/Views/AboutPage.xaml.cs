using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace Marechai.App.Presentation.Views;

public sealed partial class AboutPage : Page
{
    private static readonly Regex InlineTagRegex =
        new("""<b>|</b>|<br/>|<a href="(?<href>[^"]*)"(?:\s+target="[^"]*")?>|</a>""", RegexOptions.Compiled);

    public AboutPage()
    {
        InitializeComponent();
        DataContextChanged += AboutPage_DataContextChanged;
    }

    private void AboutPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is AboutViewModel viewModel) RenderContent(viewModel.Localizer);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if(DataContext is AboutViewModel viewModel) RenderContent(viewModel.Localizer);
    }

    private void RenderContent(IStringLocalizer l)
    {
        HeaderTitleText.Text = l["AboutPage_HeaderTitle"].Value;

        SectionsPanel.Children.Clear();

        SectionsPanel.Children.Add(BuildSection(l["AboutPage_WhoAreWe_Header"].Value,
                                                 l["AboutPage_WhoAreWe_Body"].Value));

        SectionsPanel.Children.Add(BuildSection(l["AboutPage_WhatIsMarechai_Header"].Value,
                                                 l["AboutPage_WhatIsMarechai_Body"].Value));

        SectionsPanel.Children.Add(BuildSection(l["AboutPage_WhyNotWikipedia_Header"].Value,
                                                 l["AboutPage_WhyNotWikipedia_Body"].Value));

        SectionsPanel.Children.Add(BuildSection(l["AboutPage_IsTheMaterialFree_Header"].Value,
                                                 l["AboutPage_IsTheMaterialFree_Body"].Value));

        SectionsPanel.Children.Add(BuildSection(l["AboutPage_ApiAccess_Header"].Value,
                                                 l["AboutPage_ApiAccess_Body"].Value));

        SectionsPanel.Children.Add(BuildSection(l["AboutPage_HowCanIHelp_Header"].Value,
                                                 l["AboutPage_HowCanIHelp_Body1"].Value,
                                                 l["AboutPage_HowCanIHelp_Body2"].Value));

        SectionsPanel.Children.Add(BuildSection(l["AboutPage_WhyAnotherSite_Header"].Value,
                                                 l["AboutPage_WhyAnotherSite_Body"].Value));

        SectionsPanel.Children.Add(BuildSection(l["AboutPage_Founder_Header"].Value,
                                                 l["AboutPage_Founder_Body"].Value));

        SectionsPanel.Children.Add(BuildSection(l["AboutPage_Copyright_Header"].Value,
                                                 l["AboutPage_Copyright_Body"].Value,
                                                 l["AboutPage_Copyright_Cookies"].Value));

        SectionsPanel.Children.Add(BuildDedicationSection(l));
        SectionsPanel.Children.Add(BuildContactSection(l));
    }

    private static Border BuildSection(string header, params string[] paragraphs)
    {
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(BuildHeaderTextBlock(header));

        foreach(string paragraph in paragraphs)
            stack.Children.Add(BuildRichTextBlock(paragraph));

        return WrapInCard(stack);
    }

    private Border BuildDedicationSection(IStringLocalizer l)
    {
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(BuildHeaderTextBlock(l["AboutPage_Dedication_Header"].Value));
        stack.Children.Add(BuildRichTextBlock(l["AboutPage_Dedication_Intro"].Value));
        stack.Children.Add(BuildRichTextBlock($"•  {l["AboutPage_Dedication_Tata"].Value}"));
        stack.Children.Add(BuildRichTextBlock($"•  {l["AboutPage_Dedication_Jhenn"].Value}"));

        return WrapInCard(stack);
    }

    private Border BuildContactSection(IStringLocalizer l)
    {
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(BuildHeaderTextBlock(l["AboutPage_Contact_Header"].Value));
        stack.Children.Add(BuildRichTextBlock(l["AboutPage_Contact_Body"].Value));

        var textBlock = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap };
        textBlock.Inlines.Add(new Run { Text = $"{l["AboutPage_Support"].Value}: ", FontWeight = FontWeights.Bold });

        var hyperlink = new Hyperlink();
        hyperlink.Inlines.Add(new Run { Text = "museum@claunia.com" });
        hyperlink.Click += (_, _) => _ = Launcher.LaunchUriAsync(new Uri("mailto:museum@claunia.com"));
        textBlock.Inlines.Add(hyperlink);
        stack.Children.Add(textBlock);

        return WrapInCard(stack);
    }

    private static TextBlock BuildHeaderTextBlock(string header) =>
        new()
        {
            Text       = header,
            FontSize   = 17,
            FontWeight = FontWeights.SemiBold
        };

    private static TextBlock BuildRichTextBlock(string htmlSubsetText)
    {
        var textBlock = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap };
        AddInlinesFromHtmlSubset(textBlock.Inlines, htmlSubsetText);

        return textBlock;
    }

    /// <summary>
    ///     Parses the limited HTML subset used in localized About page strings
    ///     (&lt;b&gt;, &lt;br/&gt;, &lt;a href="..."&gt;, possibly nested) into TextBlock inlines.
    ///     Hyperlink derives from Span in WinUI, so both nest on the same stack.
    /// </summary>
    private static void AddInlinesFromHtmlSubset(InlineCollection inlines, string text)
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

    private static Border WrapInCard(UIElement content) =>
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
