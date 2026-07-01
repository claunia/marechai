using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Dialogs;

public static class ConfirmationDialogHelper
{
    public static async Task<bool> ConfirmDeleteAsync(IStringLocalizer localizer, string itemName)
    {
        var dialog = new ContentDialog
        {
            XamlRoot          = App.MainWindow?.Content?.XamlRoot,
            Title             = localizer["ConfirmDeleteDialogTitle"],
            Content           = string.Format(localizer["ConfirmDeleteDialogMessage"], itemName),
            PrimaryButtonText = localizer["DeleteButton"],
            CloseButtonText   = localizer["CancelButton"],
            DefaultButton     = ContentDialogButton.Close
        };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }
}
