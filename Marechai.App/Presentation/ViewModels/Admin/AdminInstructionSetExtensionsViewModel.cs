#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminInstructionSetExtensionsViewModel : ObservableObject, IRegionAware
{
    private readonly ApiClient                                        _apiClient;
    private readonly IJwtService                                      _jwtService;
    private readonly IStringLocalizer                                 _localizer;
    private readonly ILogger<AdminInstructionSetExtensionsViewModel>   _logger;
    private readonly ITokenService                                    _tokenService;

    // --- List state ---
    [ObservableProperty]
    private ObservableCollection<InstructionSetExtensionDto> _extensions = [];

    [ObservableProperty]
    private ObservableCollection<InstructionSetExtensionDto> _filteredExtensions = [];

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private InstructionSetExtensionDto? _selectedExtension;

    private List<InstructionSetExtensionDto>? _allExtensions;

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
    private string _extensionName = string.Empty;

    [ObservableProperty]
    private bool _isNameUnique = true;

    [ObservableProperty]
    private string _uniquenessMessage = string.Empty;

    public AdminInstructionSetExtensionsViewModel(ApiClient                                        apiClient,
                                                  IJwtService                                      jwtService,
                                                  ITokenService                                    tokenService,
                                                  ILogger<AdminInstructionSetExtensionsViewModel>   logger,
                                                  IStringLocalizer                                 localizer)
    {
        _apiClient    = apiClient;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadItemsCommand  = new AsyncRelayCommand(LoadItemsAsync);
        OpenAddCommand    = new RelayCommand(OpenAdd);
        OpenEditCommand   = new RelayCommand<InstructionSetExtensionDto>(OpenEdit);
        DeleteCommand     = new AsyncRelayCommand<InstructionSetExtensionDto>(DeleteAsync);
        SaveCommand       = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);

        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand                              LoadItemsCommand  { get; }
    public IRelayCommand                                   OpenAddCommand    { get; }
    public IRelayCommand<InstructionSetExtensionDto>        OpenEditCommand   { get; }
    public IAsyncRelayCommand<InstructionSetExtensionDto>   DeleteCommand     { get; }
    public IAsyncRelayCommand                              SaveCommand       { get; }
    public IRelayCommand                                   CancelEditCommand { get; }

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
            _ = LoadItemsCommand.ExecuteAsync(null);
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

            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("Uberadmin",      StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Extensions.Clear();

            List<InstructionSetExtensionDto>? response = await _apiClient.InstructionSetExtensions.GetAsync();
            _allExtensions = response;

            if(response != null)
                foreach(InstructionSetExtensionDto item in response)
                    Extensions.Add(item);

            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading instruction set extensions");
            ErrorMessage = _localizer["FailedToLoadISExtensions"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenAdd()
    {
        _editingId     = null;
        EditPanelTitle = _localizer["AddISExtensionDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    private void OpenEdit(InstructionSetExtensionDto? item)
    {
        if(item == null) return;

        _editingId        = item.Id;
        EditPanelTitle    = _localizer["EditISExtensionDialog_Title"];
        ExtensionName     = item.Extension ?? string.Empty;
        IsNameUnique      = true;
        UniquenessMessage = string.Empty;
        HasError          = false;
        ErrorMessage      = string.Empty;
        IsEditing         = true;
    }

    private async Task DeleteAsync(InstructionSetExtensionDto? item)
    {
        if(item?.Id == null) return;

        try
        {
            await _apiClient.InstructionSetExtensions[item.Id.Value].DeleteAsync();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting instruction set extension {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteISExtension"];
            HasError     = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(ExtensionName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError     = true;
                return;
            }

            bool? unique = await _apiClient.InstructionSetExtensions.VerifyUnique[ExtensionName].GetAsync();

            if(unique == false)
            {
                bool isSameName = _editingId.HasValue &&
                                  _allExtensions != null &&
                                  _allExtensions.Any(s => s.Id == _editingId.Value &&
                                                          string.Equals(s.Extension, ExtensionName,
                                                                        StringComparison.OrdinalIgnoreCase));

                if(!isSameName)
                {
                    IsNameUnique      = false;
                    UniquenessMessage = _localizer["ISExtensionNameNotUnique"];
                    ErrorMessage      = _localizer["ISExtensionNameNotUnique"];
                    HasError          = true;
                    return;
                }
            }

            var dto = new InstructionSetExtensionDto
            {
                Extension = ExtensionName
            };

            if(_editingId == null)
                await _apiClient.InstructionSetExtensions.PostAsync(dto);
            else
            {
                dto.Id = _editingId;
                await _apiClient.InstructionSetExtensions[_editingId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving instruction set extension");
            ErrorMessage = _localizer["FailedToSaveISExtension"];
            HasError     = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing  = false;
        _editingId = null;
        ClearForm();
        HasError     = false;
        ErrorMessage = string.Empty;
    }

    public async Task VerifyNameUniquenessAsync()
    {
        if(string.IsNullOrWhiteSpace(ExtensionName))
        {
            IsNameUnique      = true;
            UniquenessMessage = string.Empty;
            return;
        }

        try
        {
            bool? unique = await _apiClient.InstructionSetExtensions.VerifyUnique[ExtensionName].GetAsync();

            if(unique == false && _editingId.HasValue && _allExtensions != null)
            {
                bool isSameName = _allExtensions.Any(s => s.Id == _editingId.Value &&
                                                          string.Equals(s.Extension, ExtensionName,
                                                                        StringComparison.OrdinalIgnoreCase));

                if(isSameName)
                {
                    IsNameUnique      = true;
                    UniquenessMessage = string.Empty;
                    return;
                }
            }

            IsNameUnique      = unique ?? true;
            UniquenessMessage = IsNameUnique ? string.Empty : _localizer["ISExtensionNameNotUnique"];
        }
        catch
        {
            IsNameUnique      = true;
            UniquenessMessage = string.Empty;
        }
    }

    public void ApplyFilter()
    {
        FilteredExtensions.Clear();

        IEnumerable<InstructionSetExtensionDto> source =
            (IEnumerable<InstructionSetExtensionDto>?)_allExtensions ?? Extensions;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(s => s.Extension != null &&
                                       s.Extension.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

        foreach(InstructionSetExtensionDto item in source)
            FilteredExtensions.Add(item);
    }

    private void ClearForm()
    {
        ExtensionName     = string.Empty;
        IsNameUnique      = true;
        UniquenessMessage = string.Empty;
        HasError          = false;
        ErrorMessage      = string.Empty;
    }
}
