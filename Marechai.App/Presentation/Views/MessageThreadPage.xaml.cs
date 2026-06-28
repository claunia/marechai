using System;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.ViewModels;
using Marechai.App.Services;
using Marechai.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class MessageThreadPage : Page
{
    MessageThreadViewModel? ViewModel => DataContext as MessageThreadViewModel;

    public MessageThreadPage()
    {
        InitializeComponent();
    }

    async void ReportButton_OnClick(object sender, RoutedEventArgs e)
    {
        if(ViewModel is null || sender is not FrameworkElement { DataContext: MessageThreadItem item }) return;

        var reasonBox = new ComboBox
        {
            ItemsSource = new[] { "Spam", "Offensive", "Misleading", "Off-topic", "Other" },
            SelectedIndex = 0
        };

        var explanationBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping  = TextWrapping.Wrap,
            MinHeight     = 120
        };

        var dialog = new ContentDialog
        {
            XamlRoot          = XamlRoot,
            Title             = "Report message",
            PrimaryButtonText = "Submit",
            CloseButtonText   = "Cancel",
            DefaultButton     = ContentDialogButton.Primary,
            Content = new StackPanel
            {
                Spacing  = 12,
                MinWidth = 400,
                Children =
                {
                    new TextBlock { Text = "Reason" },
                    reasonBox,
                    new TextBlock { Text = "Explanation (optional)" },
                    explanationBox
                }
            }
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if(result != ContentDialogResult.Primary) return;

        ReviewReportReason reason = reasonBox.SelectedIndex switch
        {
            0 => ReviewReportReason.Spam,
            1 => ReviewReportReason.Offensive,
            2 => ReviewReportReason.Misleading,
            3 => ReviewReportReason.OffTopic,
            _ => ReviewReportReason.Other
        };

        (bool succeeded, string? error) = await ViewModel.ReportMessageAsync(item, reason, explanationBox.Text);
        if(!succeeded)
        {
            var failureDialog = new ContentDialog
            {
                XamlRoot          = XamlRoot,
                Title             = "Report failed",
                CloseButtonText   = "OK",
                DefaultButton     = ContentDialogButton.Close,
                Content           = error ?? "Could not submit the report."
            };
            await failureDialog.ShowAsync();
        }
    }

    async void DeleteConversationButton_OnClick(object sender, RoutedEventArgs e)
    {
        if(ViewModel is null) return;

        var dialog = new ContentDialog
        {
            XamlRoot          = XamlRoot,
            Title             = "Delete conversation",
            PrimaryButtonText = "Delete",
            CloseButtonText   = "Cancel",
            DefaultButton     = ContentDialogButton.Close,
            Content           = "Are you sure you want to delete this conversation? It will only be removed from your view."
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if(result != ContentDialogResult.Primary) return;

        await ViewModel.DeleteConversationCoreAsync();
    }

    async void DeleteMessageButton_OnClick(object sender, RoutedEventArgs e)
    {
        if(ViewModel is null || sender is not FrameworkElement { DataContext: MessageThreadItem item }) return;

        var dialog = new ContentDialog
        {
            XamlRoot          = XamlRoot,
            Title             = "Delete message",
            PrimaryButtonText = "Delete",
            CloseButtonText   = "Cancel",
            DefaultButton     = ContentDialogButton.Close,
            Content           = "Are you sure you want to delete this message? It will only be removed from your view."
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if(result != ContentDialogResult.Primary) return;

        await ViewModel.DeleteMessageCoreAsync(item);
    }

    async void SendReplyButton_OnClick(object sender, RoutedEventArgs e)
    {
        if(ViewModel is null) return;

        (MessageSendResult result, string? error) = await ViewModel.SendReplyCoreAsync();
        if(result != MessageSendResult.InboxFull && result != MessageSendResult.RateLimited) return;

        var dialog = new ContentDialog
        {
            XamlRoot          = XamlRoot,
            Title             = result == MessageSendResult.InboxFull ? "Inbox Full" : "Rate Limited",
            CloseButtonText   = "OK",
            DefaultButton     = ContentDialogButton.Close,
            Content           = error ?? (result == MessageSendResult.InboxFull
                ? "The recipient inbox is full."
                : "You are sending messages too quickly. Please try again later.")
        };

        await dialog.ShowAsync();
    }
}
