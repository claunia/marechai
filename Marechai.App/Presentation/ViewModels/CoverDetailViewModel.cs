#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Services;
using Marechai.App.Services.Caching;

namespace Marechai.App.Presentation.ViewModels;

public partial class CoverDetailViewModel : ObservableObject, IRegionAware
{
    readonly SoftwareBrowsingService              _browsingService;
    readonly SoftwareCoverCache                    _coverCache;
    readonly ImageSourceFactory                    _imageSourceFactory;
    readonly IStringLocalizer                      _localizer;
    readonly ILogger<CoverDetailViewModel>         _logger;
    readonly IRegionManager                        _regionManager;

    [ObservableProperty]
    private Microsoft.UI.Xaml.Media.Imaging.BitmapImage? _coverSource;

    [ObservableProperty]
    private string _caption = string.Empty;

    [ObservableProperty]
    private string _softwareName = string.Empty;

    [ObservableProperty]
    private string _typeName = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _errorOccurred;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public CoverDetailViewModel(SoftwareBrowsingService      browsingService,
                                SoftwareCoverCache            coverCache,
                                ImageSourceFactory            imageSourceFactory,
                                IStringLocalizer              localizer,
                                ILogger<CoverDetailViewModel> logger,
                                IRegionManager                regionManager)
    {
        _browsingService    = browsingService;
        _coverCache         = coverCache;
        _imageSourceFactory = imageSourceFactory;
        _localizer          = localizer;
        _logger             = logger;
        _regionManager      = regionManager;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.SoftwareName, out string? softwareName))
            SoftwareName = softwareName ?? string.Empty;

        if(navigationContext.Parameters.TryGetValue<Guid>(NavParamKeys.CoverId, out Guid coverId))
            _ = LoadCoverCommand.ExecuteAsync(coverId);
    }

    [RelayCommand]
    public Task GoBack()
    {
        _regionManager.Regions[RegionNames.Content].NavigationService.Journal.GoBack();

        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task LoadCover(Guid coverId)
    {
        try
        {
            IsLoading     = true;
            ErrorOccurred = false;
            ErrorMessage  = string.Empty;
            CoverSource   = null;

            _logger.LogInformation("Loading cover details for {CoverId}", coverId);

            SoftwareCoverDto? dto = await _browsingService.GetCoverByIdAsync(coverId);

            if(dto is null)
            {
                ErrorOccurred = true;
                ErrorMessage  = _localizer["Cover not found"];
                IsLoading     = false;

                return;
            }

            TypeName = dto.TypeName ?? string.Empty;
            Caption  = dto.Caption  ?? string.Empty;

            await LoadCoverImageAsync(coverId);

            IsLoading = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading cover details for {CoverId}", coverId);
            ErrorOccurred = true;
            ErrorMessage  = ex.Message;
            IsLoading     = false;
        }
    }

    async Task LoadCoverImageAsync(Guid coverId)
    {
        try
        {
            Stream stream = await _coverCache.GetCoverAsync(coverId);
            CoverSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading cover image {CoverId}", coverId);
            ErrorOccurred = true;
            ErrorMessage  = _localizer["Failed to load cover image"];
        }
    }
}
