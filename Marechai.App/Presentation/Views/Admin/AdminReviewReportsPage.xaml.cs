using System;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminReviewReportsPage : Page
{
    AdminReviewReportsViewModel? ViewModel => DataContext as AdminReviewReportsViewModel;

    public AdminReviewReportsPage()
    {
        InitializeComponent();
    }

    async void DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if(ViewModel is null || sender is not FrameworkElement { DataContext: ReviewReportListItem item }) return;

        var dialog = new ContentDialog
        {
            XamlRoot          = XamlRoot,
            Title             = "Delete review report",
            PrimaryButtonText = "Delete",
            CloseButtonText   = "Cancel",
            DefaultButton     = ContentDialogButton.Close,
            Content           = "Delete this review report? This cannot be undone."
        };

        ContentDialogResult result = await dialog.ShowAsync();
        if(result != ContentDialogResult.Primary) return;

        await ViewModel.DeleteReportCommand.ExecuteAsync(item);
    }
}
