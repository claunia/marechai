#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Services;
using Marechai.App.Services.Caching;

namespace Marechai.App.Presentation.ViewModels;

public partial class ScreenshotDetailViewModel : ObservableObject, IRegionAware
{
    readonly Client                                _apiClient;
    readonly SoftwareScreenshotCache                  _screenshotCache;
    readonly ImageSourceFactory                       _imageSourceFactory;
    readonly IStringLocalizer                         _localizer;
    readonly ILogger<ScreenshotDetailViewModel>       _logger;
    readonly IRegionManager                           _regionManager;

    [ObservableProperty]
    private Microsoft.UI.Xaml.Media.Imaging.BitmapImage? _screenshotSource;

    [ObservableProperty]
    private string _caption = string.Empty;

    [ObservableProperty]
    private string _softwareName = string.Empty;

    [ObservableProperty]
    private string _platformName = string.Empty;

    [ObservableProperty]
    private string _versionString = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _errorOccurred;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ScreenshotDetailViewModel(Client                           apiClient,
                                      SoftwareScreenshotCache             screenshotCache,
                                      ImageSourceFactory                  imageSourceFactory,
                                      IStringLocalizer                    localizer,
                                      ILogger<ScreenshotDetailViewModel>  logger,
                                      IRegionManager                      regionManager)
    {
        _apiClient          = apiClient;
        _screenshotCache    = screenshotCache;
        _imageSourceFactory = imageSourceFactory;
        _localizer          = localizer;
        _logger             = logger;
        _regionManager      = regionManager;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<Guid>(NavParamKeys.ScreenshotId, out Guid screenshotId))
            _ = LoadScreenshotCommand.ExecuteAsync(screenshotId);
    }

    [RelayCommand]
    public Task GoBack()
    {
        _regionManager.Regions[RegionNames.Content].NavigationService.Journal.GoBack();

        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task LoadScreenshot(Guid screenshotId)
    {
        try
        {
            IsLoading        = true;
            ErrorOccurred    = false;
            ErrorMessage     = string.Empty;
            ScreenshotSource = null;

            _logger.LogInformation("Loading screenshot details for {ScreenshotId}", screenshotId);

            SoftwareScreenshotDto? dto =
                await _apiClient.Software.Screenshots[screenshotId.ToString()].GetAsync();

            if(dto is null)
            {
                ErrorOccurred = true;
                ErrorMessage  = _localizer["Screenshot not found"];
                IsLoading     = false;

                return;
            }

            SoftwareName  = dto.SoftwareName  ?? string.Empty;
            PlatformName  = dto.PlatformName  ?? string.Empty;
            VersionString = dto.VersionString  ?? string.Empty;
            Caption       = dto.Caption        ?? string.Empty;

            await LoadScreenshotImageAsync(screenshotId);

            IsLoading = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading screenshot details for {ScreenshotId}", screenshotId);
            ErrorOccurred = true;
            ErrorMessage  = ex.Message;
            IsLoading     = false;
        }
    }

    async Task LoadScreenshotImageAsync(Guid screenshotId)
    {
        try
        {
            Stream stream = await _screenshotCache.GetScreenshotAsync(screenshotId);
            ScreenshotSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading screenshot image {ScreenshotId}", screenshotId);
            ErrorOccurred = true;
            ErrorMessage  = _localizer["Failed to load screenshot image"];
        }
    }
}
