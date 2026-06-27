#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Services;
using Marechai.App.Services.Caching;

namespace Marechai.App.Presentation.ViewModels;

public partial class SoftwarePromoArtDetailViewModel : ObservableObject, IRegionAware
{
    readonly ImageSourceFactory                              _imageSourceFactory;
    readonly IStringLocalizer                                _localizer;
    readonly ILogger<SoftwarePromoArtDetailViewModel>        _logger;
    readonly SoftwarePromoArtCache                           _softwarePromoArtCache;
    readonly SoftwarePromoArtService                         _softwarePromoArtService;
    readonly SoftwareBrowsingService                         _softwareBrowsingService;
    readonly IRegionManager                                  _regionManager;

    [ObservableProperty]
    private string _caption = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _errorOccurred;

    [ObservableProperty]
    private string _groupName = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _softwareName = string.Empty;

    [ObservableProperty]
    private Microsoft.UI.Xaml.Media.Imaging.BitmapImage? _promoArtSource;

    public SoftwarePromoArtDetailViewModel(SoftwarePromoArtService                  softwarePromoArtService,
                                            SoftwarePromoArtCache                    softwarePromoArtCache,
                                            SoftwareBrowsingService                  softwareBrowsingService,
                                            ImageSourceFactory                       imageSourceFactory,
                                            IStringLocalizer                         localizer,
                                            ILogger<SoftwarePromoArtDetailViewModel> logger,
                                            IRegionManager                           regionManager)
    {
        _softwarePromoArtService = softwarePromoArtService;
        _softwarePromoArtCache   = softwarePromoArtCache;
        _softwareBrowsingService = softwareBrowsingService;
        _imageSourceFactory      = imageSourceFactory;
        _localizer               = localizer;
        _logger                  = logger;
        _regionManager           = regionManager;
    }

    public bool HasCaption => !string.IsNullOrWhiteSpace(Caption);
    public bool HasGroupName => !string.IsNullOrWhiteSpace(GroupName);

    partial void OnCaptionChanged(string value) => OnPropertyChanged(nameof(HasCaption));
    partial void OnGroupNameChanged(string value) => OnPropertyChanged(nameof(HasGroupName));

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<Guid>(NavParamKeys.SoftwarePromoArtId, out Guid promoArtId))
            _ = LoadPromoArtCommand.ExecuteAsync(promoArtId);
    }

    [RelayCommand]
    public Task GoBack()
    {
        _regionManager.Regions[RegionNames.Content].NavigationService.Journal.GoBack();

        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task LoadPromoArt(Guid promoArtId)
    {
        try
        {
            IsLoading      = true;
            ErrorOccurred  = false;
            ErrorMessage   = string.Empty;
            PromoArtSource = null;
            SoftwareName   = string.Empty;
            GroupName      = string.Empty;
            Caption        = string.Empty;

            _logger.LogInformation("Loading software promo art details for {PromoArtId}", promoArtId);

            SoftwarePromoArtDto? promoArt = await _softwarePromoArtService.GetPromoArtDetailsAsync(promoArtId);

            if(promoArt is null)
            {
                ErrorOccurred = true;
                ErrorMessage  = _localizer["PromoArtNotFound"];
                IsLoading     = false;

                return;
            }

            GroupName = promoArt.GroupName ?? string.Empty;
            Caption   = promoArt.Caption   ?? string.Empty;

            int softwareId = promoArt.SoftwareId ?? 0;

            SoftwareDto? software = softwareId > 0
                                         ? await _softwareBrowsingService.GetSoftwareByIdAsync(softwareId)
                                         : null;
            SoftwareName = software?.Name ?? string.Empty;

            await LoadPromoArtImageAsync(promoArtId);

            IsLoading = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software promo art details for {PromoArtId}", promoArtId);
            ErrorOccurred = true;
            ErrorMessage  = ex.Message;
            IsLoading     = false;
        }
    }

    async Task LoadPromoArtImageAsync(Guid promoArtId)
    {
        try
        {
            Stream stream = await _softwarePromoArtCache.GetPromoArtAsync(promoArtId);
            PromoArtSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software promo art image {PromoArtId}", promoArtId);
            ErrorOccurred = true;
            ErrorMessage  = _localizer["FailedToLoadPromoArtImage"];
        }
    }
}
