#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Presentation.Dialogs;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminExternalSitesViewModel : ObservableObject, IRegionAware
{
    private readonly ExternalSitesService                  _service;
    private readonly IJwtService                           _jwtService;
    private readonly IStringLocalizer                      _localizer;
    private readonly ILogger<AdminExternalSitesViewModel>  _logger;
    private readonly ITokenService                          _tokenService;

    [ObservableProperty] private ObservableCollection<ExternalSiteDto> _sites = [];
    [ObservableProperty] private ObservableCollection<ExternalSiteDto> _filteredSites = [];
    [ObservableProperty] private string                                _filterText = string.Empty;
    [ObservableProperty] private ExternalSiteDto?                      _selectedSite;
    [ObservableProperty] private bool                                   _isLoading;
    [ObservableProperty] private bool                                   _isDataLoaded;
    [ObservableProperty] private bool                                   _hasError;
    [ObservableProperty] private string                                _errorMessage = string.Empty;
    [ObservableProperty] private bool                                   _isAdmin;
    [ObservableProperty] private bool                                   _isEditing;
    [ObservableProperty] private string                                _editPanelTitle = string.Empty;
    [ObservableProperty] private string                                _siteName = string.Empty;
    [ObservableProperty] private string                                _urlTemplate = string.Empty;

    private long? _editingId;
    private List<ExternalSiteDto>? _allSites;

    public AdminExternalSitesViewModel(ExternalSitesService                  service,
                                       IJwtService                           jwtService,
                                       ITokenService                         tokenService,
                                       ILogger<AdminExternalSitesViewModel>  logger,
                                       IStringLocalizer                      localizer)
    {
        _service      = service;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadCommand       = new AsyncRelayCommand(LoadAsync);
        OpenAddCommand     = new RelayCommand(OpenAdd);
        OpenEditCommand    = new RelayCommand<ExternalSiteDto>(OpenEdit);
        DeleteCommand      = new AsyncRelayCommand<ExternalSiteDto>(DeleteAsync);
        SaveCommand        = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand  = new RelayCommand(CancelEdit);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                  LoadCommand      { get; }
    public IRelayCommand                       OpenAddCommand   { get; }
    public IRelayCommand<ExternalSiteDto>      OpenEditCommand  { get; }
    public IAsyncRelayCommand<ExternalSiteDto> DeleteCommand    { get; }
    public IAsyncRelayCommand                  SaveCommand      { get; }
    public IRelayCommand                       CancelEditCommand { get; }

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
            Sites.Clear();

            List<ExternalSiteDto> response = await _service.GetAllAsync();
            _allSites = response;

            foreach(ExternalSiteDto item in response) Sites.Add(item);

            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading external sites");
            ErrorMessage = _localizer["FailedToLoadExternalSites"];
            HasError     = true;
        }
        finally { IsLoading = false; }
    }

    private void OpenAdd()
    {
        _editingId     = null;
        EditPanelTitle = _localizer["AddExternalSiteDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    private void OpenEdit(ExternalSiteDto? item)
    {
        if(item == null) return;

        _editingId     = item.Id;
        EditPanelTitle = _localizer["EditExternalSiteDialog_Title"];
        SiteName       = item.Name ?? string.Empty;
        UrlTemplate    = item.UrlTemplate ?? string.Empty;
        HasError       = false;
        ErrorMessage   = string.Empty;
        IsEditing      = true;
    }

    private async Task DeleteAsync(ExternalSiteDto? item)
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
            _logger.LogError(ex, "Error deleting external site {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteExternalSite"];
            HasError     = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(SiteName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError     = true;

                return;
            }

            var dto = new ExternalSiteDto
            {
                Name        = SiteName,
                UrlTemplate = string.IsNullOrWhiteSpace(UrlTemplate) ? null : UrlTemplate
            };

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
            _logger.LogError(ex, "Error saving external site");
            ErrorMessage = _localizer["FailedToSaveExternalSite"];
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
        FilteredSites.Clear();
        IEnumerable<ExternalSiteDto> source = (IEnumerable<ExternalSiteDto>?)_allSites ?? Sites;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(s => s.Name != null &&
                                       s.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

        foreach(ExternalSiteDto item in source) FilteredSites.Add(item);
    }

    private void ClearForm()
    {
        SiteName     = string.Empty;
        UrlTemplate  = string.Empty;
        HasError     = false;
        ErrorMessage = string.Empty;
    }
}
