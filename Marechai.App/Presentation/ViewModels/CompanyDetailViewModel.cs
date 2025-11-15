#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Helpers;
using Marechai.App.Presentation.Models;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Marechai.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Uno.Extensions.Navigation;

namespace Marechai.App.Presentation.ViewModels;

public partial class CompanyDetailViewModel : ObservableObject
{
    private readonly CompanyDetailService            _companyDetailService;
    private readonly FlagCache                       _flagCache;
    private readonly IStringLocalizer                _localizer;
    private readonly ILogger<CompanyDetailViewModel> _logger;
    private readonly CompanyLogoCache                _logoCache;
    private readonly INavigator                      _navigator;

    [ObservableProperty]
    private CompanyDto? _company;

    [ObservableProperty]
    private int _companyId;

    [ObservableProperty]
    private ObservableCollection<CompanyLogoItem> _companyLogos = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _computers = [];

    [ObservableProperty]
    private string _computersFilterText = string.Empty;

    [ObservableProperty]
    private string _consoelsFilterText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _consoles = [];

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _filteredComputers = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _filteredConsoles = [];

    [ObservableProperty]
    private SvgImageSource? _flagImageSource;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private SvgImageSource? _logoImageSource;

    [ObservableProperty]
    private CompanyDto? _soldToCompany;

    public CompanyDetailViewModel(CompanyDetailService            companyDetailService, FlagCache        flagCache,
                                  CompanyLogoCache                logoCache,            IStringLocalizer localizer,
                                  ILogger<CompanyDetailViewModel> logger,               INavigator       navigator)
    {
        _companyDetailService    = companyDetailService;
        _flagCache               = flagCache;
        _logoCache               = logoCache;
        _localizer               = localizer;
        _logger                  = logger;
        _navigator               = navigator;
        LoadData                 = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand            = new AsyncRelayCommand(GoBackAsync);
        NavigateToMachineCommand = new AsyncRelayCommand<CompanyDetailMachine>(NavigateToMachineAsync);
    }

    /// <summary>
    ///     Gets the display text for the company's status
    /// </summary>
    public string CompanyStatusDisplay => Company != null ? GetStatusMessage(Company) : string.Empty;

    /// <summary>
    ///     Gets the display text for the company's founded date
    /// </summary>
    public string CompanyFoundedDateDisplay => Company != null ? GetFoundedDateDisplay(Company) : string.Empty;

    /// <summary>
    ///     Gets whether flag content is available
    /// </summary>
    public bool HasFlagContent => FlagImageSource != null;

    /// <summary>
    ///     Gets whether logo content is available
    /// </summary>
    public bool HasLogoContent => LogoImageSource != null;

    /// <summary>
    ///     Gets whether company has multiple logos
    /// </summary>
    public bool HasMultipleLogos => CompanyLogos.Count > 1;

    public IAsyncRelayCommand                       LoadData                 { get; }
    public ICommand                                 GoBackCommand            { get; }
    public IAsyncRelayCommand<CompanyDetailMachine> NavigateToMachineCommand { get; }
    public string                                   Title                    { get; } = "Company Details";

    partial void OnCompanyChanged(CompanyDto? oldValue, CompanyDto? newValue)
    {
        // Notify that computed properties have changed
        OnPropertyChanged(nameof(CompanyStatusDisplay));
        OnPropertyChanged(nameof(CompanyFoundedDateDisplay));
    }

    partial void OnFlagImageSourceChanged(SvgImageSource? oldValue, SvgImageSource? newValue)
    {
        // Notify that HasFlagContent has changed
        OnPropertyChanged(nameof(HasFlagContent));
    }

    partial void OnLogoImageSourceChanged(SvgImageSource? oldValue, SvgImageSource? newValue)
    {
        // Notify that HasLogoContent has changed
        OnPropertyChanged(nameof(HasLogoContent));
    }

    partial void OnCompanyLogosChanged(ObservableCollection<CompanyLogoItem>? oldValue,
                                       ObservableCollection<CompanyLogoItem> newValue)
    {
        // Notify that HasMultipleLogos has changed
        OnPropertyChanged(nameof(HasMultipleLogos));
    }

    partial void OnComputersFilterTextChanged(string value)
    {
        FilterComputers(value);
    }

    partial void OnConsoelsFilterTextChanged(string value)
    {
        FilterConsoles(value);
    }

    private void FilterComputers(string filterText)
    {
        ObservableCollection<CompanyDetailMachine> filtered = string.IsNullOrWhiteSpace(filterText)
                                                                  ? new ObservableCollection<
                                                                      CompanyDetailMachine>(Computers)
                                                                  : new
                                                                      ObservableCollection<
                                                                          CompanyDetailMachine>(Computers.Where(c =>
                                                                          c.Name.Contains(filterText,
                                                                              StringComparison
                                                                                 .OrdinalIgnoreCase)));

        FilteredComputers = filtered;
    }

    private void FilterConsoles(string filterText)
    {
        ObservableCollection<CompanyDetailMachine> filtered = string.IsNullOrWhiteSpace(filterText)
                                                                  ? new ObservableCollection<
                                                                      CompanyDetailMachine>(Consoles)
                                                                  : new
                                                                      ObservableCollection<
                                                                          CompanyDetailMachine>(Consoles.Where(c =>
                                                                          c.Name.Contains(filterText,
                                                                              StringComparison
                                                                                 .OrdinalIgnoreCase)));

        FilteredConsoles = filtered;
    }

    private async Task NavigateToMachineAsync(CompanyDetailMachine? machine)
    {
        if(machine == null) return;

        var navParam = new MachineViewNavigationParameter
        {
            MachineId        = machine.Id,
            NavigationSource = this
        };

        await _navigator.NavigateViewModelAsync<MachineViewViewModel>(this, data: navParam);
    }

    /// <summary>
    ///     Gets the formatted founding date with unknown handling
    /// </summary>
    public string GetFoundedDateDisplay(CompanyDto company)
    {
        if(company.Founded is null) return string.Empty;

        DateTime date = company.Founded.Value.DateTime;

        if(company.FoundedMonthIsUnknown ?? false) return $"{date.Year}.";

        if(company.FoundedDayIsUnknown ?? false) return $"{date:Y}.";

        return $"{date:D}.";
    }

    /// <summary>
    ///     Gets the formatted sold/event date with unknown handling
    /// </summary>
    public string GetEventDateDisplay(CompanyDto? company, bool monthUnknown = false, bool dayUnknown = false)
    {
        if(company?.Sold is null) return _localizer["unknown date"].Value;

        DateTime date = company.Sold.Value.DateTime;

        if(monthUnknown || (company.SoldMonthIsUnknown ?? false)) return $"{date.Year}";

        if(dayUnknown || (company.SoldDayIsUnknown ?? false)) return $"{date:Y}";

        return $"{date:D}";
    }

    /// <summary>
    ///     Gets the status message for the company
    /// </summary>
    public string GetStatusMessage(CompanyDto company)
    {
        return company.Status switch
               {
                   1 => _localizer["Company is active."].Value,
                   2 => GetSoldStatusMessage(company),
                   3 => GetMergedStatusMessage(company),
                   4 => GetBankruptcyMessage(company),
                   5 => GetDefunctMessage(company),
                   6 => GetRenamedStatusMessage(company),
                   _ => _localizer["Current company status is unknown."].Value
               };
    }

    private string GetSoldStatusMessage(CompanyDto company)
    {
        if(SoldToCompany != null)
        {
            return string.Format(_localizer["Company sold to {0} on {1}."].Value,
                                 SoldToCompany.Name,
                                 GetEventDateDisplay(company));
        }

        if(company.Sold != null)
        {
            return string.Format(_localizer["Company sold on {0} to an unknown company."].Value,
                                 GetEventDateDisplay(company));
        }

        return SoldToCompany != null
                   ? string.Format(_localizer["Company sold to {0} on an unknown date."].Value, SoldToCompany.Name)
                   : _localizer["Company was sold to an unknown company on an unknown date."].Value;
    }

    private string GetMergedStatusMessage(CompanyDto company)
    {
        if(SoldToCompany != null)
        {
            return string.Format(_localizer["Company merged on {0} to form {1}."].Value,
                                 GetEventDateDisplay(company),
                                 SoldToCompany.Name);
        }

        if(company.Sold != null)
        {
            return string.Format(_localizer["Company merged on {0} to form an unknown company."].Value,
                                 GetEventDateDisplay(company));
        }

        return SoldToCompany != null
                   ? string.Format(_localizer["Company merged on an unknown date to form {0}."].Value,
                                   SoldToCompany.Name)
                   : _localizer["Company merged to form an unknown company on an unknown date."].Value;
    }

    private string GetBankruptcyMessage(CompanyDto company) => company.Sold != null
                                                                   ? string.Format(_localizer
                                                                               ["Company declared bankruptcy on {0}."]
                                                                          .Value,
                                                                       GetEventDateDisplay(company))
                                                                   : _localizer
                                                                           ["Company declared bankruptcy on an unknown date."]
                                                                      .Value;

    private string GetDefunctMessage(CompanyDto company) => company.Sold != null
                                                                ? string.Format(_localizer
                                                                                            ["Company ceased operations on {0}."]
                                                                                       .Value,
                                                                                    GetEventDateDisplay(company))
                                                                : _localizer
                                                                        ["Company ceased operations on an unknown date."]
                                                                   .Value;

    private string GetRenamedStatusMessage(CompanyDto company)
    {
        if(SoldToCompany != null)
        {
            return string.Format(_localizer["Company renamed to {0} on {1}."].Value,
                                 SoldToCompany.Name,
                                 GetEventDateDisplay(company));
        }

        if(company.Sold != null)
        {
            return string.Format(_localizer["Company was renamed on {0} to an unknown name."].Value,
                                 GetEventDateDisplay(company));
        }

        return SoldToCompany != null
                   ? string.Format(_localizer["Company renamed to {0} on an unknown date."].Value, SoldToCompany.Name)
                   : _localizer["Company renamed to an unknown name on an unknown date."].Value;
    }

    /// <summary>
    ///     Loads company details from the API
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading       = true;
            ErrorMessage    = string.Empty;
            HasError        = false;
            IsDataLoaded    = false;
            FlagImageSource = null;
            LogoImageSource = null;
            CompanyLogos.Clear();

            if(CompanyId <= 0)
            {
                ErrorMessage = _localizer["Invalid company ID."].Value;
                HasError     = true;

                return;
            }

            // Load company details
            Company = await _companyDetailService.GetCompanyByIdAsync(CompanyId);

            if(Company is null)
            {
                ErrorMessage = _localizer["Company not found."].Value;
                HasError     = true;

                return;
            }

            // Load flag if country is available
            if(Company.CountryId is not null)
            {
                try
                {
                    var     countryCode = (short)UntypedNodeExtractor.ExtractInt(Company.CountryId);
                    Stream? flagStream  = await _flagCache.GetFlagAsync(countryCode);

                    var flagSource = new SvgImageSource();
                    await flagSource.SetSourceAsync(flagStream.AsRandomAccessStream());
                    FlagImageSource = flagSource;

                    _logger.LogInformation("Successfully loaded flag for country code {CountryCode}", countryCode);
                }
                catch(Exception ex)
                {
                    _logger.LogError("Failed to load flag for country {CountryId}: {Exception}",
                                     Company.CountryId,
                                     ex.Message);

                    // Continue without flag if loading fails
                }
            }

            if(Company.SoldToId != null)
            {
                int soldToId                   = UntypedNodeExtractor.ExtractInt(Company.SoldToId);
                if(soldToId > 0) SoldToCompany = await _companyDetailService.GetSoldToCompanyAsync(soldToId);
            }

            // Load logo if available
            if(Company.LastLogo.HasValue)
            {
                try
                {
                    Stream? logoStream = await _logoCache.GetLogoAsync(Company.LastLogo.Value);

                    var logoSource = new SvgImageSource();
                    await logoSource.SetSourceAsync(logoStream.AsRandomAccessStream());
                    LogoImageSource = logoSource;

                    _logger.LogInformation("Successfully loaded logo for company {CompanyId}", CompanyId);
                }
                catch(Exception ex)
                {
                    _logger.LogError("Failed to load logo for company {CompanyId}: {Exception}", CompanyId, ex.Message);

                    // Continue without logo if loading fails
                }
            }

            // Load all logos for carousel
            try
            {
                // Get all logos for this company
                List<CompanyLogoDto> logosList = await _companyDetailService.GetCompanyLogosAsync(CompanyId);

                // Convert to list with extracted years for sorting
                var logosWithYears = logosList.Select(logo => new
                                               {
                                                   Logo = logo,
                                                   Year = logo.Year != null
                                                              ? (int?)UntypedNodeExtractor.ExtractInt(logo.Year)
                                                              : null
                                               })
                                              .OrderBy(l => l.Year)
                                              .ToList();

                var loadedLogos = new ObservableCollection<CompanyLogoItem>();

                foreach(var logoData in logosWithYears)
                {
                    try
                    {
                        if(logoData.Logo.Guid == null) continue;

                        Stream? logoStream = await _logoCache.GetLogoAsync(logoData.Logo.Guid.Value);
                        var     logoSource = new SvgImageSource();
                        await logoSource.SetSourceAsync(logoStream.AsRandomAccessStream());

                        loadedLogos.Add(new CompanyLogoItem
                        {
                            LogoGuid   = logoData.Logo.Guid.Value,
                            LogoSource = logoSource,
                            Year       = logoData.Year
                        });
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError("Failed to load carousel logo: {Exception}", ex.Message);
                    }
                }

                // Assign the new collection (this will trigger OnCompanyLogosChanged)
                CompanyLogos = loadedLogos;

                _logger.LogInformation("Loaded {Count} logos for company {CompanyId}", CompanyLogos.Count, CompanyId);
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load company logos for carousel: {Exception}", ex.Message);
            }

            // Load computers and consoles made by this company
            List<MachineDto> machines = await _companyDetailService.GetComputersByCompanyAsync(CompanyId);
            Computers.Clear();
            Consoles.Clear();
            FilteredComputers.Clear();
            FilteredConsoles.Clear();

            foreach(MachineDto machine in machines)
            {
                int machineId = UntypedNodeExtractor.ExtractInt(machine.Id);

                var machineItem = new CompanyDetailMachine
                {
                    Id   = machineId,
                    Name = machine.Name ?? string.Empty
                };

                // Categorize by machine type enum
                if(machine.Type == (int)MachineType.Computer)
                    Computers.Add(machineItem);
                else if(machine.Type == (int)MachineType.Console) Consoles.Add(machineItem);
            }

            // Initialize filtered lists
            FilteredComputers = new ObservableCollection<CompanyDetailMachine>(Computers);
            FilteredConsoles  = new ObservableCollection<CompanyDetailMachine>(Consoles);

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading company details: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load company details. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Handles back navigation
    /// </summary>
    private async Task GoBackAsync()
    {
        await _navigator.NavigateViewModelAsync<CompaniesViewModel>(this);
    }
}

/// <summary>
///     Data model for a machine in the company detail view
/// </summary>
public class CompanyDetailMachine
{
    public int    Id   { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
///     Data model for a company logo in the carousel
/// </summary>
public class CompanyLogoItem
{
    public Guid            LogoGuid   { get; set; }
    public SvgImageSource? LogoSource { get; set; }
    public int?            Year       { get; set; }
}