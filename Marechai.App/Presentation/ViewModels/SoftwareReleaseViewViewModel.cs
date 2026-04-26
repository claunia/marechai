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
    private string? _regionsDisplay;

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
    private Visibility _showRegions = Visibility.Collapsed;

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

    [ObservableProperty]
    private Visibility _showIncludedVersions = Visibility.Collapsed;

    [ObservableProperty]
    private bool _isCompilation;

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
    public ObservableCollection<string> IncludedVersions { get; } = [];

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
            Companies.Clear();
            IncludedVersions.Clear();

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
            RegionsDisplay     = release.Regions is { Count: > 0 }
                ? string.Join(", ", release.Regions.Select(r => r.RegionName))
                : null;
            Publisher          = release.Publisher;

            if(release.ReleaseDate.HasValue)
                ReleaseDateDisplay = (release.ReleaseDatePrecision ?? 0) == 2 ? $"{release.ReleaseDate.Value.Year}" : (release.ReleaseDatePrecision ?? 0) == 1 ? release.ReleaseDate.Value.ToString("MMMM yyyy") : release.ReleaseDate.Value.DateTime.ToString("MMMM d, yyyy");

            // Determine if this is a compilation
            IsCompilation = release.IsCompilation == true;

            if(IsCompilation)
            {
                // Compilation: use Title or fallback
                ReleaseTitle = release.Title ?? _localizer["Compilation"];

                // Load included versions (versioned compilations)
                List<SoftwareVersionBySoftwareReleaseDto> includedVersions =
                    await _browsingService.GetIncludedVersionsAsync(releaseId);

                foreach(SoftwareVersionBySoftwareReleaseDto iv in includedVersions)
                    IncludedVersions.Add($"{iv.SoftwareName} — {iv.SoftwareVersion}");

                // Load included software (versionless compilations)
                List<SoftwareBySoftwareReleaseDto> includedSoftware =
                    await _browsingService.GetIncludedSoftwareAsync(releaseId);

                foreach(SoftwareBySoftwareReleaseDto sw in includedSoftware)
                    IncludedVersions.Add(sw.SoftwareName ?? string.Empty);
            }
            else
            {
                // Single release: build title from version string or software name
                if(!string.IsNullOrEmpty(VersionString))
                {
                    ReleaseTitle = VersionString;
                }
                else
                {
                    // Versionless single release: use software name from DTO
                    ReleaseTitle = release.Software ?? _localizer["Software Release"];
                }

                // Load software name for display
                if(!string.IsNullOrEmpty(release.Software))
                {
                    SoftwareName = release.Software;
                }
                else if(_sourceSoftwareId > 0)
                {
                    SoftwareDto? software = await _browsingService.GetSoftwareByIdAsync(_sourceSoftwareId);
                    SoftwareName = software?.Name;
                }
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
        ShowVariant     = !string.IsNullOrEmpty(Variant) ? Visibility.Visible : Visibility.Collapsed;
        ShowSubvariant  = !string.IsNullOrEmpty(Subvariant) ? Visibility.Visible : Visibility.Collapsed;
        ShowRegions     = !string.IsNullOrEmpty(RegionsDisplay) ? Visibility.Visible : Visibility.Collapsed;
        ShowPublisher   = !string.IsNullOrEmpty(Publisher) ? Visibility.Visible : Visibility.Collapsed;
        ShowReleaseDate = !string.IsNullOrEmpty(ReleaseDateDisplay) ? Visibility.Visible : Visibility.Collapsed;
        ShowBarcodes    = Barcodes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowProductCodes = ProductCodes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowCompanies   = Companies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowIncludedVersions = IncludedVersions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
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

        // 4. Companies from the variant
        if(release.VariantId is > 0)
        {
            List<CompanyBySoftwareVariantDto> variantCompanies =
                await _browsingService.GetCompaniesByVariantAsync(release.VariantId.Value);

            foreach(CompanyBySoftwareVariantDto c in variantCompanies)
            {
                var key = (c.CompanyId ?? 0, c.RoleId ?? string.Empty);

                if(companySet.Add(key))
                    companyDisplays.Add((c.Company ?? string.Empty, c.Role));
            }
        }

        // Sort by company name and add to collection
        foreach((string company, string? role) in companyDisplays.OrderBy(c => c.company))
        {
            string display = !string.IsNullOrEmpty(role) ? $"{company} ({role})" : company;
            Companies.Add(display);
        }
    }
}
