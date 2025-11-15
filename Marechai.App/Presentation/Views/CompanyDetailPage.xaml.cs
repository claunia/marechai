#nullable enable

using System;
using Windows.System;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

public sealed partial class CompanyDetailPage : Page
{
    private object? _navigationSource;
    private int?    _pendingCompanyId;

    public CompanyDetailPage()
    {
        InitializeComponent();
        DataContextChanged += CompanyDetailPage_DataContextChanged;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        int? companyId = null;

        // Handle both int and CompanyDetailNavigationParameter
        if(e.Parameter is int intId)
            companyId = intId;
        else if(e.Parameter is CompanyDetailNavigationParameter navParam)
        {
            companyId         = navParam.CompanyId;
            _navigationSource = navParam.NavigationSource;
        }

        if(companyId.HasValue)
        {
            _pendingCompanyId = companyId;

            if(DataContext is CompanyDetailViewModel viewModel)
            {
                viewModel.CompanyId = companyId.Value;
                _                   = viewModel.LoadData.ExecuteAsync(null);
            }
        }
    }

    private void CompanyDetailPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is CompanyDetailViewModel viewModel && _pendingCompanyId.HasValue)
        {
            viewModel.CompanyId = _pendingCompanyId.Value;
            _                   = viewModel.LoadData.ExecuteAsync(null);
        }
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