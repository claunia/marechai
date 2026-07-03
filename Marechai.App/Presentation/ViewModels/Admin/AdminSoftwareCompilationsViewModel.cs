#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Presentation.Dialogs;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareCompilationsViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareCompilationsService                  _service;
    private readonly IJwtService                                  _jwtService;
    private readonly ITokenService                                _tokenService;
    private readonly ILogger<AdminSoftwareCompilationsViewModel>  _logger;
    private readonly IStringLocalizer                             _localizer;

    private const int PageSize = 25;
    private const int SearchDebounceMs = 300;

    private CancellationTokenSource? _softwarePickerDebounce;
    private CancellationTokenSource? _predecessorPickerDebounce;
    private CancellationTokenSource? _includedSoftwareDebounce;
    private CancellationTokenSource? _includedCompilationDebounce;
    private CancellationTokenSource? _mergeTargetDebounce;
    private CancellationTokenSource? _mergeSourceDebounce;

    private int? _editingId;
    private List<MachineDto>? _allMachines;
    private SoftwareDto? _selectedIncludedSoftwareForVersions;

    // --- List / paging state ---
    [ObservableProperty] private ObservableCollection<SoftwareCompilationDto> _compilations = [];
    [ObservableProperty] private string                                       _searchText = string.Empty;
    [ObservableProperty] private SoftwareCompilationDto?                      _selectedCompilation;
    [ObservableProperty] private bool                                         _isLoading;
    [ObservableProperty] private bool                                         _isDataLoaded;
    [ObservableProperty] private bool                                         _hasError;
    [ObservableProperty] private string                                       _errorMessage = string.Empty;
    [ObservableProperty] private bool                                         _isAdmin;
    [ObservableProperty] private bool                                         _isEditing;
    [ObservableProperty] private bool                                         _isEditingExisting;
    [ObservableProperty] private string                                       _editPanelTitle = string.Empty;
    [ObservableProperty] private int                                          _currentSkip;
    [ObservableProperty] private int                                          _totalCount;

    public bool CanGoPrevious => CurrentSkip > 0;
    public bool CanGoNext     => CurrentSkip + PageSize < TotalCount;

    public string PageInfoText =>
        string.Format(_localizer["SoftwareCompilationsPageInfo"],
                      TotalCount == 0 ? 0 : (CurrentSkip / PageSize) + 1,
                      TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize));

    // --- Form fields ---
    [ObservableProperty] private string  _compilationName = string.Empty;

    [ObservableProperty] private string                              _softwareSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareDto>    _softwareSuggestions = [];
    [ObservableProperty] private SoftwareDto?                        _selectedSoftware;

    [ObservableProperty] private string                              _machineSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<MachineDto>     _machineSuggestions = [];
    [ObservableProperty] private MachineDto?                         _selectedMachine;

    [ObservableProperty] private string                                    _predecessorSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareCompilationDto> _predecessorSuggestions = [];
    [ObservableProperty] private SoftwareCompilationDto?                   _selectedPredecessor;

    [ObservableProperty] private int _selectedRelationshipTypeIndex;

    public List<string> RelationshipTypeItems { get; }

    // --- Included software junction ---
    [ObservableProperty] private ObservableCollection<SoftwareBySoftwareCompilationDto> _includedSoftware = [];
    [ObservableProperty] private string                                                  _includedSoftwareSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareDto>                       _includedSoftwareSuggestions = [];
    [ObservableProperty] private SoftwareDto?                                            _selectedIncludedSoftware;

    // --- Included versions junction (cascades from a chosen software) ---
    [ObservableProperty] private ObservableCollection<SoftwareVersionBySoftwareCompilationDto> _includedVersions = [];
    [ObservableProperty] private ObservableCollection<SoftwareVersionDto>                      _includedVersionSuggestions = [];
    [ObservableProperty] private SoftwareVersionDto?                                           _selectedIncludedVersion;

    // --- Included sub-compilations junction ---
    [ObservableProperty] private ObservableCollection<SoftwareCompilationDto> _includedCompilations = [];
    [ObservableProperty] private string                                       _includedCompilationSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareCompilationDto> _includedCompilationSuggestions = [];
    [ObservableProperty] private SoftwareCompilationDto?                     _selectedIncludedCompilation;

    // --- Merge ---
    [ObservableProperty] private bool                                         _isMerging;
    [ObservableProperty] private string                                       _mergeTargetSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareCompilationDto> _mergeTargetSuggestions = [];
    [ObservableProperty] private SoftwareCompilationDto?                      _selectedMergeTargetCompilation;
    [ObservableProperty] private string                                       _mergeSourceSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareCompilationDto> _mergeSourceSuggestions = [];
    [ObservableProperty] private SoftwareCompilationDto?                      _mergeSourcePickerValue;
    [ObservableProperty] private ObservableCollection<SoftwareCompilationDto> _mergeSelectedSources = [];

    public bool CanConfirmMerge => SelectedMergeTargetCompilation?.Id is not null && MergeSelectedSources.Count > 0;

    public AdminSoftwareCompilationsViewModel(SoftwareCompilationsService                 service,
                                               IJwtService                                 jwtService,
                                               ITokenService                               tokenService,
                                               ILogger<AdminSoftwareCompilationsViewModel> logger,
                                               IStringLocalizer                            localizer)
    {
        _service      = service;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        RelationshipTypeItems =
        [
            localizer["RelationshipTypeSequel"],
            localizer["RelationshipTypeFork"],
            localizer["RelationshipTypeRemake"],
            localizer["RelationshipTypeRemaster"],
            localizer["RelationshipTypePort"]
        ];

        LoadCommand         = new AsyncRelayCommand(LoadAsync);
        NextPageCommand     = new AsyncRelayCommand(NextPageAsync);
        PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync);
        OpenAddCommand       = new RelayCommand(OpenAdd);
        OpenEditCommand      = new AsyncRelayCommand<SoftwareCompilationDto>(OpenEditAsync);
        DeleteCommand        = new AsyncRelayCommand<SoftwareCompilationDto>(DeleteAsync);
        SaveCommand          = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand    = new RelayCommand(CancelEdit);

        AddIncludedSoftwareCommand    = new AsyncRelayCommand(AddIncludedSoftwareAsync);
        RemoveIncludedSoftwareCommand = new AsyncRelayCommand<SoftwareBySoftwareCompilationDto>(RemoveIncludedSoftwareAsync);
        AddIncludedVersionCommand     = new AsyncRelayCommand(AddIncludedVersionAsync);
        RemoveIncludedVersionCommand  = new AsyncRelayCommand<SoftwareVersionBySoftwareCompilationDto>(RemoveIncludedVersionAsync);
        AddIncludedCompilationCommand    = new AsyncRelayCommand(AddIncludedCompilationAsync);
        RemoveIncludedCompilationCommand = new AsyncRelayCommand<SoftwareCompilationDto>(RemoveIncludedCompilationAsync);

        OpenMergeCommand    = new RelayCommand(OpenMerge);
        ConfirmMergeCommand = new AsyncRelayCommand(ConfirmMergeAsync);
        CancelMergeCommand  = new RelayCommand(CancelMerge);
        RemoveMergeSourceCommand = new RelayCommand<SoftwareCompilationDto>(RemoveMergeSource);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                                  LoadCommand         { get; }
    public IAsyncRelayCommand                                  NextPageCommand     { get; }
    public IAsyncRelayCommand                                  PreviousPageCommand { get; }
    public IRelayCommand                                       OpenAddCommand      { get; }
    public IAsyncRelayCommand<SoftwareCompilationDto>          OpenEditCommand     { get; }
    public IAsyncRelayCommand<SoftwareCompilationDto>          DeleteCommand       { get; }
    public IAsyncRelayCommand                                  SaveCommand         { get; }
    public IRelayCommand                                       CancelEditCommand   { get; }

    public IAsyncRelayCommand                                            AddIncludedSoftwareCommand    { get; }
    public IAsyncRelayCommand<SoftwareBySoftwareCompilationDto>          RemoveIncludedSoftwareCommand { get; }
    public IAsyncRelayCommand                                            AddIncludedVersionCommand     { get; }
    public IAsyncRelayCommand<SoftwareVersionBySoftwareCompilationDto>   RemoveIncludedVersionCommand  { get; }
    public IAsyncRelayCommand                                            AddIncludedCompilationCommand    { get; }
    public IAsyncRelayCommand<SoftwareCompilationDto>                    RemoveIncludedCompilationCommand { get; }

    public IRelayCommand                              OpenMergeCommand         { get; }
    public IAsyncRelayCommand                         ConfirmMergeCommand      { get; }
    public IRelayCommand                              CancelMergeCommand       { get; }
    public IRelayCommand<SoftwareCompilationDto>      RemoveMergeSourceCommand { get; }

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
            _ = LoadCommand.ExecuteAsync(null);
    }

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

            if(!_jwtService.IsTokenValid(token)) { IsAdmin = false; return; }
            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("UberAdmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    // --- List / paging ---

    private async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            HasError  = false;
            ErrorMessage = string.Empty;

            Task<List<SoftwareCompilationDto>> dataTask  = _service.GetPagedAsync(CurrentSkip, PageSize, NullIfEmpty(SearchText));
            Task<int>                          countTask = _service.GetCountAsync(NullIfEmpty(SearchText));

            await Task.WhenAll(dataTask, countTask);

            Compilations.Clear();

            foreach(SoftwareCompilationDto item in dataTask.Result)
                Compilations.Add(item);

            TotalCount = countTask.Result;
            IsDataLoaded = true;

            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(PageInfoText));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software compilations");
            ErrorMessage = _localizer["FailedToLoadSoftwareCompilations"];
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SearchAsync()
    {
        CurrentSkip = 0;
        await LoadAsync();
    }

    private async Task NextPageAsync()
    {
        if(!CanGoNext) return;
        CurrentSkip += PageSize;
        await LoadAsync();
    }

    private async Task PreviousPageAsync()
    {
        if(!CanGoPrevious) return;
        CurrentSkip = Math.Max(0, CurrentSkip - PageSize);
        await LoadAsync();
    }

    // --- Add / edit / delete ---

    private void OpenAdd()
    {
        _editingId        = null;
        EditPanelTitle     = _localizer["AddSoftwareCompilationDialog_Title"];
        IsEditingExisting  = false;
        ClearForm();
        IsEditing = true;
    }

    private async Task OpenEditAsync(SoftwareCompilationDto? item)
    {
        if(item?.Id == null) return;

        SoftwareCompilationDto? full = await _service.GetAsync(item.Id.Value);

        if(full == null)
        {
            ErrorMessage = _localizer["FailedToLoadSoftwareCompilations"];
            HasError = true;

            return;
        }

        _editingId        = full.Id!.Value;
        EditPanelTitle    = _localizer["EditSoftwareCompilationDialog_Title"];
        IsEditingExisting = true;
        CompilationName   = full.Name ?? string.Empty;

        SelectedSoftware   = full.SoftwareId.HasValue
            ? new SoftwareDto { Id = full.SoftwareId.Value, Name = full.Software }
            : null;
        SoftwareSearchText = SelectedSoftware?.Name ?? string.Empty;

        await EnsureMachinesLoadedAsync();
        SelectedMachine   = full.MachineId.HasValue
            ? _allMachines?.FirstOrDefault(m => m.Id == full.MachineId.Value)
            : null;
        MachineSearchText = SelectedMachine?.Name ?? string.Empty;

        SelectedPredecessor   = full.PredecessorId.HasValue
            ? new SoftwareCompilationDto { Id = full.PredecessorId, Name = full.Predecessor }
            : null;
        PredecessorSearchText = SelectedPredecessor?.Name ?? string.Empty;

        SelectedRelationshipTypeIndex = full.RelationshipType ?? 0;

        HasError = false;
        ErrorMessage = string.Empty;
        IsEditing = true;

        await LoadJunctionsAsync(_editingId.Value);
    }

    private async Task DeleteAsync(SoftwareCompilationDto? item)
    {
        if(item?.Id == null) return;

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer, item.Name))
            return;

        try
        {
            await _service.DeleteAsync(item.Id.Value);
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software compilation {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteSoftwareCompilation"];
            HasError = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(CompilationName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError = true;

                return;
            }

            var dto = new SoftwareCompilationDto
            {
                Name             = CompilationName,
                SoftwareId       = SelectedSoftware?.Id,
                MachineId        = SelectedMachine?.Id,
                PredecessorId    = SelectedPredecessor?.Id,
                RelationshipType = SelectedRelationshipTypeIndex
            };

            if(_editingId == null)
            {
                int? newId = await _service.CreateAsync(dto);

                if(!newId.HasValue)
                {
                    ErrorMessage = _localizer["FailedToSaveSoftwareCompilation"];
                    HasError = true;

                    return;
                }

                _editingId = newId.Value;
            }
            else
            {
                dto.Id = _editingId.Value;
                bool ok = await _service.UpdateAsync(dto);

                if(!ok)
                {
                    ErrorMessage = _localizer["FailedToSaveSoftwareCompilation"];
                    HasError = true;

                    return;
                }
            }

            IsEditing = false;
            ClearForm();
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving software compilation");
            ErrorMessage = _localizer["FailedToSaveSoftwareCompilation"];
            HasError = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing  = false;
        _editingId = null;
        ClearForm();
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private void ClearForm()
    {
        CompilationName = string.Empty;

        SelectedSoftware   = null;
        SoftwareSearchText = string.Empty;
        SoftwareSuggestions.Clear();

        SelectedMachine   = null;
        MachineSearchText = string.Empty;
        MachineSuggestions.Clear();

        SelectedPredecessor   = null;
        PredecessorSearchText = string.Empty;
        PredecessorSuggestions.Clear();

        SelectedRelationshipTypeIndex = 0;

        IncludedSoftware.Clear();
        IncludedSoftwareSearchText = string.Empty;
        IncludedSoftwareSuggestions.Clear();
        SelectedIncludedSoftware = null;

        IncludedVersions.Clear();
        IncludedVersionSuggestions.Clear();
        SelectedIncludedVersion = null;
        _selectedIncludedSoftwareForVersions = null;

        IncludedCompilations.Clear();
        IncludedCompilationSearchText = string.Empty;
        IncludedCompilationSuggestions.Clear();
        SelectedIncludedCompilation = null;

        HasError = false;
        ErrorMessage = string.Empty;
    }

    // --- Pickers (debounced server search) ---

    public void UpdateSoftwareSuggestions(string query) => Debounce(ref _softwarePickerDebounce, async () =>
    {
        List<SoftwareDto> results = await _service.SearchSoftwareAsync(query);
        SoftwareSuggestions.Clear();
        foreach(SoftwareDto s in results) SoftwareSuggestions.Add(s);
    });

    public async Task EnsureMachinesLoadedAsync()
    {
        if(_allMachines != null) return;

        _allMachines = await _service.GetAllMachinesAsync();
    }

    public void UpdateMachineSuggestions(string query)
    {
        MachineSuggestions.Clear();

        if(_allMachines == null) return;

        IEnumerable<MachineDto> source = _allMachines;

        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(m => m.Name != null && m.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(MachineDto m in source) MachineSuggestions.Add(m);
    }

    public void UpdatePredecessorSuggestions(string query) => Debounce(ref _predecessorPickerDebounce, async () =>
    {
        List<SoftwareCompilationDto> results = await _service.SearchAsync(query);

        if(_editingId.HasValue)
            results = results.Where(c => c.Id != _editingId.Value).ToList();

        PredecessorSuggestions.Clear();
        foreach(SoftwareCompilationDto c in results) PredecessorSuggestions.Add(c);
    });

    public void UpdateIncludedSoftwareSuggestions(string query) => Debounce(ref _includedSoftwareDebounce, async () =>
    {
        List<SoftwareDto> results = await _service.SearchSoftwareAsync(query);
        IncludedSoftwareSuggestions.Clear();
        foreach(SoftwareDto s in results) IncludedSoftwareSuggestions.Add(s);
    });

    public void UpdateIncludedCompilationSuggestions(string query) => Debounce(ref _includedCompilationDebounce, async () =>
    {
        List<SoftwareCompilationDto> results = await _service.SearchAsync(query);

        if(_editingId.HasValue)
            results = results.Where(c => c.Id != _editingId.Value).ToList();

        IncludedCompilationSuggestions.Clear();
        foreach(SoftwareCompilationDto c in results) IncludedCompilationSuggestions.Add(c);
    });

    private static void Debounce(ref CancellationTokenSource? slot, Func<Task> action)
    {
        slot?.Cancel();
        var cts = new CancellationTokenSource();
        slot = cts;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(SearchDebounceMs, cts.Token);

                if(!cts.Token.IsCancellationRequested)
                    await action();
            }
            catch(TaskCanceledException) { }
        });
    }

    // --- Junctions ---

    private async Task LoadJunctionsAsync(int id)
    {
        IncludedSoftware.Clear();
        IncludedVersions.Clear();
        IncludedCompilations.Clear();

        try
        {
            foreach(SoftwareBySoftwareCompilationDto item in await _service.GetIncludedSoftwareAsync(id))
                IncludedSoftware.Add(item);

            foreach(SoftwareVersionBySoftwareCompilationDto item in await _service.GetIncludedVersionsAsync(id))
                IncludedVersions.Add(item);

            foreach(SoftwareCompilationDto item in await _service.GetIncludedCompilationsAsync(id))
                IncludedCompilations.Add(item);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading junctions for compilation {Id}", id);
        }
    }

    private async Task AddIncludedSoftwareAsync()
    {
        if(_editingId == null || SelectedIncludedSoftware?.Id == null) return;

        var dto = new SoftwareBySoftwareCompilationDto
        {
            SoftwareCompilationId = _editingId,
            SoftwareId            = SelectedIncludedSoftware.Id
        };

        if(await _service.AddIncludedSoftwareAsync(dto))
        {
            await LoadJunctionsAsync(_editingId.Value);
            SelectedIncludedSoftware = null;
            IncludedSoftwareSearchText = string.Empty;
            IncludedSoftwareSuggestions.Clear();
        }
    }

    private async Task RemoveIncludedSoftwareAsync(SoftwareBySoftwareCompilationDto? item)
    {
        if(_editingId == null || item?.SoftwareId == null) return;

        if(await _service.RemoveIncludedSoftwareAsync(_editingId.Value, item.SoftwareId.Value))
            await LoadJunctionsAsync(_editingId.Value);
    }

    public void SelectSoftwareForVersions(SoftwareDto? software)
    {
        _selectedIncludedSoftwareForVersions = software;
        IncludedVersionSuggestions.Clear();
        SelectedIncludedVersion = null;

        if(software?.Id == null) return;

        _ = Task.Run(async () =>
        {
            List<SoftwareVersionDto> versions = await _service.GetSoftwareVersionsAsync(software.Id.Value);

            foreach(SoftwareVersionDto v in versions)
                IncludedVersionSuggestions.Add(v);
        });
    }

    private async Task AddIncludedVersionAsync()
    {
        if(_editingId == null || SelectedIncludedVersion?.Id == null) return;

        var dto = new SoftwareVersionBySoftwareCompilationDto
        {
            SoftwareCompilationId = _editingId,
            SoftwareVersionId      = SelectedIncludedVersion.Id
        };

        if(await _service.AddIncludedVersionAsync(dto))
        {
            await LoadJunctionsAsync(_editingId.Value);
            SelectedIncludedVersion = null;
            IncludedVersionSuggestions.Clear();
        }
    }

    private async Task RemoveIncludedVersionAsync(SoftwareVersionBySoftwareCompilationDto? item)
    {
        if(_editingId == null || item?.SoftwareVersionId == null) return;

        if(await _service.RemoveIncludedVersionAsync(_editingId.Value, item.SoftwareVersionId.Value))
            await LoadJunctionsAsync(_editingId.Value);
    }

    private async Task AddIncludedCompilationAsync()
    {
        if(_editingId == null || SelectedIncludedCompilation?.Id == null) return;

        if(await _service.AddIncludedCompilationAsync(_editingId.Value, SelectedIncludedCompilation.Id.Value))
        {
            await LoadJunctionsAsync(_editingId.Value);
            SelectedIncludedCompilation = null;
            IncludedCompilationSearchText = string.Empty;
            IncludedCompilationSuggestions.Clear();
        }
    }

    private async Task RemoveIncludedCompilationAsync(SoftwareCompilationDto? item)
    {
        if(_editingId == null || item?.Id == null) return;

        if(await _service.RemoveIncludedCompilationAsync(_editingId.Value, item.Id.Value))
            await LoadJunctionsAsync(_editingId.Value);
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    // --- Merge ---

    public string MergeConfirmDialogTitle   => _localizer["MergeConfirmDialogTitle"];
    public string MergeConfirmDialogMessage => _localizer["MergeConfirmDialogMessage"];
    public string MergeButtonText           => _localizer["MergeButton"];
    public string CancelButtonText          => _localizer["CancelButton"];

    partial void OnSelectedMergeTargetCompilationChanged(SoftwareCompilationDto? value)
    {
        MergeSelectedSources.Clear();
        MergeSourceSearchText = string.Empty;
        MergeSourceSuggestions.Clear();
        OnPropertyChanged(nameof(CanConfirmMerge));
    }

    partial void OnMergeSourcePickerValueChanged(SoftwareCompilationDto? value)
    {
        if(value?.Id is null) return;

        if(MergeSelectedSources.All(s => s.Id != value.Id))
            MergeSelectedSources.Add(value);

        MergeSourcePickerValue = null;
        MergeSourceSearchText  = string.Empty;
        MergeSourceSuggestions.Clear();
        OnPropertyChanged(nameof(CanConfirmMerge));
    }

    private void OpenMerge()
    {
        CancelEdit();
        HasError     = false;
        ErrorMessage = string.Empty;
        IsMerging    = true;
        MergeTargetSearchText = string.Empty;
        MergeTargetSuggestions.Clear();
        SelectedMergeTargetCompilation = null;
        MergeSourceSearchText = string.Empty;
        MergeSourceSuggestions.Clear();
        MergeSelectedSources.Clear();
    }

    public void UpdateMergeTargetSuggestions(string query) => Debounce(ref _mergeTargetDebounce, async () =>
    {
        List<SoftwareCompilationDto> results = await _service.SearchAsync(query);
        MergeTargetSuggestions.Clear();
        foreach(SoftwareCompilationDto c in results) MergeTargetSuggestions.Add(c);
    });

    public void UpdateMergeSourceSuggestions(string query) => Debounce(ref _mergeSourceDebounce, async () =>
    {
        List<SoftwareCompilationDto> results = await _service.SearchAsync(query);

        MergeSourceSuggestions.Clear();

        foreach(SoftwareCompilationDto c in results)
            if(c.Id != SelectedMergeTargetCompilation?.Id && MergeSelectedSources.All(s => s.Id != c.Id))
                MergeSourceSuggestions.Add(c);
    });

    private void RemoveMergeSource(SoftwareCompilationDto? item)
    {
        if(item == null) return;

        MergeSelectedSources.Remove(item);
        OnPropertyChanged(nameof(CanConfirmMerge));
    }

    private async Task ConfirmMergeAsync()
    {
        if(SelectedMergeTargetCompilation?.Id is not int targetId) return;

        List<int> sourceIds = MergeSelectedSources
                              .Where(c => c.Id is not null)
                              .Select(c => c.Id!.Value)
                              .ToList();

        if(sourceIds.Count == 0) return;

        try
        {
            HasError     = false;
            ErrorMessage = string.Empty;

            bool succeeded = await _service.MergeAsync(targetId, sourceIds);

            if(!succeeded)
            {
                ErrorMessage = _localizer["FailedToMergeSoftwareCompilations"];
                HasError     = true;

                return;
            }

            CancelMerge();
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error merging software compilations into {TargetId}", targetId);
            ErrorMessage = _localizer["FailedToMergeSoftwareCompilations"];
            HasError     = true;
        }
    }

    private void CancelMerge()
    {
        IsMerging = false;
        MergeTargetSearchText = string.Empty;
        MergeTargetSuggestions.Clear();
        SelectedMergeTargetCompilation = null;
        MergeSourceSearchText = string.Empty;
        MergeSourceSuggestions.Clear();
        MergeSelectedSources.Clear();
        HasError     = false;
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(CanConfirmMerge));
    }
}
