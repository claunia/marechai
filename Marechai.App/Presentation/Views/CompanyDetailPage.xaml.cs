#nullable enable

using System;
using Windows.System;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Views;

public sealed partial class CompanyDetailPage : Page
{
    public CompanyDetailPage()
    {
        InitializeComponent();
    }

    private async void OnTwitterClick(object sender, RoutedEventArgs e)
    {
        if(DataContext is CompanyDetailViewModel viewModel && viewModel.Company?.Twitter is not null)
        {
            var uri = new Uri($"https://www.twitter.com/{viewModel.Company.Twitter}");
            await Launcher.LaunchUriAsync(uri);
        }
    }

    private async void OnFacebookClick(object sender, RoutedEventArgs e)
    {
        if(DataContext is CompanyDetailViewModel viewModel && viewModel.Company?.Facebook is not null)
        {
            var uri = new Uri($"https://www.facebook.com/{viewModel.Company.Facebook}");
            await Launcher.LaunchUriAsync(uri);
        }
    }
}