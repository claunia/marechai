#nullable enable

using System;
using System.ComponentModel;
using Windows.System;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class CompanyDetailPage : Page
{
    public CompanyDetailPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is CompanyDetailViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(CompanyDetailViewModel.DescriptionHtml) &&
           sender is CompanyDetailViewModel vm &&
           !string.IsNullOrWhiteSpace(vm.DescriptionHtml))
        {
            bool isDark = ActualTheme == ElementTheme.Dark;
            var  html   = WrapInHtmlDocument(vm.DescriptionHtml, isDark);
            DescriptionWebView.NavigateToString(html);
        }
    }

    private static string WrapInHtmlDocument(string bodyHtml, bool isDark)
    {
        string textColor = isDark ? "#e0e0e0" : "#1a1a1a";
        string bgColor   = isDark ? "#1e1e1e" : "#ffffff";
        string linkColor  = isDark ? "#6cb4ee" : "#0063b1";

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
                 img { max-width: 100%; height: auto; }
               </style>
               </head>
               <body>{{bodyHtml}}</body>
               </html>
               """;
    }

    private async void OnTwitterClick(object sender, RoutedEventArgs e)
    {
        if(DataContext is CompanyDetailViewModel viewModel && viewModel.Company?.Twitter is not null)
        {
            var uri = new Uri($"https://www.twitter.com/{viewModel.Company.Twitter}");
            await Launcher.LaunchUriAsync(uri);
        }
    }

    private async void OnFacebookClick(object sender, RoutedEventArgs e)
    {
        if(DataContext is CompanyDetailViewModel viewModel && viewModel.Company?.Facebook is not null)
        {
            var uri = new Uri($"https://www.facebook.com/{viewModel.Company.Facebook}");
            await Launcher.LaunchUriAsync(uri);
        }
    }
}