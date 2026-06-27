using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

public sealed partial class AboutPage : Page
{
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

    private void RenderContent(IStringLocalizer localizer)
    {
        bool   isDark = ActualTheme == ElementTheme.Dark;
        string html   = BuildHtml(localizer, isDark);
        ContentWebView.NavigateToString(html);
    }

    private static string BuildHtml(IStringLocalizer l, bool isDark)
    {
        string textColor      = isDark ? "#e0e0e0" : "#1a1a1a";
        string bgColor        = isDark ? "#1e1e1e" : "#ffffff";
        string linkColor      = isDark ? "#6cb4ee" : "#0063b1";
        string cardBg         = isDark ? "#2a2a2a" : "#f5f5f5";
        string headerTitle    = l["AboutPage_HeaderTitle"].Value;

        string Section(string header, string body) => $"""
            <section>
              <h2>{header}</h2>
              <p>{body}</p>
            </section>
            """;

        string sections = string.Join("\n",
            Section(l["AboutPage_WhoAreWe_Header"].Value, l["AboutPage_WhoAreWe_Body"].Value),
            Section(l["AboutPage_WhatIsMarechai_Header"].Value, l["AboutPage_WhatIsMarechai_Body"].Value),
            Section(l["AboutPage_WhyNotWikipedia_Header"].Value, l["AboutPage_WhyNotWikipedia_Body"].Value),
            Section(l["AboutPage_IsTheMaterialFree_Header"].Value, l["AboutPage_IsTheMaterialFree_Body"].Value),
            Section(l["AboutPage_ApiAccess_Header"].Value, l["AboutPage_ApiAccess_Body"].Value),
            $"""
            <section>
              <h2>{l["AboutPage_HowCanIHelp_Header"].Value}</h2>
              <p>{l["AboutPage_HowCanIHelp_Body1"].Value}</p>
              <p>{l["AboutPage_HowCanIHelp_Body2"].Value}</p>
            </section>
            """,
            Section(l["AboutPage_WhyAnotherSite_Header"].Value, l["AboutPage_WhyAnotherSite_Body"].Value),
            Section(l["AboutPage_Founder_Header"].Value, l["AboutPage_Founder_Body"].Value),
            $"""
            <section>
              <h2>{l["AboutPage_Copyright_Header"].Value}</h2>
              <p>{l["AboutPage_Copyright_Body"].Value}</p>
              <p class="secondary">{l["AboutPage_Copyright_Cookies"].Value}</p>
            </section>
            """,
            $"""
            <section>
              <h2>{l["AboutPage_Dedication_Header"].Value}</h2>
              <p>{l["AboutPage_Dedication_Intro"].Value}</p>
              <ul>
                <li>{l["AboutPage_Dedication_Tata"].Value}</li>
                <li>{l["AboutPage_Dedication_Jhenn"].Value}</li>
              </ul>
            </section>
            """,
            $"""
            <section>
              <h2>{l["AboutPage_Contact_Header"].Value}</h2>
              <p>{l["AboutPage_Contact_Body"].Value}</p>
              <p><strong>{l["AboutPage_Support"].Value}:</strong> <a href="mailto:museum@claunia.com">museum@claunia.com</a></p>
            </section>
            """);

        return $$"""
               <!DOCTYPE html>
               <html>
               <head>
               <meta charset="utf-8" />
               <meta name="viewport" content="width=device-width, initial-scale=1" />
               <style>
                 body {
                   font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                   font-size: 14px;
                   line-height: 1.6;
                   margin: 0;
                   padding: 8px;
                   color: {{textColor}};
                   background: {{bgColor}};
                 }
                 a { color: {{linkColor}}; }
                 .header {
                   background: linear-gradient(135deg, #3F8CFF 0%, #7C3BFF 100%);
                   border-radius: 16px;
                   padding: 24px;
                   margin-bottom: 16px;
                   color: white;
                   font-size: 24px;
                   font-weight: bold;
                 }
                 section {
                   background: {{cardBg}};
                   border-radius: 12px;
                   padding: 16px 20px;
                   margin-bottom: 16px;
                 }
                 h2 { margin: 0 0 8px 0; font-size: 17px; }
                 p { margin: 0 0 8px 0; }
                 p.secondary { font-size: 12px; opacity: 0.8; }
                 ul { margin: 8px 0 0 0; padding-left: 20px; }
               </style>
               </head>
               <body>
                 <div class="header">{{headerTitle}}</div>
                 {{sections}}
               </body>
               </html>
               """;
    }
}
