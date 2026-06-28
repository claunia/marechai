using System;
using Marechai.ApiClient.Models;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminOldDosImportReviewPage : Page
{
    public AdminOldDosImportReviewPage() => InitializeComponent();

    private async void Merge_Click(object sender, RoutedEventArgs e)
    {
        if((sender as Button)?.CommandParameter is OldDosNameMatchCandidateItem item &&
           DataContext is AdminOldDosImportReviewViewModel vm)
            await vm.EnterMergeModeAsync(item);
    }

    private void OpenCreateCompany_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is AdminOldDosImportReviewViewModel vm)
            vm.OpenCreateCompany();
    }

    private void CancelCreateCompany_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is AdminOldDosImportReviewViewModel vm)
        {
            vm.ShowCreateCompany = false;
            vm.NewCompanyName    = string.Empty;
        }
    }

    private void AddGenre_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is AdminOldDosImportReviewViewModel vm)
            vm.AddSelectedGenre();
    }

    private void RemoveGenre_Click(object sender, RoutedEventArgs e)
    {
        if((sender as Button)?.CommandParameter is SoftwareGenreDto item &&
           DataContext is AdminOldDosImportReviewViewModel vm)
            vm.RemoveGenre(item);
    }

    private async void OpenImportSource_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminOldDosImportReviewViewModel vm || string.IsNullOrWhiteSpace(vm.Detail?.SourceUrl)) return;

        string url = vm.AbsoluteOldDosUrl(vm.Detail.SourceUrl);
        if(Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            await Launcher.LaunchUriAsync(uri);
    }

    private async void OpenDownload_Click(object sender, RoutedEventArgs e)
    {
        if((sender as HyperlinkButton)?.CommandParameter is not OldDosVersionDecisionItem item) return;
        if(DataContext is not AdminOldDosImportReviewViewModel vm) return;

        string url = vm.AbsoluteOldDosUrl(item.DownloadUrl);
        if(Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            await Launcher.LaunchUriAsync(uri);
    }
}
