#nullable enable

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class SoftwareViewPage : Page
{
    public SoftwareViewPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is SoftwareViewViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OpenReviewDialog_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not SoftwareViewViewModel vm) return;

        vm.PrepareReviewDraft();

        var content = new UserReviewEditorControl
        {
            DataContext = vm
        };

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = vm.ReviewDialogTitle,
            Content = content,
            PrimaryButtonText = vm.ReviewSaveButtonText,
            CloseButtonText = vm.ReviewCancelButtonText,
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = vm.CanSubmitReviewDraft
        };

        PropertyChangedEventHandler? handler = null;
        handler = (_, args) =>
        {
            if(args.PropertyName == nameof(SoftwareViewViewModel.CanSubmitReviewDraft))
                dialog.IsPrimaryButtonEnabled = vm.CanSubmitReviewDraft;
        };

        vm.PropertyChanged += handler;

        dialog.PrimaryButtonClick += async (_, args) =>
        {
            args.Cancel = true;

            if(await vm.SubmitReviewDraftAsync())
            {
                vm.PropertyChanged -= handler;
                dialog.Hide();
            }
            else
            {
                dialog.IsPrimaryButtonEnabled = vm.CanSubmitReviewDraft;
            }
        };

        try
        {
            await dialog.ShowAsync();
        }
        finally
        {
            vm.PropertyChanged -= handler;
        }
    }

    private void ReviewFeedback_CloseButtonClick(InfoBar sender, object args)
    {
        if(DataContext is SoftwareViewViewModel vm)
            vm.ClearReviewFeedbackCommand.Execute(null);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(SoftwareViewViewModel.DescriptionHtml) &&
           sender is SoftwareViewViewModel vm &&
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
}
