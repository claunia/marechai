using System;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminInvitationCodesPage : Page
{
    AdminInvitationCodesViewModel? ViewModel => DataContext as AdminInvitationCodesViewModel;

    public AdminInvitationCodesPage()
    {
        InitializeComponent();
    }

    void CopyCodeButton_OnClick(object sender, RoutedEventArgs e)
    {
        if(sender is not FrameworkElement { DataContext: InvitationCodeDto { Code: { } code } }) return;

        try
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(code);
            Clipboard.SetContent(dataPackage);
        }
        catch
        {
            // Clipboard access can be denied on some platforms; the code remains visible on screen.
        }
    }

    async void RevokeButton_OnClick(object sender, RoutedEventArgs e)
    {
        if(ViewModel is null || sender is not FrameworkElement { DataContext: InvitationCodeDto item }) return;

        var dialog = new ContentDialog
        {
            XamlRoot          = XamlRoot,
            Title             = "Revoke invitation code",
            PrimaryButtonText = "Revoke",
            CloseButtonText   = "Cancel",
            DefaultButton     = ContentDialogButton.Close,
            Content           = $"Revoke invitation code '{item.Code}'? This cannot be undone."
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if(result != ContentDialogResult.Primary) return;

        await ViewModel.RevokeCodeCommand.ExecuteAsync(item);
    }
}
