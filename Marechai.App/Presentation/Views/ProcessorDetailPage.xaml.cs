#nullable enable

using System.ComponentModel;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class ProcessorDetailPage : Page
{
    public ProcessorDetailPage()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(args.NewValue is ProcessorDetailViewModel vm)
            vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(ProcessorDetailViewModel.DescriptionHtml) &&
           sender is ProcessorDetailViewModel vm &&
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

    private void ComputersSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(DataContext is ProcessorDetailViewModel vm) vm.ComputersFilterCommand.Execute(null);
    }

    private void ComputersSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            if(DataContext is ProcessorDetailViewModel vm)
                vm.ComputersFilterCommand.Execute(null);
    }

    private void ConsolesSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if(DataContext is ProcessorDetailViewModel vm) vm.ConsolesFilterCommand.Execute(null);
    }

    private void ConsolesSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            if(DataContext is ProcessorDetailViewModel vm)
                vm.ConsolesFilterCommand.Execute(null);
    }

    private void Computer_Click(object sender, RoutedEventArgs e)
    {
        if(sender is Button button && button.Tag is int machineId && DataContext is ProcessorDetailViewModel vm)
            _ = vm.SelectMachineCommand.ExecuteAsync(machineId);
    }

    private void Console_Click(object sender, RoutedEventArgs e)
    {
        if(sender is Button button && button.Tag is int machineId && DataContext is ProcessorDetailViewModel vm)
            _ = vm.SelectMachineCommand.ExecuteAsync(machineId);
    }
}
