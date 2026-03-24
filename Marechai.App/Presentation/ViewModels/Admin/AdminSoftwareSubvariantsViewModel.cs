#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareSubvariantsViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareSubvariantsService                       _service;
    private readonly IJwtService                                     _jwtService;
    private readonly IStringLocalizer                                _localizer;
    private readonly ILogger<AdminSoftwareSubvariantsViewModel>      _logger;
    private readonly ITokenService                                   _tokenService;

    [ObservableProperty] private ObservableCollection<SoftwareSubvariantDto> _subvariants = [];
    [ObservableProperty] private ObservableCollection<SoftwareSubvariantDto> _filteredSubvariants = [];
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _subvariantName = string.Empty;

    private int? _editingId;
    private int  _parentVariantId;
    private List<SoftwareSubvariantDto>? _allSubvariants;

    public AdminSoftwareSubvariantsViewModel(SoftwareSubvariantsService                  service,
                                             IJwtService                                jwtService,
                                             ITokenService                              tokenService,
                                             ILogger<AdminSoftwareSubvariantsViewModel>  logger,
                                             IStringLocalizer                           localizer)
    {
        _service      = service;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadCommand       = new AsyncRelayCommand(LoadAsync);
        OpenAddCommand    = new RelayCommand(OpenAdd);
        OpenEditCommand   = new RelayCommand<SoftwareSubvariantDto>(OpenEdit);
        DeleteCommand     = new AsyncRelayCommand<SoftwareSubvariantDto>(DeleteAsync);
        SaveCommand       = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                           LoadCommand       { get; }
    public IRelayCommand                                OpenAddCommand    { get; }
    public IRelayCommand<SoftwareSubvariantDto>         OpenEditCommand   { get; }
    public IAsyncRelayCommand<SoftwareSubvariantDto>    DeleteCommand     { get; }
    public IAsyncRelayCommand                           SaveCommand       { get; }
    public IRelayCommand                                CancelEditCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();
        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareVariantId, out int variantId))
            _parentVariantId = variantId;
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.SoftwareVariantName, out string? name))
            PageTitle = string.Format(_localizer["SoftwareSubvariantsForTitle"], name);
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
            IsLoading = true; HasError = false;
            Subvariants.Clear();
            List<SoftwareSubvariantDto> response = await _service.GetByVariantAsync(_parentVariantId);
            _allSubvariants = response;
            foreach(SoftwareSubvariantDto item in response) Subvariants.Add(item);
            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading subvariants");
            ErrorMessage = _localizer["FailedToLoadSoftwareSubvariants"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    private void OpenAdd()
    {
        _editingId = null;
        EditPanelTitle = _localizer["AddSoftwareSubvariantDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    private void OpenEdit(SoftwareSubvariantDto? item)
    {
        if(item == null) return;
        _editingId      = item.Id;
        EditPanelTitle  = _localizer["EditSoftwareSubvariantDialog_Title"];
        SubvariantName  = item.Name ?? string.Empty;
        HasError = false; ErrorMessage = string.Empty;
        IsEditing = true;
    }

    private async Task DeleteAsync(SoftwareSubvariantDto? item)
    {
        if(item?.Id == null) return;
        try { await _service.DeleteAsync(item.Id.Value); await LoadAsync(); }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting subvariant {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteSoftwareSubvariant"];
            HasError = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(SubvariantName))
            {
                ErrorMessage = _localizer["NameIsRequired"]; HasError = true; return;
            }

            var dto = new SoftwareSubvariantDto
            {
                VariantId = _parentVariantId,
                Name      = SubvariantName
            };

            if(_editingId == null) await _service.CreateAsync(dto);
            else { dto.Id = _editingId; await _service.UpdateAsync(dto); }

            IsEditing = false; ClearForm();
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving subvariant");
            ErrorMessage = _localizer["FailedToSaveSoftwareSubvariant"];
            HasError = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing = false; _editingId = null;
        ClearForm();
        HasError = false; ErrorMessage = string.Empty;
    }

    public void ApplyFilter()
    {
        FilteredSubvariants.Clear();
        IEnumerable<SoftwareSubvariantDto> source = (IEnumerable<SoftwareSubvariantDto>?)_allSubvariants ?? Subvariants;
        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(s => s.Name != null && s.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
        foreach(SoftwareSubvariantDto item in source) FilteredSubvariants.Add(item);
    }

    private void ClearForm()
    {
        SubvariantName = string.Empty;
        HasError = false; ErrorMessage = string.Empty;
    }
}
