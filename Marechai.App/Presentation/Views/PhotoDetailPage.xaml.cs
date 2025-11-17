using System;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Marechai.App.Presentation.Views;

public sealed partial class PhotoDetailPage : Page
{
    private Guid? _pendingPhotoId;

    public PhotoDetailPage()
    {
        InitializeComponent();
        DataContextChanged += PhotoDetailPage_DataContextChanged;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Guid? photoId = null;

        if(e.Parameter is PhotoDetailNavigationParameter param) photoId = param.PhotoId;

        if(photoId.HasValue)
        {
            _pendingPhotoId = photoId;

            if(DataContext is PhotoDetailViewModel viewModel)
                _ = viewModel.LoadPhotoCommand.ExecuteAsync(photoId.Value);
        }
    }

    private void PhotoDetailPage_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if(DataContext is PhotoDetailViewModel viewModel && _pendingPhotoId.HasValue)
            _ = viewModel.LoadPhotoCommand.ExecuteAsync(_pendingPhotoId.Value);
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        _pendingPhotoId = null;
    }
}