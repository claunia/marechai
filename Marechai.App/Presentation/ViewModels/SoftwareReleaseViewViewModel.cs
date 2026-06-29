#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Humanizer;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.Data;
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

    private int _sourceCompilationId;
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
    private string? _regionsDisplay;

    [ObservableProperty]
    private string? _languagesDisplay;

    [ObservableProperty]
    private string? _title;

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
    private Visibility _showRegions = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showLanguages = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showTitle = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showPublisher = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showReleaseDate = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showBarcodes = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showProductCodes = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _showCompanies = Visibility.Collapsed;

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

    public ObservableCollection<string> Barcodes         { get; } = [];
    public ObservableCollection<string> ProductCodes     { get; } = [];
    public ObservableCollection<string> Companies        { get; } = [];

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareId, out int softwareId))
            _sourceSoftwareId = softwareId;

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SourceSoftwareCompilationId,
                                                         out int sourceCompilationId))
            _sourceCompilationId = sourceCompilationId;

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareReleaseId, out int releaseId))
            _ = LoadReleaseAsync(releaseId);
    }

    [RelayCommand]
    public Task GoBack()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        if(_sourceCompilationId > 0)
        {
            var compilationParameters = new NavigationParameters
            {
                { NavParamKeys.SoftwareCompilationId, _sourceCompilationId },
                { NavParamKeys.NavigationSource, nameof(SoftwareReleaseViewViewModel) }
            };

            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareCompilationViewPage),
                                           compilationParameters);

            return Task.CompletedTask;
        }

        if(_sourceSoftwareId <= 0)
        {
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwarePage));

            return Task.CompletedTask;
        }

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
            Companies.Clear();

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
            RegionsDisplay     = release.Regions is { Count: > 0 }
                ? string.Join(", ", release.Regions.Select(r => r.RegionName))
                : null;
            LanguagesDisplay   = release.Languages is { Count: > 0 }
                ? string.Join(", ", release.Languages.Select(l => l.Language))
                : null;
            Publisher          = release.Publisher;

            if(release.ReleaseDate.HasValue)
                ReleaseDateDisplay = DatePrecisionFormatter.Format(release.ReleaseDate, release.ReleaseDatePrecision);

            if(!string.IsNullOrEmpty(VersionString))
            {
                ReleaseTitle = VersionString;
            }
            else
            {
                ReleaseTitle = release.Software ?? _localizer["Software Release"];
            }

            if(!string.IsNullOrEmpty(release.Title))
                Title = release.Title;

            if(!string.IsNullOrEmpty(release.Software))
            {
                SoftwareName = release.Software;
            }
            else if(_sourceSoftwareId > 0)
            {
                SoftwareDto? software = await _browsingService.GetSoftwareByIdAsync(_sourceSoftwareId);
                SoftwareName = software?.Name;
            }

            // Load barcodes
            List<SoftwareBarcodeDto> barcodes = await _browsingService.GetBarcodesAsync(releaseId);

            foreach(SoftwareBarcodeDto barcode in barcodes)
                Barcodes.Add($"{barcode.Code} ({((BarcodeType)(barcode.Type ?? 0)).Humanize()})");

            // Load product codes
            List<SoftwareProductCodeDto> productCodes = await _browsingService.GetProductCodesAsync(releaseId);

            foreach(SoftwareProductCodeDto pc in productCodes)
                ProductCodes.Add($"{pc.Code} ({((ProductCodeIssuer)(pc.Issuer ?? 0)).Humanize()})");

            // Load all companies from all junction levels
            await LoadAllCompaniesAsync(release);

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
        ShowRegions     = !string.IsNullOrEmpty(RegionsDisplay) ? Visibility.Visible : Visibility.Collapsed;
        ShowLanguages   = !string.IsNullOrEmpty(LanguagesDisplay) ? Visibility.Visible : Visibility.Collapsed;
        ShowTitle       = !string.IsNullOrEmpty(Title) ? Visibility.Visible : Visibility.Collapsed;
        ShowPublisher   = !string.IsNullOrEmpty(Publisher) ? Visibility.Visible : Visibility.Collapsed;
        ShowReleaseDate = !string.IsNullOrEmpty(ReleaseDateDisplay) ? Visibility.Visible : Visibility.Collapsed;
        ShowBarcodes    = Barcodes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowProductCodes = ProductCodes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowCompanies   = Companies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async Task LoadAllCompaniesAsync(SoftwareReleaseDto release)
    {
        var companySet = new HashSet<(int companyId, string roleId)>();
        var companyDisplays = new List<(string company, string? role)>();

        // 1. Companies from the software version
        if(release.SoftwareVersionId.HasValue)
        {
            int versionId = release.SoftwareVersionId.Value;

            List<CompanyBySoftwareVersionDto> versionCompanies =
                await _browsingService.GetCompaniesByVersionAsync(versionId);

            foreach(CompanyBySoftwareVersionDto c in versionCompanies)
            {
                var key = (c.CompanyId ?? 0, c.RoleId ?? string.Empty);

                if(companySet.Add(key))
                    companyDisplays.Add((c.Company ?? string.Empty, c.Role));
            }

            // 2. Companies from the software itself
            SoftwareVersionDto? version = await _browsingService.GetVersionByIdAsync(versionId);

            if(version?.SoftwareId is > 0)
            {
                int softwareId = version.SoftwareId.Value;

                List<SoftwareCompanyRoleDto> softwareCompanies =
                    await _browsingService.GetCompaniesAsync(softwareId);

                foreach(SoftwareCompanyRoleDto c in softwareCompanies)
                {
                    var key = (c.CompanyId ?? 0, c.RoleId ?? string.Empty);

                    if(companySet.Add(key))
                        companyDisplays.Add((c.Company ?? string.Empty, c.Role));
                }

                // 3. Companies from the software family
                SoftwareDto? software = await _browsingService.GetSoftwareByIdAsync(softwareId);

                if(software?.FamilyId is > 0)
                {
                    List<CompanyBySoftwareFamilyDto> familyCompanies =
                        await _browsingService.GetCompaniesByFamilyAsync(software.FamilyId.Value);

                    foreach(CompanyBySoftwareFamilyDto c in familyCompanies)
                    {
                        var key = (c.CompanyId ?? 0, c.RoleId ?? string.Empty);

                        if(companySet.Add(key))
                            companyDisplays.Add((c.Company ?? string.Empty, c.Role));
                    }
                }
            }
        }
        else if(release.SoftwareId is > 0)
        {
            // Versionless single release: load companies from software + family
            int softwareId = release.SoftwareId.Value;

            List<SoftwareCompanyRoleDto> softwareCompanies =
                await _browsingService.GetCompaniesAsync(softwareId);

            foreach(SoftwareCompanyRoleDto c in softwareCompanies)
            {
                var key = (c.CompanyId ?? 0, c.RoleId ?? string.Empty);

                if(companySet.Add(key))
                    companyDisplays.Add((c.Company ?? string.Empty, c.Role));
            }

            SoftwareDto? software = await _browsingService.GetSoftwareByIdAsync(softwareId);

            if(software?.FamilyId is > 0)
            {
                List<CompanyBySoftwareFamilyDto> familyCompanies =
                    await _browsingService.GetCompaniesByFamilyAsync(software.FamilyId.Value);

                foreach(CompanyBySoftwareFamilyDto c in familyCompanies)
                {
                    var key = (c.CompanyId ?? 0, c.RoleId ?? string.Empty);

                    if(companySet.Add(key))
                        companyDisplays.Add((c.Company ?? string.Empty, c.Role));
                }
            }
        }

        // Sort by company name and add to collection
        foreach((string company, string? role) in companyDisplays.OrderBy(c => c.company))
        {
            string localizedRole = !string.IsNullOrEmpty(role) ? _localizer[role] : null;
            string display = !string.IsNullOrEmpty(localizedRole) ? $"{company} ({localizedRole})" : company;
            Companies.Add(display);
        }
    }
}
