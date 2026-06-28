using System;
using Marechai.ApiClient.Models;
using Marechai.App.Presentation.ViewModels;
using Marechai.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class ComposeMessagePage : Page
{
    ComposeMessageViewModel? ViewModel => DataContext as ComposeMessageViewModel;

    public ComposeMessagePage()
    {
        InitializeComponent();
    }

    async void RecipientBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if(args.Reason != AutoSuggestionBoxTextChangeReason.UserInput || ViewModel is null) return;
        await ViewModel.SearchRecipientsCommand.ExecuteAsync(sender.Text);
    }

    void RecipientBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        if(args.SelectedItem is UserSummaryDto user)
            ViewModel?.SelectRecipientCommand.Execute(user);
    }

    async void SendButton_OnClick(object sender, RoutedEventArgs e)
    {
        if(ViewModel is null) return;

        (MessageSendResult result, _, string? error) = await ViewModel.SendMessageAsync();
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
