using System;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class MessagesPage : Page
{
    MessagesViewModel? ViewModel => DataContext as MessagesViewModel;

    public MessagesPage()
    {
        InitializeComponent();
    }

    async void DeleteConversationButton_OnClick(object sender, RoutedEventArgs e)
    {
        if(ViewModel is null || sender is not FrameworkElement { DataContext: ConversationListItem item }) return;

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

        await ViewModel.DeleteConversationCoreAsync(item);
    }
}
