#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Models;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwarePlatformsViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwarePlatformsService                    _service;
    private readonly IJwtService                                _jwtService;
    private readonly IStringLocalizer                           _localizer;
    private readonly ILogger<AdminSoftwarePlatformsViewModel>   _logger;
    private readonly ITokenService                              _tokenService;

    [ObservableProperty] private ObservableCollection<SoftwarePlatformDto> _platforms = [];
    [ObservableProperty] private ObservableCollection<SoftwarePlatformDto> _filteredPlatforms = [];
    [ObservableProperty] private string                                   _filterText = string.Empty;
    [ObservableProperty] private SoftwarePlatformDto?                     _selectedPlatform;
    [ObservableProperty] private bool                                      _isLoading;
    [ObservableProperty] private bool                                      _isDataLoaded;
    [ObservableProperty] private bool                                      _hasError;
    [ObservableProperty] private string                                   _errorMessage = string.Empty;
    [ObservableProperty] private bool                                      _isAdmin;
    [ObservableProperty] private bool                                      _isEditing;
    [ObservableProperty] private string                                   _editPanelTitle = string.Empty;
    [ObservableProperty] private string                                   _platformName = string.Empty;

    private int?                       _editingId;
    private List<SoftwarePlatformDto>? _allPlatforms;

    public AdminSoftwarePlatformsViewModel(SoftwarePlatformsService                  service,
                                           IJwtService                               jwtService,
                                           ITokenService                             tokenService,
                                           ILogger<AdminSoftwarePlatformsViewModel>  logger,
                                           IStringLocalizer                          localizer)
    {
        _service      = service;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadCommand   = new AsyncRelayCommand(LoadAsync);
        OpenAddCommand = new RelayCommand(OpenAdd);
        OpenEditCommand = new RelayCommand<SoftwarePlatformDto>(OpenEdit);
        DeleteCommand  = new AsyncRelayCommand<SoftwarePlatformDto>(DeleteAsync);
        SaveCommand    = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                       LoadCommand      { get; }
    public IRelayCommand                            OpenAddCommand   { get; }
    public IRelayCommand<SoftwarePlatformDto>       OpenEditCommand  { get; }
    public IAsyncRelayCommand<SoftwarePlatformDto>  DeleteCommand    { get; }
    public IAsyncRelayCommand                       SaveCommand      { get; }
    public IRelayCommand                            CancelEditCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin) _ = LoadCommand.ExecuteAsync(null);
    }

    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(string.IsNullOrWhiteSpace(token)) { IsAdmin = false; return; }

            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("UberAdmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch { IsAdmin = false; }
    }

    private async Task LoadAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Platforms.Clear();

            List<SoftwarePlatformDto> response = await _service.GetAllAsync();
            _allPlatforms = response;

            foreach(SoftwarePlatformDto item in response) Platforms.Add(item);

            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software platforms");
            ErrorMessage = _localizer["FailedToLoadSoftwarePlatforms"];
            HasError     = true;
        }
        finally { IsLoading = false; }
    }

    private void OpenAdd()
    {
        _editingId     = null;
        EditPanelTitle = _localizer["AddSoftwarePlatformDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    private void OpenEdit(SoftwarePlatformDto? item)
    {
        if(item == null) return;

        _editingId     = item.Id;
        EditPanelTitle = _localizer["EditSoftwarePlatformDialog_Title"];
        PlatformName   = item.Name ?? string.Empty;
        HasError       = false;
        ErrorMessage   = string.Empty;
        IsEditing      = true;
    }

    private async Task DeleteAsync(SoftwarePlatformDto? item)
    {
        if(item?.Id == null) return;

        try
        {
            await _service.DeleteAsync(item.Id.Value);
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software platform {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteSoftwarePlatform"];
            HasError     = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(PlatformName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError     = true;
                return;
            }

            var dto = new SoftwarePlatformDto { Name = PlatformName };

            if(_editingId == null)
                await _service.CreateAsync(dto);
            else
            {
                dto.Id = _editingId;
                await _service.UpdateAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving software platform");
            ErrorMessage = _localizer["FailedToSaveSoftwarePlatform"];
            HasError     = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing    = false;
        _editingId   = null;
        ClearForm();
        HasError     = false;
        ErrorMessage = string.Empty;
    }

    public void ApplyFilter()
    {
        FilteredPlatforms.Clear();
        IEnumerable<SoftwarePlatformDto> source = (IEnumerable<SoftwarePlatformDto>?)_allPlatforms ?? Platforms;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(p => p.Name != null &&
                                       p.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

        foreach(SoftwarePlatformDto item in source) FilteredPlatforms.Add(item);
    }

    private void ClearForm()
    {
        PlatformName = string.Empty;
        HasError     = false;
        ErrorMessage = string.Empty;
    }
}
