#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Presentation.Dialogs;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminInstructionSetsViewModel : ObservableObject, IRegionAware
{
    private readonly Client                                _apiClient;
    private readonly IJwtService                              _jwtService;
    private readonly IStringLocalizer                         _localizer;
    private readonly ILogger<AdminInstructionSetsViewModel>   _logger;
    private readonly ITokenService                            _tokenService;

    // --- List state ---
    [ObservableProperty]
    private ObservableCollection<InstructionSetDto> _instructionSets = [];

    [ObservableProperty]
    private ObservableCollection<InstructionSetDto> _filteredInstructionSets = [];

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private InstructionSetDto? _selectedInstructionSet;

    private List<InstructionSetDto>? _allInstructionSets;

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

    private int? _editingId;

    // --- Form fields ---
    [ObservableProperty]
    private string _instructionSetName = string.Empty;

    [ObservableProperty]
    private bool _isNameUnique = true;

    [ObservableProperty]
    private string _uniquenessMessage = string.Empty;

    public AdminInstructionSetsViewModel(Client                                apiClient,
                                         IJwtService                              jwtService,
                                         ITokenService                            tokenService,
                                         ILogger<AdminInstructionSetsViewModel>   logger,
                                         IStringLocalizer                         localizer)
    {
        _apiClient    = apiClient;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadItemsCommand  = new AsyncRelayCommand(LoadItemsAsync);
        OpenAddCommand    = new RelayCommand(OpenAdd);
        OpenEditCommand   = new RelayCommand<InstructionSetDto>(OpenEdit);
        DeleteCommand     = new AsyncRelayCommand<InstructionSetDto>(DeleteAsync);
        SaveCommand       = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);

        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand                    LoadItemsCommand  { get; }
    public IRelayCommand                         OpenAddCommand    { get; }
    public IRelayCommand<InstructionSetDto>       OpenEditCommand   { get; }
    public IAsyncRelayCommand<InstructionSetDto>  DeleteCommand     { get; }
    public IAsyncRelayCommand                    SaveCommand       { get; }
    public IRelayCommand                         CancelEditCommand { get; }

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
            _ = LoadItemsCommand.ExecuteAsync(null);
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

            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    // --- Load ---
    private async Task LoadItemsAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            InstructionSets.Clear();

            List<InstructionSetDto>? response = await _apiClient.InstructionSets.GetAsync();
            _allInstructionSets = response;

            if(response != null)
                foreach(InstructionSetDto item in response)
                    InstructionSets.Add(item);

            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading instruction sets");
            ErrorMessage = _localizer["FailedToLoadInstructionSets"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- Add ---
    private void OpenAdd()
    {
        _editingId     = null;
        EditPanelTitle = _localizer["AddInstructionSetDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    // --- Edit ---
    private void OpenEdit(InstructionSetDto? item)
    {
        if(item == null) return;

        _editingId         = item.Id;
        EditPanelTitle     = _localizer["EditInstructionSetDialog_Title"];
        InstructionSetName = item.Name ?? string.Empty;
        IsNameUnique       = true;
        UniquenessMessage  = string.Empty;
        HasError           = false;
        ErrorMessage       = string.Empty;
        IsEditing          = true;
    }

    // --- Delete ---
    private async Task DeleteAsync(InstructionSetDto? item)
    {
        if(item?.Id == null) return;

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer, item.Name))
            return;

        try
        {
            await _apiClient.InstructionSets[item.Id.Value].DeleteAsync();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting instruction set {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteInstructionSet"];
            HasError     = true;
        }
    }

    // --- Save ---
    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(InstructionSetName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError     = true;

                return;
            }

            // Verify uniqueness before saving
            bool? unique = await _apiClient.InstructionSets.VerifyUnique[InstructionSetName].GetAsync();

            // When editing, the current name is allowed (it's the same record)
            if(unique == false)
            {
                // Check if it's the same record being edited
                bool isSameName = _editingId.HasValue &&
                                  _allInstructionSets != null &&
                                  _allInstructionSets.Any(s => s.Id == _editingId.Value &&
                                                               string.Equals(s.Name, InstructionSetName,
                                                                             StringComparison.OrdinalIgnoreCase));

                if(!isSameName)
                {
                    IsNameUnique      = false;
                    UniquenessMessage = _localizer["InstructionSetNameNotUnique"];
                    ErrorMessage      = _localizer["InstructionSetNameNotUnique"];
                    HasError          = true;

                    return;
                }
            }

            var dto = new InstructionSetDto
            {
                Name = InstructionSetName
            };

            if(_editingId == null)
                await _apiClient.InstructionSets.PostAsync(dto);
            else
            {
                dto.Id = _editingId;
                await _apiClient.InstructionSets[_editingId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving instruction set");
            ErrorMessage = _localizer["FailedToSaveInstructionSet"];
            HasError     = true;
        }
    }

    // --- Cancel ---
    private void CancelEdit()
    {
        IsEditing  = false;
        _editingId = null;
        ClearForm();
        HasError     = false;
        ErrorMessage = string.Empty;
    }

    // --- Verify uniqueness (called from UI on text change) ---
    public async Task VerifyNameUniquenessAsync()
    {
        if(string.IsNullOrWhiteSpace(InstructionSetName))
        {
            IsNameUnique      = true;
            UniquenessMessage = string.Empty;

            return;
        }

        try
        {
            bool? unique = await _apiClient.InstructionSets.VerifyUnique[InstructionSetName].GetAsync();

            // Allow same name if editing the same record
            if(unique == false && _editingId.HasValue && _allInstructionSets != null)
            {
                bool isSameName = _allInstructionSets.Any(s => s.Id == _editingId.Value &&
                                                               string.Equals(s.Name, InstructionSetName,
                                                                             StringComparison.OrdinalIgnoreCase));

                if(isSameName)
                {
                    IsNameUnique      = true;
                    UniquenessMessage = string.Empty;

                    return;
                }
            }

            IsNameUnique      = unique ?? true;
            UniquenessMessage = IsNameUnique ? string.Empty : _localizer["InstructionSetNameNotUnique"];
        }
        catch
        {
            // On error, assume unique to not block the user
            IsNameUnique      = true;
            UniquenessMessage = string.Empty;
        }
    }

    // --- Filtering ---
    public void ApplyFilter()
    {
        FilteredInstructionSets.Clear();

        IEnumerable<InstructionSetDto> source = (IEnumerable<InstructionSetDto>?)_allInstructionSets ?? InstructionSets;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(s => s.Name != null &&
                                       s.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

        foreach(InstructionSetDto item in source)
            FilteredInstructionSets.Add(item);
    }

    // --- Helpers ---
    private void ClearForm()
    {
        InstructionSetName = string.Empty;
        IsNameUnique       = true;
        UniquenessMessage  = string.Empty;
        HasError           = false;
        ErrorMessage       = string.Empty;
    }
}
