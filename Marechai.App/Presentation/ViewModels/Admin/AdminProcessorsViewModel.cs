#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Dialogs;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Microsoft.UI.Dispatching;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminProcessorsViewModel : ObservableObject, IRegionAware
{
    private readonly Client                          _apiClient;
    private readonly IJwtService                     _jwtService;
    private readonly IStringLocalizer                _localizer;
    private readonly ILogger<AdminProcessorsViewModel> _logger;
    private readonly ProcessorsService               _processorsService;
    private readonly IRegionManager                  _regionManager;
    private readonly ITokenService                   _tokenService;
    private readonly DispatcherQueue?                _dispatcherQueue;

    // --- List state ---
    [ObservableProperty]
    private ObservableCollection<ProcessorDto> _processors = [];

    [ObservableProperty]
    private ObservableCollection<ProcessorDto> _filteredProcessors = [];

    [ObservableProperty]
    private List<int> _pageSizeOptions = [10, 25, 50, 100];

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 25;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private ProcessorDto? _selectedProcessor;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    // --- Edit panel state ---
    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editPanelTitle = string.Empty;

    private int? _editingProcessorId;

    // --- Description panel state ---
    [ObservableProperty]
    private bool _isEditingDescription;

    [ObservableProperty]
    private string _descriptionMarkdown = string.Empty;

    [ObservableProperty]
    private int? _descriptionProcessorId;

    [ObservableProperty]
    private ObservableCollection<LanguageItem> _availableLanguages = [];

    [ObservableProperty]
    private LanguageItem? _selectedLanguage;

    [ObservableProperty]
    private ObservableCollection<ProcessorDescriptionDto> _existingTranslations = [];

    public bool CanSaveDescription =>
        DescriptionProcessorId.HasValue &&
        SelectedLanguage is not null &&
        !string.IsNullOrWhiteSpace(DescriptionMarkdown);

    // --- Form fields ---
    [ObservableProperty]
    private string _processorName = string.Empty;

    [ObservableProperty]
    private string _modelCode = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _introduced;

    [ObservableProperty]
    private int _introducedPrecision;

    [ObservableProperty]
    private double? _speed;

    [ObservableProperty]
    private string _package = string.Empty;

    [ObservableProperty]
    private string _process = string.Empty;

    [ObservableProperty]
    private double? _processNm;

    [ObservableProperty]
    private double? _dieSize;

    [ObservableProperty]
    private long? _transistors;

    [ObservableProperty]
    private int? _cores;

    [ObservableProperty]
    private int? _threadsPerCore;

    [ObservableProperty]
    private int? _dataBus;

    [ObservableProperty]
    private int? _addressBus;

    [ObservableProperty]
    private int? _gprs;

    [ObservableProperty]
    private int? _gprSize;

    [ObservableProperty]
    private int? _fprs;

    [ObservableProperty]
    private int? _fprSize;

    [ObservableProperty]
    private int? _simdRegisters;

    [ObservableProperty]
    private int? _simdSize;

    [ObservableProperty]
    private double? _l1Instruction;

    [ObservableProperty]
    private double? _l1Data;

    [ObservableProperty]
    private double? _l2;

    [ObservableProperty]
    private double? _l3;

    // --- Company picker ---
    [ObservableProperty]
    private CompanyDto? _selectedCompany;

    [ObservableProperty]
    private string _companySearchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyDto> _companySuggestions = [];

    private List<CompanyDto>? _allCompanies;

    // --- Instruction set picker ---
    [ObservableProperty]
    private InstructionSetDto? _selectedInstructionSet;

    [ObservableProperty]
    private ObservableCollection<InstructionSetDto> _instructionSets = [];

    // --- ISA extensions by processor ---
    [ObservableProperty]
    private ObservableCollection<InstructionSetExtensionByProcessorDto> _processorExtensions = [];

    [ObservableProperty]
    private ObservableCollection<InstructionSetExtensionDto> _availableExtensions = [];

    [ObservableProperty]
    private InstructionSetExtensionDto? _selectedAvailableExtension;

    private List<InstructionSetExtensionDto>? _allAvailableExtensions;

    public AdminProcessorsViewModel(Client                          apiClient,
                                    IJwtService                     jwtService,
                                    ITokenService                   tokenService,
                                    ILogger<AdminProcessorsViewModel> logger,
                                    ProcessorsService               processorsService,
                                    IStringLocalizer                localizer,
                                    IRegionManager                  regionManager)
    {
        _apiClient         = apiClient;
        _jwtService        = jwtService;
        _tokenService      = tokenService;
        _logger            = logger;
        _processorsService = processorsService;
        _localizer         = localizer;
        _regionManager     = regionManager;
        _dispatcherQueue   = DispatcherQueue.GetForCurrentThread();

        LoadProcessorsCommand    = new AsyncRelayCommand(LoadProcessorsAsync);
        OpenAddProcessorCommand  = new RelayCommand(OpenAddProcessor);
        OpenEditProcessorCommand = new RelayCommand<ProcessorDto>(OpenEditProcessor);
        OpenDescriptionCommand   = new RelayCommand<ProcessorDto>(OpenDescription);
        OpenPhotosCommand        = new RelayCommand<ProcessorDto>(OpenPhotos);
        OpenVideosCommand        = new RelayCommand<ProcessorDto>(OpenVideos);
        DeleteProcessorCommand   = new AsyncRelayCommand<ProcessorDto>(DeleteProcessorAsync);
        NextPageCommand          = new AsyncRelayCommand(NextPageAsync);
        PreviousPageCommand      = new AsyncRelayCommand(PreviousPageAsync);
        SaveProcessorCommand     = new AsyncRelayCommand(SaveProcessorAsync);
        CancelEditCommand        = new RelayCommand(CancelEdit);
        SaveDescriptionCommand   = new AsyncRelayCommand(SaveDescriptionAsync);
        CancelDescriptionCommand = new RelayCommand(CancelDescription);
        DeleteTranslationCommand = new AsyncRelayCommand<ProcessorDescriptionDto>(DeleteTranslationAsync);
        EditTranslationCommand   = new RelayCommand<ProcessorDescriptionDto>(EditTranslation);
        AddExtensionCommand      = new AsyncRelayCommand(AddExtensionAsync);
        RemoveExtensionCommand   = new AsyncRelayCommand<InstructionSetExtensionByProcessorDto>(RemoveExtensionAsync);

        InitializeLanguages();
        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand               LoadProcessorsCommand    { get; }
    public IRelayCommand                    OpenAddProcessorCommand  { get; }
    public IRelayCommand<ProcessorDto>      OpenEditProcessorCommand { get; }
    public IRelayCommand<ProcessorDto>      OpenDescriptionCommand   { get; }
    public IRelayCommand<ProcessorDto>      OpenPhotosCommand        { get; }
    public IRelayCommand<ProcessorDto>      OpenVideosCommand        { get; }
    public IAsyncRelayCommand<ProcessorDto> DeleteProcessorCommand   { get; }
    public IAsyncRelayCommand               NextPageCommand { get; }
    public IAsyncRelayCommand               PreviousPageCommand { get; }
    public IAsyncRelayCommand               SaveProcessorCommand     { get; }
    public IRelayCommand                    CancelEditCommand        { get; }
    public IAsyncRelayCommand               SaveDescriptionCommand   { get; }
    public IRelayCommand                    CancelDescriptionCommand { get; }
    public IAsyncRelayCommand<ProcessorDescriptionDto> DeleteTranslationCommand { get; }
    public IRelayCommand<ProcessorDescriptionDto> EditTranslationCommand { get; }
    public IAsyncRelayCommand               AddExtensionCommand      { get; }
    public IAsyncRelayCommand<InstructionSetExtensionByProcessorDto> RemoveExtensionCommand { get; }
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage * PageSize < TotalCount;
    public string PageSummary => TotalCount == 0
                                     ? "0 items"
                                     : $"{((CurrentPage - 1) * PageSize) + 1}-{Math.Min(CurrentPage * PageSize, TotalCount)} of {TotalCount}";

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
        {
            _ = LoadProcessorsCommand.ExecuteAsync(null);
            _ = LoadPickerDataAsync();
        }
    }

    // --- Role check ---
    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(string.IsNullOrWhiteSpace(token))
            {
                IsAdmin = false;

                return;
            }

            if(!_jwtService.IsTokenValid(token))
            {
                IsAdmin = false;

                return;
            }

            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    // --- Load processors ---
    private async Task LoadProcessorsAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Processors.Clear();

            string? filter = NullIfWhiteSpace(FilterText);
            string[]? filters = BuildNameFilters(filter);
            TotalCount = await _apiClient.Processors.Count.GetAsync(config =>
            {
                if(filters != null)
                    config.QueryParameters.Filters = filters;
            }) ?? 0;

            int maxPage = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

            if(CurrentPage > maxPage)
                CurrentPage = maxPage;

            int skip = (CurrentPage - 1) * PageSize;

            List<ProcessorDto>? response = await _apiClient.Processors.GetAsync(config =>
            {
                config.QueryParameters.Skip = skip;
                config.QueryParameters.Take = PageSize;

                if(filters != null)
                    config.QueryParameters.Filters = filters;
            });

            if(response != null)
                foreach(ProcessorDto proc in response)
                    Processors.Add(proc);

            ReplaceFilteredProcessors(Processors);
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading processors");
            ErrorMessage = _localizer["FailedToLoadProcessors"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- Add ---
    private void OpenAddProcessor()
    {
        _editingProcessorId = null;
        EditPanelTitle      = _localizer["AddProcessorDialog_Title"];
        IsEditingDescription = false;
        ClearForm();
        IsEditing = true;
    }

    private void OpenPhotos(ProcessorDto? proc)
    {
        if(proc?.Id == null) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.ProcessorId, proc.Id.Value },
            { NavParamKeys.ProcessorName, proc.Name ?? string.Empty }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminProcessorPhotosPage), parameters);
    }

    private void OpenVideos(ProcessorDto? proc)
    {
        if(proc?.Id == null) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.ProcessorId, proc.Id.Value },
            { NavParamKeys.ProcessorName, proc.Name ?? string.Empty }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminProcessorVideosPage), parameters);
    }

    // --- Edit ---
    private async void OpenEditProcessor(ProcessorDto? proc)
    {
        if(proc?.Id == null) return;

        try
        {
            // Fetch full details (list may return partial data)
            ProcessorDto? full = await _apiClient.Processors[proc.Id.Value].GetAsync();

            if(full == null) return;

            _editingProcessorId = proc.Id;
            EditPanelTitle      = _localizer["EditProcessorDialog_Title"];
            IsEditingDescription = false;
            PopulateForm(full);

            if(proc.Id.HasValue)
                await LoadProcessorExtensionsAsync(proc.Id.Value);

            IsEditing = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading processor details for {Id}", proc.Id);
            ErrorMessage = _localizer["FailedToLoadProcessors"];
            HasError     = true;
        }
    }

    // --- Delete ---
    private async Task DeleteProcessorAsync(ProcessorDto? proc)
    {
        if(proc?.Id == null) return;

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer, proc.Name ?? string.Empty))
            return;

        try
        {
            await _apiClient.Processors[proc.Id.Value].DeleteAsync();
            await LoadProcessorsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting processor {Id}", proc.Id);
            ErrorMessage = _localizer["FailedToDeleteProcessor"];
            HasError     = true;
        }
    }

    // --- Save ---
    private async Task SaveProcessorAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(ProcessorName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError     = true;

                return;
            }

            var dto = new ProcessorDto
            {
                Name             = ProcessorName,
                CompanyId        = SelectedCompany?.Id,
                ModelCode        = string.IsNullOrWhiteSpace(ModelCode) ? null : ModelCode,
                InstructionSetId = SelectedInstructionSet?.Id,
                Introduced       = Introduced,
                IntroducedPrecision = IntroducedPrecision,
                Speed            = Speed,
                Package          = string.IsNullOrWhiteSpace(Package) ? null : Package,
                Process          = string.IsNullOrWhiteSpace(Process) ? null : Process,
                ProcessNm        = ProcessNm.HasValue ? (float)ProcessNm.Value : null,
                DieSize          = DieSize.HasValue   ? (float)DieSize.Value   : null,
                Transistors      = Transistors,
                Cores            = Cores,
                ThreadsPerCore   = ThreadsPerCore,
                DataBus          = DataBus,
                AddressBus       = AddressBus,
                Gprs             = Gprs,
                GprSize          = GprSize,
                Fprs             = Fprs,
                FprSize          = FprSize,
                SimdRegisters    = SimdRegisters,
                SimdSize         = SimdSize,
                L1Instruction    = L1Instruction.HasValue ? (float)L1Instruction.Value : null,
                L1Data           = L1Data.HasValue        ? (float)L1Data.Value        : null,
                L2               = L2.HasValue            ? (float)L2.Value            : null,
                L3               = L3.HasValue            ? (float)L3.Value            : null
            };

            if(_editingProcessorId == null)
                await _apiClient.Processors.PostAsync(dto);
            else
            {
                dto.Id = _editingProcessorId;
                await _apiClient.Processors[_editingProcessorId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadProcessorsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving processor");
            ErrorMessage = _localizer["FailedToSaveProcessor"];
            HasError     = true;
        }
    }

    // --- Cancel ---
    private void CancelEdit()
    {
        IsEditing           = false;
        _editingProcessorId = null;
        ClearForm();
        HasError     = false;
        ErrorMessage = string.Empty;
    }

    // --- Filtering ---
    public void ApplyFilter()
    {
        _ = ReloadFromFirstPageAsync();
    }

    private void InitializeLanguages()
    {
        AvailableLanguages =
        [
            new LanguageItem { Code = "eng", DisplayName = "English" },
            new LanguageItem { Code = "spa", DisplayName = "Español" },
            new LanguageItem { Code = "deu", DisplayName = "Deutsch" },
            new LanguageItem { Code = "fra", DisplayName = "Français" },
            new LanguageItem { Code = "lat", DisplayName = "Latina" },
            new LanguageItem { Code = "por", DisplayName = "Português (Brasil)" }
        ];

        SelectedLanguage = AvailableLanguages[0];
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        if(value == null || ExistingTranslations.Count == 0)
        {
            DescriptionMarkdown = string.Empty;
            OnPropertyChanged(nameof(CanSaveDescription));

            return;
        }

        ProcessorDescriptionDto? existing = ExistingTranslations.FirstOrDefault(t => t.LanguageCode == value.Code);
        DescriptionMarkdown = existing?.Markdown ?? string.Empty;
        OnPropertyChanged(nameof(CanSaveDescription));
    }

    partial void OnDescriptionMarkdownChanged(string value) => OnPropertyChanged(nameof(CanSaveDescription));

    partial void OnDescriptionProcessorIdChanged(int? value) => OnPropertyChanged(nameof(CanSaveDescription));

    // --- Company search ---
    public void UpdateCompanySuggestions(string query)
    {
        CompanySuggestions.Clear();

        if(_allCompanies == null) return;

        IEnumerable<CompanyDto> source = _allCompanies;

        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(c => c.Name != null &&
                                       c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(CompanyDto match in source)
            CompanySuggestions.Add(match);
    }

    // --- Load picker data ---
    public async Task LoadPickerDataAsync()
    {
        try
        {
            _allCompanies = await _apiClient.Companies.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading companies for picker");
        }

        try
        {
            List<InstructionSetDto>? sets = await _apiClient.InstructionSets.GetAsync();
            InstructionSets.Clear();

            if(sets != null)
                foreach(InstructionSetDto s in sets)
                    InstructionSets.Add(s);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading instruction sets");
        }

        try
        {
            _allAvailableExtensions = await _apiClient.InstructionSetExtensions.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading instruction set extensions");
        }
    }

    private void OpenDescription(ProcessorDto? processor) => _ = OpenDescriptionAsync(processor);

    private async Task OpenDescriptionAsync(ProcessorDto? proc)
    {
        if(proc?.Id == null) return;

        try
        {
            await RunOnUiThreadAsync(() =>
            {
                HasError               = false;
                ErrorMessage           = string.Empty;
                DescriptionProcessorId = proc.Id;
                DescriptionMarkdown    = string.Empty;
                IsEditing              = false;
                ExistingTranslations.Clear();
            });
            await ReloadDescriptionTranslationsAsync(proc.Id.Value);
            await RunOnUiThreadAsync(() =>
            {
                SelectedLanguage     = GetDefaultDescriptionLanguage();
                IsEditingDescription = true;
            });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading descriptions for processor {Id}", proc.Id);
            await RunOnUiThreadAsync(() =>
            {
                DescriptionMarkdown  = string.Empty;
                IsEditingDescription = true;
            });
        }
    }

    private async Task ReloadDescriptionTranslationsAsync(int processorId)
    {
        List<ProcessorDescriptionDto> translations = await _processorsService.GetDescriptionsAsync(processorId);

        await RunOnUiThreadAsync(() =>
        {
            ExistingTranslations.Clear();

            foreach(ProcessorDescriptionDto translation in translations.OrderBy(t => GetLanguageDisplayName(t.LanguageCode)))
            {
                translation.Language ??= GetLanguageDisplayName(translation.LanguageCode);
                ExistingTranslations.Add(translation);
            }
        });
    }

    private Task RunOnUiThreadAsync(Action action)
    {
        if(_dispatcherQueue is null || _dispatcherQueue.HasThreadAccess)
        {
            action();

            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();

        if(!_dispatcherQueue.TryEnqueue(() =>
           {
               try
               {
                   action();
                   tcs.SetResult();
               }
               catch(Exception ex)
               {
                   tcs.SetException(ex);
               }
           }))
            tcs.SetException(new InvalidOperationException("Unable to enqueue work on the UI thread."));

        return tcs.Task;
    }

    private LanguageItem GetDefaultDescriptionLanguage()
    {
        return AvailableLanguages.FirstOrDefault(language =>
                   ExistingTranslations.All(translation => translation.LanguageCode != language.Code)) ??
               AvailableLanguages.FirstOrDefault(language => language.Code == "eng") ??
               AvailableLanguages.First();
    }

    private string GetLanguageDisplayName(string? languageCode)
    {
        if(string.IsNullOrWhiteSpace(languageCode)) return string.Empty;

        return AvailableLanguages.FirstOrDefault(language => language.Code == languageCode)?.DisplayName ??
               ExistingTranslations.FirstOrDefault(translation => translation.LanguageCode == languageCode)?.Language ??
               languageCode;
    }

    private async Task SaveDescriptionAsync()
    {
        if(!CanSaveDescription || DescriptionProcessorId == null || SelectedLanguage == null) return;

        try
        {
            var dto = new ProcessorDescriptionDto
            {
                ProcessorId  = DescriptionProcessorId.Value,
                Markdown     = DescriptionMarkdown,
                LanguageCode = SelectedLanguage.Code
            };

            (bool succeeded, string? error) =
                await _processorsService.CreateOrUpdateDescriptionAsync(DescriptionProcessorId.Value, dto);

            if(!succeeded)
            {
                ErrorMessage = error ?? _localizer["FailedToSaveDescription"];
                HasError     = true;

                return;
            }

            await ReloadDescriptionTranslationsAsync(DescriptionProcessorId.Value);
            HasError     = false;
            ErrorMessage = string.Empty;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving processor description");
            ErrorMessage = _localizer["FailedToSaveDescription"];
            HasError     = true;
        }
    }

    private void EditTranslation(ProcessorDescriptionDto? translation)
    {
        if(translation?.LanguageCode == null) return;

        SelectedLanguage    = AvailableLanguages.FirstOrDefault(l => l.Code == translation.LanguageCode);
        DescriptionMarkdown = translation.Markdown ?? string.Empty;
    }

    private async Task DeleteTranslationAsync(ProcessorDescriptionDto? translation)
    {
        if(DescriptionProcessorId == null || translation?.LanguageCode == null) return;

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer,
               $"{ProcessorName} ({translation.LanguageCode})"))
            return;

        try
        {
            (bool succeeded, string? error) =
                await _processorsService.DeleteDescriptionAsync(DescriptionProcessorId.Value, translation.LanguageCode);

            if(!succeeded)
            {
                ErrorMessage = error ?? _localizer["FailedToDeleteTranslation"];
                HasError     = true;

                return;
            }

            await ReloadDescriptionTranslationsAsync(DescriptionProcessorId.Value);
            HasError     = false;
            ErrorMessage = string.Empty;

            if(SelectedLanguage?.Code == translation.LanguageCode)
            {
                DescriptionMarkdown = string.Empty;

                LanguageItem nextLanguage = GetDefaultDescriptionLanguage();

                if(SelectedLanguage?.Code == nextLanguage.Code)
                    OnSelectedLanguageChanged(nextLanguage);
                else
                    SelectedLanguage = nextLanguage;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting processor description");
            ErrorMessage = _localizer["FailedToDeleteTranslation"];
            HasError     = true;
        }
    }

    partial void OnCurrentPageChanged(int value) => NotifyPaginationStateChanged();

    partial void OnPageSizeChanged(int value) => NotifyPaginationStateChanged();

    partial void OnTotalCountChanged(int value) => NotifyPaginationStateChanged();

    private async Task NextPageAsync()
    {
        if(!CanGoNext) return;

        CurrentPage++;
        await LoadProcessorsAsync();
    }

    private async Task PreviousPageAsync()
    {
        if(!CanGoPrevious) return;

        CurrentPage--;
        await LoadProcessorsAsync();
    }

    private async Task ReloadFromFirstPageAsync()
    {
        CurrentPage = 1;
        await LoadProcessorsAsync();
    }

    private static string[]? BuildNameFilters(string? filter) =>
        string.IsNullOrWhiteSpace(filter) ? null : [$"Name||contains||{filter.Trim()}"];

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void ReplaceFilteredProcessors(IEnumerable<ProcessorDto> items)
    {
        FilteredProcessors.Clear();

        foreach(ProcessorDto item in items)
            FilteredProcessors.Add(item);
    }

    private void NotifyPaginationStateChanged()
    {
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(PageSummary));
    }

    private void CancelDescription()
    {
        IsEditingDescription = false;
        DescriptionProcessorId = null;
        DescriptionMarkdown = string.Empty;
        ExistingTranslations.Clear();
        SelectedLanguage = AvailableLanguages.FirstOrDefault(language => language.Code == "eng") ??
                           AvailableLanguages.FirstOrDefault();
        HasError         = false;
        ErrorMessage     = string.Empty;
    }

    // --- ISA extension management ---
    private async Task LoadProcessorExtensionsAsync(int processorId)
    {
        ProcessorExtensions.Clear();

        try
        {
            List<InstructionSetExtensionByProcessorDto>? exts =
                await _apiClient.Processor[processorId].InstructionSetExtensions.GetAsync();

            if(exts != null)
                foreach(InstructionSetExtensionByProcessorDto ext in exts)
                    ProcessorExtensions.Add(ext);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading extensions for processor {Id}", processorId);
        }

        RefreshAvailableExtensions();
    }

    private void RefreshAvailableExtensions()
    {
        AvailableExtensions.Clear();

        if(_allAvailableExtensions == null) return;

        HashSet<int> assignedIds = new(ProcessorExtensions
                                     .Where(e => e.ExtensionId.HasValue)
                                     .Select(e => e.ExtensionId!.Value));

        foreach(InstructionSetExtensionDto ext in _allAvailableExtensions)
            if(ext.Id.HasValue && !assignedIds.Contains(ext.Id.Value))
                AvailableExtensions.Add(ext);
    }

    private async Task AddExtensionAsync()
    {
        if(_editingProcessorId == null || SelectedAvailableExtension?.Id == null) return;

        try
        {
            var dto = new InstructionSetExtensionByProcessorDto
            {
                ProcessorId = _editingProcessorId.Value,
                ExtensionId = SelectedAvailableExtension.Id.Value
            };

            await _apiClient.InstructionSetExtensionsByProcessor.PostAsync(dto);
            SelectedAvailableExtension = null;
            await LoadProcessorExtensionsAsync(_editingProcessorId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding extension to processor");
            ErrorMessage = _localizer["FailedToSaveProcessor"];
            HasError     = true;
        }
    }

    private async Task RemoveExtensionAsync(InstructionSetExtensionByProcessorDto? ext)
    {
        if(ext?.Id == null || _editingProcessorId == null) return;

        try
        {
            await _apiClient.InstructionSetExtensionsByProcessor[ext.Id.Value].DeleteAsync();
            await LoadProcessorExtensionsAsync(_editingProcessorId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing extension from processor");
            ErrorMessage = _localizer["FailedToSaveProcessor"];
            HasError     = true;
        }
    }

    // --- Helpers ---
    private void ClearForm()
    {
        ProcessorName        = string.Empty;
        ModelCode             = string.Empty;
        Introduced            = null;
        IntroducedPrecision = 0;
        Speed                 = null;
        Package               = string.Empty;
        Process               = string.Empty;
        ProcessNm             = null;
        DieSize               = null;
        Transistors           = null;
        Cores                 = null;
        ThreadsPerCore        = null;
        DataBus               = null;
        AddressBus            = null;
        Gprs                  = null;
        GprSize               = null;
        Fprs                  = null;
        FprSize               = null;
        SimdRegisters         = null;
        SimdSize              = null;
        L1Instruction         = null;
        L1Data                = null;
        L2                    = null;
        L3                    = null;
        SelectedCompany       = null;
        CompanySearchText     = string.Empty;
        SelectedInstructionSet = null;
        ProcessorExtensions.Clear();
        AvailableExtensions.Clear();
        SelectedAvailableExtension = null;
        HasError              = false;
        ErrorMessage          = string.Empty;
    }

    private void PopulateForm(ProcessorDto proc)
    {
        ProcessorName  = proc.Name      ?? string.Empty;
        ModelCode      = proc.ModelCode  ?? string.Empty;
        Introduced     = proc.Introduced;
        IntroducedPrecision = proc.IntroducedPrecision ?? 0;
        Speed          = proc.Speed;
        Package        = proc.Package   ?? string.Empty;
        Process        = proc.Process   ?? string.Empty;
        ProcessNm      = proc.ProcessNm;
        DieSize        = proc.DieSize;
        Transistors    = proc.Transistors;
        Cores          = proc.Cores;
        ThreadsPerCore = proc.ThreadsPerCore;
        DataBus        = proc.DataBus;
        AddressBus     = proc.AddressBus;
        Gprs           = proc.Gprs;
        GprSize        = proc.GprSize;
        Fprs           = proc.Fprs;
        FprSize        = proc.FprSize;
        SimdRegisters  = proc.SimdRegisters;
        SimdSize       = proc.SimdSize;
        L1Instruction  = proc.L1Instruction;
        L1Data         = proc.L1Data;
        L2             = proc.L2;
        L3             = proc.L3;

        // Company picker
        if(proc.CompanyId.HasValue && _allCompanies != null)
        {
            CompanyDto? company = _allCompanies.FirstOrDefault(c => c.Id == proc.CompanyId.Value);

            if(company != null)
            {
                CompanySearchText = company.Name ?? string.Empty;
                UpdateCompanySuggestions(CompanySearchText);
                SelectedCompany = CompanySuggestions.FirstOrDefault(c => c.Id == company.Id);
            }
            else
            {
                CompanySearchText = proc.Company ?? string.Empty;
                SelectedCompany   = null;
            }
        }
        else
        {
            CompanySearchText = string.Empty;
            SelectedCompany   = null;
        }

        // Instruction set picker
        SelectedInstructionSet = proc.InstructionSetId.HasValue
                                     ? InstructionSets.FirstOrDefault(s => s.Id == proc.InstructionSetId.Value)
                                     : null;
    }
}
