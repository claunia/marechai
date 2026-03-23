#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class SoftwareReleaseViewViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareBrowsingService               _browsingService;
    private readonly IStringLocalizer                      _localizer;
    private readonly ILogger<SoftwareReleaseViewViewModel> _logger;
    private readonly IRegionManager                        _regionManager;

    private int _sourceSoftwareId;

    [ObservableProperty]
    private string _releaseTitle = string.Empty;

    [ObservableProperty]
    private string? _softwareName;

    [ObservableProperty]
    private string? _versionString;

    [ObservableProperty]
    private string? _platform;

    [ObservableProperty]
    private string? _variant;

    [ObservableProperty]
    private string? _subvariant;

    [ObservableProperty]
    private string? _region;

    [ObservableProperty]
    private string? _publisher;

    [ObservableProperty]
    private string? _releaseDateDisplay;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private Visibility _showPlatform = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showVariant = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showSubvariant = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showRegion = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPublisher = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showReleaseDate = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showBarcodes = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showProductCodes = Visibility.Collapsed;

    public SoftwareReleaseViewViewModel(ILogger<SoftwareReleaseViewViewModel> logger,
                                        IRegionManager                        regionManager,
                                        SoftwareBrowsingService               browsingService,
                                        IStringLocalizer                      localizer)
    {
        _logger          = logger;
        _regionManager   = regionManager;
        _browsingService = browsingService;
        _localizer       = localizer;
    }

    public ObservableCollection<string> Barcodes     { get; } = [];
    public ObservableCollection<string> ProductCodes { get; } = [];

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareId, out int softwareId))
            _sourceSoftwareId = softwareId;

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareReleaseId, out int releaseId))
            _ = LoadReleaseAsync(releaseId);
    }

    [RelayCommand]
    public Task GoBack()
    {
        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, _sourceSoftwareId },
            { NavParamKeys.NavigationSource, nameof(SoftwareListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task LoadData()
    {
        HasError     = false;
        ErrorMessage = string.Empty;

        return Task.CompletedTask;
    }

    private async Task LoadReleaseAsync(int releaseId)
    {
        try
        {
            IsLoading    = true;
            IsDataLoaded = false;
            HasError     = false;
            ErrorMessage = string.Empty;
            Barcodes.Clear();
            ProductCodes.Clear();

            SoftwareReleaseDto? release = await _browsingService.GetReleaseByIdAsync(releaseId);

            if(release is null)
            {
                HasError     = true;
                ErrorMessage = _localizer["Release not found"];
                IsLoading    = false;

                return;
            }

            VersionString      = release.SoftwareVersion;
            Platform           = release.Platform;
            Variant            = release.Variant;
            Subvariant         = release.Subvariant;
            Region             = release.Region;
            Publisher          = release.Publisher;

            if(release.ReleaseDate.HasValue)
                ReleaseDateDisplay = release.ReleaseDate.Value.DateTime.ToString("MMMM d, yyyy");

            // Build title
            ReleaseTitle = VersionString ?? _localizer["Software Release"];

            // Load software name for display
            if(_sourceSoftwareId > 0)
            {
                SoftwareDto? software = await _browsingService.GetSoftwareByIdAsync(_sourceSoftwareId);
                SoftwareName = software?.Name;
            }

            // Load barcodes
            List<SoftwareBarcodeDto> barcodes = await _browsingService.GetBarcodesAsync(releaseId);

            foreach(SoftwareBarcodeDto barcode in barcodes)
                Barcodes.Add($"{barcode.Code} ({barcode.Type})");

            // Load product codes
            List<SoftwareProductCodeDto> productCodes = await _browsingService.GetProductCodesAsync(releaseId);

            foreach(SoftwareProductCodeDto pc in productCodes)
                ProductCodes.Add($"{pc.Code} ({pc.Issuer})");

            UpdateVisibilities();
            IsDataLoaded = true;
            IsLoading    = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading release {ReleaseId}", releaseId);
            HasError     = true;
            ErrorMessage = ex.Message;
            IsLoading    = false;
        }
    }

    private void UpdateVisibilities()
    {
        ShowPlatform    = !string.IsNullOrEmpty(Platform) ? Visibility.Visible : Visibility.Collapsed;
        ShowVariant     = !string.IsNullOrEmpty(Variant) ? Visibility.Visible : Visibility.Collapsed;
        ShowSubvariant  = !string.IsNullOrEmpty(Subvariant) ? Visibility.Visible : Visibility.Collapsed;
        ShowRegion      = !string.IsNullOrEmpty(Region) ? Visibility.Visible : Visibility.Collapsed;
        ShowPublisher   = !string.IsNullOrEmpty(Publisher) ? Visibility.Visible : Visibility.Collapsed;
        ShowReleaseDate = !string.IsNullOrEmpty(ReleaseDateDisplay) ? Visibility.Visible : Visibility.Collapsed;
        ShowBarcodes    = Barcodes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowProductCodes = ProductCodes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}
