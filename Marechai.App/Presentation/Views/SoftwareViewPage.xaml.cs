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

    private async void OpenReportReviewDialog_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not SoftwareViewViewModel vm ||
           sender is not FrameworkElement { DataContext: Marechai.App.Presentation.Models.UserReviewDisplayItem review })
            return;

        vm.PrepareReviewReportDraft(review);

        var content = new ReportReviewEditorControl
        {
            DataContext = vm
        };

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = vm.ReportReviewDialogTitle,
            Content = content,
            PrimaryButtonText = vm.ReportReviewSubmitButtonText,
            CloseButtonText = vm.ReviewCancelButtonText,
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = vm.CanSubmitReviewReportDraft
        };

        PropertyChangedEventHandler? handler = null;
        handler = (_, args) =>
        {
            if(args.PropertyName == nameof(SoftwareViewViewModel.CanSubmitReviewReportDraft))
                dialog.IsPrimaryButtonEnabled = vm.CanSubmitReviewReportDraft;
        };

        vm.PropertyChanged += handler;

        dialog.PrimaryButtonClick += async (_, args) =>
        {
            args.Cancel = true;

            if(await vm.SubmitReviewReportDraftAsync())
            {
                vm.PropertyChanged -= handler;
                dialog.Hide();
            }
            else
            {
                dialog.IsPrimaryButtonEnabled = vm.CanSubmitReviewReportDraft;
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

}
