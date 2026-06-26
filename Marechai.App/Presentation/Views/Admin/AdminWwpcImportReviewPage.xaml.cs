using System;
using Marechai.App.Presentation.ViewModels.Admin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace Marechai.App.Presentation.Views.Admin;

public sealed partial class AdminWwpcImportReviewPage : Page
{
    public AdminWwpcImportReviewPage() => InitializeComponent();

    private async void Merge_Click(object sender, RoutedEventArgs e)
    {
        if((sender as Button)?.CommandParameter is WwpcNameMatchCandidateItem item &&
           DataContext is AdminWwpcImportReviewViewModel vm)
            await vm.EnterMergeModeAsync(item);
    }

    private void VendorCandidate_Click(object sender, RoutedEventArgs e)
    {
        if((sender as Button)?.CommandParameter is WwpcCompanyMatchCandidateItem item &&
           DataContext is AdminWwpcImportReviewViewModel vm)
            vm.PickVendor(item);
    }

    private void OpenCreateCompany_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is AdminWwpcImportReviewViewModel vm)
            vm.OpenCreateCompany();
    }

    private void CancelCreateCompany_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is AdminWwpcImportReviewViewModel vm)
        {
            vm.ShowCreateCompany = false;
            vm.NewCompanyName    = string.Empty;
        }
    }

    private void AddGenre_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is AdminWwpcImportReviewViewModel vm)
            vm.AddSelectedGenre();
    }

    private void RemoveGenre_Click(object sender, RoutedEventArgs e)
    {
        if((sender as Button)?.CommandParameter is WwpcGenreChipItem item &&
           DataContext is AdminWwpcImportReviewViewModel vm)
            vm.RemoveGenre(item);
    }

    private async void OpenImportSource_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not AdminWwpcImportReviewViewModel vm || string.IsNullOrWhiteSpace(vm.Detail?.SourceUrl)) return;

        string url = vm.AbsoluteWwpcUrl(vm.Detail.SourceUrl);
        if(Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            await Launcher.LaunchUriAsync(uri);
    }

    private async void OpenScreenshotSource_Click(object sender, RoutedEventArgs e)
    {
        if((sender as HyperlinkButton)?.CommandParameter is not WwpcScreenshotDecisionItem item) return;
        if(string.IsNullOrWhiteSpace(item.ImageUrl)) return;
        if(Uri.TryCreate(item.ImageUrl, UriKind.Absolute, out Uri? uri))
            await Launcher.LaunchUriAsync(uri);
    }
}
