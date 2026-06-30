#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Marechai.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Marechai.App.Presentation.ViewModels;

public partial class CompanyDetailViewModel : ObservableObject, IRegionAware
{
    private readonly CompanyDetailService            _companyDetailService;
    private readonly FlagCache                       _flagCache;
    private readonly ImageSourceFactory              _imageSourceFactory;
    private readonly IStringLocalizer                _localizer;
    private readonly ILogger<CompanyDetailViewModel> _logger;
    private readonly CompanyLogoCache                _logoCache;
    private readonly IRegionManager                  _regionManager;

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
    private ObservableCollection<CompanyDetailMachine> _consoles = [];

    [ObservableProperty]
    private string _consolesFilterText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _smartphones = [];

    [ObservableProperty]
    private string _smartphonesFilterText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _tablets = [];

    [ObservableProperty]
    private string _tabletsFilterText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _pdas = [];

    [ObservableProperty]
    private string _pdasFilterText = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _filteredComputers = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _filteredConsoles = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _filteredSmartphones = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _filteredTablets = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailMachine> _filteredPdas = [];

    [ObservableProperty]
    private BitmapImage? _flagImageSource;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private BitmapImage? _logoImageSource;

    [ObservableProperty]
    private CompanyDto? _soldToCompany;

    [ObservableProperty]
    private string _descriptionMarkdown = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyDetailItem> _people = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailItem> _machineFamilies = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailItem> _gpus = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailItem> _processors = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailItem> _soundSynths = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailItem> _software = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailItem> _books = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailItem> _documents = [];

    [ObservableProperty]
    private ObservableCollection<CompanyDetailItem> _magazines = [];

    public bool ShowPeople => People.Count > 0;

    public CompanyDetailViewModel(CompanyDetailService            companyDetailService, FlagCache          flagCache,
                                  CompanyLogoCache                logoCache,            IStringLocalizer   localizer,
                                  ILogger<CompanyDetailViewModel> logger,               IRegionManager     regionManager,
                                  ImageSourceFactory              imageSourceFactory)
    {
        _companyDetailService    = companyDetailService;
        _flagCache               = flagCache;
        _logoCache               = logoCache;
        _localizer               = localizer;
        _logger                  = logger;
        _regionManager           = regionManager;
        _imageSourceFactory      = imageSourceFactory;
        LoadData                      = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand                 = new AsyncRelayCommand(GoBackAsync);
        NavigateToMachineCommand      = new AsyncRelayCommand<CompanyDetailMachine>(NavigateToMachineAsync);
        NavigateToSoldToCompanyCommand = new AsyncRelayCommand(NavigateToSoldToCompanyAsync);
        NavigateToMachineFamilyCommand = new AsyncRelayCommand<CompanyDetailItem>(item =>
            NavigateToDetailAsync(item, nameof(MachineFamilyViewPage), NavParamKeys.MachineFamilyId));
        NavigateToGpuCommand = new AsyncRelayCommand<CompanyDetailItem>(item =>
            NavigateToDetailAsync(item, nameof(GpuDetailPage), NavParamKeys.GpuId));
        NavigateToProcessorCommand = new AsyncRelayCommand<CompanyDetailItem>(item =>
            NavigateToDetailAsync(item, nameof(ProcessorDetailPage), NavParamKeys.ProcessorId));
        NavigateToSoundSynthCommand = new AsyncRelayCommand<CompanyDetailItem>(item =>
            NavigateToDetailAsync(item, nameof(SoundSynthDetailPage), NavParamKeys.SoundSynthId));
        NavigateToSoftwareCommand = new AsyncRelayCommand<CompanyDetailItem>(item =>
            NavigateToDetailAsync(item, nameof(SoftwareViewPage), NavParamKeys.SoftwareId));
        NavigateToBookCommand = new AsyncRelayCommand<CompanyDetailItem>(item =>
            NavigateToDetailAsync(item, nameof(BookViewPage), NavParamKeys.BookId, useLongId: true));
        NavigateToDocumentCommand = new AsyncRelayCommand<CompanyDetailItem>(item =>
            NavigateToDetailAsync(item, nameof(DocumentViewPage), NavParamKeys.DocumentId, useLongId: true));
        NavigateToMagazineCommand = new AsyncRelayCommand<CompanyDetailItem>(item =>
            NavigateToDetailAsync(item, nameof(MagazineViewPage), NavParamKeys.MagazineId, useLongId: true));
        NavigateToPersonCommand = new AsyncRelayCommand<CompanyDetailItem>(item =>
            NavigateToDetailAsync(item, nameof(PersonViewPage), NavParamKeys.PersonId));
        Title                    = _localizer["Company Details"];
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
    ///     Gets whether a description is available
    /// </summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(DescriptionMarkdown);

    /// <summary>
    ///     Gets whether company has multiple logos
    /// </summary>
    public bool HasMultipleLogos => CompanyLogos.Count > 1;

    public IAsyncRelayCommand                       LoadData                       { get; }
    public ICommand                                 GoBackCommand                  { get; }
    public IAsyncRelayCommand<CompanyDetailMachine> NavigateToMachineCommand       { get; }
    public IAsyncRelayCommand                       NavigateToSoldToCompanyCommand { get; }
    public IAsyncRelayCommand<CompanyDetailItem>    NavigateToMachineFamilyCommand { get; }
    public IAsyncRelayCommand<CompanyDetailItem>    NavigateToGpuCommand           { get; }
    public IAsyncRelayCommand<CompanyDetailItem>    NavigateToProcessorCommand     { get; }
    public IAsyncRelayCommand<CompanyDetailItem>    NavigateToSoundSynthCommand    { get; }
    public IAsyncRelayCommand<CompanyDetailItem>    NavigateToSoftwareCommand      { get; }
    public IAsyncRelayCommand<CompanyDetailItem>    NavigateToBookCommand          { get; }
    public IAsyncRelayCommand<CompanyDetailItem>    NavigateToDocumentCommand      { get; }
    public IAsyncRelayCommand<CompanyDetailItem>    NavigateToMagazineCommand      { get; }
    public IAsyncRelayCommand<CompanyDetailItem>    NavigateToPersonCommand        { get; }
    public string                                   Title                          { get; }

    partial void OnCompanyChanged(CompanyDto? oldValue, CompanyDto? newValue)
    {
        // Notify that computed properties have changed
        OnPropertyChanged(nameof(CompanyStatusDisplay));
        OnPropertyChanged(nameof(CompanyFoundedDateDisplay));
    }

    partial void OnFlagImageSourceChanged(BitmapImage? oldValue, BitmapImage? newValue)
    {
        // Notify that HasFlagContent has changed
        OnPropertyChanged(nameof(HasFlagContent));
    }

    partial void OnLogoImageSourceChanged(BitmapImage? oldValue, BitmapImage? newValue)
    {
        // Notify that HasLogoContent has changed
        OnPropertyChanged(nameof(HasLogoContent));
    }

    partial void OnDescriptionMarkdownChanged(string value)
    {
        OnPropertyChanged(nameof(HasDescription));
    }

    partial void OnCompanyLogosChanged(ObservableCollection<CompanyLogoItem>? oldValue,
                                       ObservableCollection<CompanyLogoItem>  newValue)
    {
        // Notify that HasMultipleLogos has changed
        OnPropertyChanged(nameof(HasMultipleLogos));
    }

    partial void OnComputersFilterTextChanged(string value)
    {
        FilterComputers(value);
    }

    partial void OnConsolesFilterTextChanged(string value)
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

    partial void OnSmartphonesFilterTextChanged(string value)
    {
        FilterSmartphones(value);
    }

    private void FilterSmartphones(string filterText)
    {
        ObservableCollection<CompanyDetailMachine> filtered = string.IsNullOrWhiteSpace(filterText)
                                                                  ? new ObservableCollection<
                                                                      CompanyDetailMachine>(Smartphones)
                                                                  : new
                                                                      ObservableCollection<
                                                                          CompanyDetailMachine>(Smartphones.Where(c =>
                                                                          c.Name.Contains(filterText,
                                                                              StringComparison
                                                                                 .OrdinalIgnoreCase)));

        FilteredSmartphones = filtered;
    }

    partial void OnTabletsFilterTextChanged(string value)
    {
        FilterTablets(value);
    }

    private void FilterTablets(string filterText)
    {
        ObservableCollection<CompanyDetailMachine> filtered = string.IsNullOrWhiteSpace(filterText)
                                                                  ? new ObservableCollection<
                                                                      CompanyDetailMachine>(Tablets)
                                                                  : new
                                                                      ObservableCollection<
                                                                          CompanyDetailMachine>(Tablets.Where(c =>
                                                                          c.Name.Contains(filterText,
                                                                              StringComparison
                                                                                 .OrdinalIgnoreCase)));

        FilteredTablets = filtered;
    }

    partial void OnPdasFilterTextChanged(string value)
    {
        FilterPdas(value);
    }

    private void FilterPdas(string filterText)
    {
        ObservableCollection<CompanyDetailMachine> filtered = string.IsNullOrWhiteSpace(filterText)
                                                                  ? new ObservableCollection<
                                                                      CompanyDetailMachine>(Pdas)
                                                                  : new
                                                                      ObservableCollection<
                                                                          CompanyDetailMachine>(Pdas.Where(c =>
                                                                          c.Name.Contains(filterText,
                                                                              StringComparison
                                                                                 .OrdinalIgnoreCase)));

        FilteredPdas = filtered;
    }

    private Task NavigateToMachineAsync(CompanyDetailMachine? machine)
    {
        if(machine == null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineId, machine.Id },
            { NavParamKeys.NavigationSource, nameof(CompanyDetailViewModel) },
            { NavParamKeys.CompanyId, CompanyId }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MachineViewPage), parameters);

        return Task.CompletedTask;
    }

    private Task NavigateToSoldToCompanyAsync()
    {
        if(SoldToCompany?.Id == null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.CompanyId, SoldToCompany.Id.Value },
            { NavParamKeys.NavigationSource, nameof(CompanyDetailViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(CompanyDetailPage), parameters);

        return Task.CompletedTask;
    }

    private Task NavigateToDetailAsync(CompanyDetailItem? item, string pageName, string idParamKey,
                                       bool             useLongId = false)
    {
        if(item == null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { idParamKey, useLongId ? item.Id : (int)item.Id },
            { NavParamKeys.NavigationSource, nameof(CompanyDetailViewModel) },
            { NavParamKeys.CompanyId, CompanyId }
        };

        _regionManager.RequestNavigate(RegionNames.Content, pageName, parameters);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Gets the formatted founding date with unknown handling
    /// </summary>
    public string GetFoundedDateDisplay(CompanyDto company)
    {
        return DatePrecisionFormatter.FormatWithTrailingPeriod(company.Founded, company.FoundedPrecision);
    }

    /// <summary>
    ///     Gets the formatted sold/event date with unknown handling
    /// </summary>
    public string GetEventDateDisplay(CompanyDto? company)
    {
        if(company?.Sold is null) return _localizer["unknown date"].Value;

        return DatePrecisionFormatter.Format(company.Sold, company.SoldPrecision);
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
                    var     countryCode = (short)(Company.CountryId ?? 0);
                    Stream? flagStream  = await _flagCache.GetFlagAsync(countryCode);

                    FlagImageSource = await _imageSourceFactory.CreateSvgImageSourceAsync(flagStream);

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
                int soldToId                   = Company.SoldToId ?? 0;
                if(soldToId > 0) SoldToCompany = await _companyDetailService.GetSoldToCompanyAsync(soldToId);
            }

            // Load logo if available
            if(Company.LastLogo.HasValue)
            {
                try
                {
                    Stream? logoStream = await _logoCache.GetLogoAsync(Company.LastLogo.Value);

                    LogoImageSource = await _imageSourceFactory.CreateSvgImageSourceAsync(logoStream);

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
                                                   logo.Year
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

                        BitmapImage? logoSource =
                            await _imageSourceFactory.CreateSvgImageSourceAsync(logoStream);

                        if(logoSource != null)
                        {
                            loadedLogos.Add(new CompanyLogoItem
                            {
                                LogoGuid   = logoData.Logo.Guid.Value,
                                LogoSource = logoSource,
                                Year       = logoData.Year
                            });
                        }
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
            Smartphones.Clear();
            Tablets.Clear();
            Pdas.Clear();
            FilteredComputers.Clear();
            FilteredConsoles.Clear();

            foreach(MachineDto machine in machines)
            {
                int machineId = machine.Id ?? 0;

                var machineItem = new CompanyDetailMachine
                {
                    Id   = machineId,
                    Name = machine.Name ?? string.Empty
                };

                // Categorize by machine type enum
                if(machine.Type == (int)MachineType.Computer)
                    Computers.Add(machineItem);
                else if(machine.Type == (int)MachineType.Console)
                    Consoles.Add(machineItem);
                else if(machine.Type == (int)MachineType.Smartphone)
                    Smartphones.Add(machineItem);
                else if(machine.Type == (int)MachineType.Tablet)
                    Tablets.Add(machineItem);
                else if(machine.Type == (int)MachineType.Pda)
                    Pdas.Add(machineItem);
            }

            // Initialize filtered lists
            FilteredComputers   = new ObservableCollection<CompanyDetailMachine>(Computers);
            FilteredConsoles    = new ObservableCollection<CompanyDetailMachine>(Consoles);
            FilteredSmartphones = new ObservableCollection<CompanyDetailMachine>(Smartphones);
            FilteredTablets     = new ObservableCollection<CompanyDetailMachine>(Tablets);
            FilteredPdas        = new ObservableCollection<CompanyDetailMachine>(Pdas);

            // Load machine families designed by this company
            try
            {
                List<MachineFamilyDto> families = await _companyDetailService.GetMachineFamiliesAsync(CompanyId);

                MachineFamilies = new ObservableCollection<CompanyDetailItem>(families.Select(f =>
                    new CompanyDetailItem { Id = f.Id ?? 0, Name = f.Name ?? string.Empty }));
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load machine families for company: {Exception}", ex.Message);
            }

            // Load GPUs manufactured by this company
            try
            {
                List<GpuDto> gpus = await _companyDetailService.GetGpusAsync(CompanyId);

                Gpus = new ObservableCollection<CompanyDetailItem>(gpus.Select(g =>
                    new CompanyDetailItem { Id = g.Id ?? 0, Name = g.Name ?? string.Empty }));
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load GPUs for company: {Exception}", ex.Message);
            }

            // Load processors manufactured by this company
            try
            {
                List<ProcessorDto> processors = await _companyDetailService.GetProcessorsAsync(CompanyId);

                Processors = new ObservableCollection<CompanyDetailItem>(processors.Select(p =>
                    new CompanyDetailItem { Id = p.Id ?? 0, Name = p.Name ?? string.Empty }));
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load processors for company: {Exception}", ex.Message);
            }

            // Load sound synthesizers made by this company
            try
            {
                List<SoundSynthDto> soundSynths = await _companyDetailService.GetSoundSynthsAsync(CompanyId);

                SoundSynths = new ObservableCollection<CompanyDetailItem>(soundSynths.Select(s =>
                    new CompanyDetailItem { Id = s.Id ?? 0, Name = s.Name ?? string.Empty }));
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load sound synthesizers for company: {Exception}", ex.Message);
            }

            // Load software related to this company
            try
            {
                List<SoftwareDto> software = await _companyDetailService.GetSoftwareAsync(CompanyId);

                Software = new ObservableCollection<CompanyDetailItem>(software.Select(s =>
                    new CompanyDetailItem { Id = s.Id ?? 0, Name = s.Name ?? string.Empty }));
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load software for company: {Exception}", ex.Message);
            }

            // Load books related to this company
            try
            {
                List<BookDto> books = await _companyDetailService.GetBooksAsync(CompanyId);

                Books = new ObservableCollection<CompanyDetailItem>(books.Select(b =>
                    new CompanyDetailItem { Id = b.Id ?? 0, Name = b.Title ?? string.Empty }));
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load books for company: {Exception}", ex.Message);
            }

            // Load documents related to this company
            try
            {
                List<DocumentDto> documents = await _companyDetailService.GetDocumentsAsync(CompanyId);

                Documents = new ObservableCollection<CompanyDetailItem>(documents.Select(d =>
                    new CompanyDetailItem { Id = d.Id ?? 0, Name = d.Title ?? string.Empty }));
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load documents for company: {Exception}", ex.Message);
            }

            // Load magazines related to this company
            try
            {
                List<MagazineDto> magazines = await _companyDetailService.GetMagazinesAsync(CompanyId);

                Magazines = new ObservableCollection<CompanyDetailItem>(magazines.Select(m =>
                    new CompanyDetailItem { Id = m.Id ?? 0, Name = m.Title ?? string.Empty }));
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load magazines for company: {Exception}", ex.Message);
            }

            // Load people associated with this company
            try
            {
                People.Clear();

                List<PersonByCompanyDto> people = await _companyDetailService.GetPeopleByCompanyAsync(CompanyId);

                foreach(PersonByCompanyDto person in people)
                {
                    string name = person.DisplayName ?? person.Alias ??
                                  $"{person.Name} {person.Surname}".Trim();

                    string display = name;

                    if(!string.IsNullOrWhiteSpace(person.Position))
                        display += $" — {person.Position}";

                    if(person.Ongoing == true)
                        display += $" ({_localizer["OngoingText"].Value})";
                    else if(person.Start != null || person.End != null)
                    {
                        string start = person.Start?.ToString("yyyy") ?? "?";
                        string end   = person.End?.ToString("yyyy")   ?? "?";
                        display += $" ({start}–{end})";
                    }

                    People.Add(new CompanyDetailItem { Id = person.PersonId ?? 0, Name = display });
                }

                OnPropertyChanged(nameof(ShowPeople));
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load people for company: {Exception}", ex.Message);
            }

            // Load localized description
            try
            {
                string langCode = GetIso639CodeFromCulture();
                CompanyDescriptionDto? desc = await _companyDetailService.GetDescriptionAsync(CompanyId, langCode);

                DescriptionMarkdown = desc?.Markdown ?? string.Empty;
            }
            catch(Exception ex)
            {
                _logger.LogError("Failed to load company description: {Exception}", ex.Message);
            }

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
    private Task GoBackAsync()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        _regionManager.RequestNavigate(RegionNames.Content, nameof(CompaniesPage));

        return Task.CompletedTask;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.CompanyId, out int companyId))
        {
            CompanyId = companyId;
            _ = LoadData.ExecuteAsync(null);
        }
    }

    private static string GetIso639CodeFromCulture()
    {
        string twoLetter = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        return twoLetter switch
        {
            "en" => "eng",
            "es" => "spa",
            "de" => "deu",
            "fr" => "fra",
            "la" => "lat",
            "pt" => "por",
            _    => "eng"
        };
    }
}
