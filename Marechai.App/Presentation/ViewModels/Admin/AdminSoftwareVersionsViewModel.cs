#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Dialogs;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareVersionsViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareVersionsService                     _service;
    private readonly Client                                   _apiClient;
    private readonly IJwtService                                 _jwtService;
    private readonly IStringLocalizer                            _localizer;
    private readonly ILogger<AdminSoftwareVersionsViewModel>     _logger;
    private readonly ITokenService                               _tokenService;
    private readonly IRegionManager                              _regionManager;

    [ObservableProperty] private ObservableCollection<SoftwareVersionDto> _versions = [];
    [ObservableProperty] private ObservableCollection<SoftwareVersionDto> _filteredVersions = [];
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private SoftwareVersionDto? _selectedVersion;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isEditingExisting;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    [ObservableProperty] private string _pageTitle = string.Empty;

    // Form
    [ObservableProperty] private string _codename = string.Empty;
    [ObservableProperty] private string _versionString = string.Empty;
    [ObservableProperty] private string _publicVersion = string.Empty;
    [ObservableProperty] private SoftwareVersionDto? _selectedParentVersion;
    [ObservableProperty] private string _parentVersionSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareVersionDto> _parentVersionSuggestions = [];

    // Companies junction
    [ObservableProperty] private ObservableCollection<CompanyBySoftwareVersionDto> _versionCompanies = [];
    [ObservableProperty] private ObservableCollection<string> _versionCompanyDisplays = [];
    [ObservableProperty] private CompanyDto? _selectedCompany;
    [ObservableProperty] private string _companySearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyDto> _companySuggestions = [];
    [ObservableProperty] private SoftwareRoleDto? _selectedRole;
    [ObservableProperty] private ObservableCollection<SoftwareRoleDto> _roles = [];

    private int? _editingId;
    private int  _parentSoftwareId;
    private List<SoftwareVersionDto>? _allVersions;
    private List<CompanyDto>? _allCompanies;
    private List<SoftwareRoleDto>? _allRoles;

    public AdminSoftwareVersionsViewModel(SoftwareVersionsService                  service,
                                          Client                                   apiClient,
                                          IJwtService                              jwtService,
                                          ITokenService                            tokenService,
                                          ILogger<AdminSoftwareVersionsViewModel>  logger,
                                          IStringLocalizer                         localizer,
                                          IRegionManager                           regionManager)
    {
        _service       = service;
        _apiClient     = apiClient;
        _jwtService    = jwtService;
        _tokenService  = tokenService;
        _logger        = logger;
        _localizer     = localizer;
        _regionManager = regionManager;

        LoadCommand            = new AsyncRelayCommand(LoadAsync);
        OpenAddCommand         = new RelayCommand(OpenAdd);
        OpenEditCommand        = new RelayCommand<SoftwareVersionDto>(OpenEdit);
        DeleteCommand          = new AsyncRelayCommand<SoftwareVersionDto>(DeleteAsync);
        SaveCommand            = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand      = new RelayCommand(CancelEdit);
        GoBackCommand          = new RelayCommand(GoBack);
        AddCompanyCommand      = new AsyncRelayCommand(AddCompanyAsync);
        RemoveCompanyCommand   = new AsyncRelayCommand<string>(RemoveCompanyByDisplayAsync);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                        LoadCommand            { get; }
    public IRelayCommand                             OpenAddCommand         { get; }
    public IRelayCommand<SoftwareVersionDto>         OpenEditCommand        { get; }
    public IAsyncRelayCommand<SoftwareVersionDto>    DeleteCommand          { get; }
    public IAsyncRelayCommand                        SaveCommand            { get; }
    public IRelayCommand                             CancelEditCommand      { get; }
    public IRelayCommand                             GoBackCommand          { get; }
    public IAsyncRelayCommand                        AddCompanyCommand      { get; }
    public IAsyncRelayCommand<string>                RemoveCompanyCommand   { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    void GoBack() => _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoftwarePage));

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();
        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareId, out int softwareId))
            _parentSoftwareId = softwareId;
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.SoftwareName, out string? name))
            PageTitle = string.Format(_localizer["SoftwareVersionsForTitle"], name);

        if(IsAdmin)
        {
            _ = LoadPickerDataAsync();
            _ = LoadCommand.ExecuteAsync(null);
        }
    }

    public async Task LoadPickerDataAsync()
    {
        try { _allCompanies = await _apiClient.Companies.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading companies for picker"); }

        try
        {
            _allRoles = await _apiClient.Software.Roles.Enabled.GetAsync();
            Roles.Clear();
            if(_allRoles != null)
                foreach(SoftwareRoleDto r in _allRoles) Roles.Add(r);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading software roles for picker"); }
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
            Versions.Clear();
            List<SoftwareVersionDto> response = await _service.GetBySoftwareAsync(_parentSoftwareId);
            _allVersions = response;
            foreach(SoftwareVersionDto item in response) Versions.Add(item);
            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading versions for software {Id}", _parentSoftwareId);
            ErrorMessage = _localizer["FailedToLoadSoftwareVersions"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    private void OpenAdd()
    {
        _editingId = null;
        EditPanelTitle = _localizer["AddSoftwareVersionDialog_Title"];
        IsEditingExisting = false;
        ClearForm();
        UpdateParentVersionSuggestions(string.Empty);
        UpdateCompanySuggestions(string.Empty);
        IsEditing = true;
    }

    private async void OpenEdit(SoftwareVersionDto? item)
    {
        if(item == null) return;
        _editingId      = item.Id;
        EditPanelTitle  = _localizer["EditSoftwareVersionDialog_Title"];
        IsEditingExisting = true;
        Codename        = item.Codename ?? string.Empty;
        VersionString   = item.VersionString ?? string.Empty;
        PublicVersion   = item.PublicVersion ?? string.Empty;

        if(item.ParentVersionId != null && _allVersions != null)
        {
            SoftwareVersionDto? parent = _allVersions.FirstOrDefault(v => v.Id == item.ParentVersionId);
            if(parent != null)
            {
                ParentVersionSearchText = $"{parent.VersionString} ({parent.PublicVersion})";
                UpdateParentVersionSuggestions(ParentVersionSearchText);
                SelectedParentVersion = ParentVersionSuggestions.FirstOrDefault(v => v.Id == parent.Id);
            }
        }
        else { ParentVersionSearchText = string.Empty; SelectedParentVersion = null; UpdateParentVersionSuggestions(string.Empty); }

        UpdateCompanySuggestions(string.Empty);
        HasError = false; ErrorMessage = string.Empty;
        IsEditing = true;

        if(item.Id.HasValue)
            await LoadVersionCompaniesAsync(item.Id.Value);
    }

    private async Task DeleteAsync(SoftwareVersionDto? item)
    {
        if(item?.Id == null) return;

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer, item.VersionString))
            return;
        try
        {
            await _service.DeleteAsync(item.Id.Value);
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting version {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteSoftwareVersion"];
            HasError = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(VersionString))
            {
                ErrorMessage = _localizer["VersionStringIsRequired"]; HasError = true; return;
            }

            var dto = new SoftwareVersionDto
            {
                SoftwareId      = _parentSoftwareId,
                Codename        = string.IsNullOrWhiteSpace(Codename) ? null : Codename,
                VersionString   = VersionString,
                PublicVersion   = string.IsNullOrWhiteSpace(PublicVersion) ? null : PublicVersion,
                ParentVersionId = SelectedParentVersion?.Id
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
            _logger.LogError(ex, "Error saving version");
            ErrorMessage = _localizer["FailedToSaveSoftwareVersion"];
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
        FilteredVersions.Clear();
        IEnumerable<SoftwareVersionDto> source = (IEnumerable<SoftwareVersionDto>?)_allVersions ?? Versions;
        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(v =>
                (v.VersionString != null && v.VersionString.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (v.PublicVersion != null && v.PublicVersion.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                (v.Codename != null && v.Codename.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));
        foreach(SoftwareVersionDto item in source) FilteredVersions.Add(item);
    }

    public void UpdateParentVersionSuggestions(string query)
    {
        ParentVersionSuggestions.Clear();
        if(_allVersions == null) return;
        IEnumerable<SoftwareVersionDto> source = _allVersions;
        if(_editingId.HasValue) source = source.Where(v => v.Id != _editingId);
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(v =>
                (v.VersionString != null && v.VersionString.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (v.PublicVersion != null && v.PublicVersion.Contains(query, StringComparison.OrdinalIgnoreCase)));
        foreach(SoftwareVersionDto match in source) ParentVersionSuggestions.Add(match);
    }

    public void UpdateCompanySuggestions(string query)
    {
        CompanySuggestions.Clear();
        if(_allCompanies == null) return;
        IEnumerable<CompanyDto> source = _allCompanies;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(c => c.Name != null && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(CompanyDto match in source) CompanySuggestions.Add(match);
    }

    private async Task LoadVersionCompaniesAsync(int versionId)
    {
        VersionCompanies.Clear();
        VersionCompanyDisplays.Clear();

        try
        {
            List<CompanyBySoftwareVersionDto> items = await _service.GetCompaniesAsync(versionId);

            foreach(CompanyBySoftwareVersionDto item in items)
            {
                VersionCompanies.Add(item);
                string name    = item.Company ?? string.Empty;
                string? role   = item.Role;
                string display = !string.IsNullOrEmpty(role) ? $"{name} ({role})" : name;
                VersionCompanyDisplays.Add(display);
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading companies for version {Id}", versionId); }
    }

    private async Task AddCompanyAsync()
    {
        if(_editingId == null || SelectedCompany?.Id == null || SelectedRole?.Id == null) return;

        try
        {
            var dto = new CompanyBySoftwareVersionDto
            {
                SoftwareVersionId = _editingId,
                CompanyId         = SelectedCompany.Id,
                RoleId            = SelectedRole.Id
            };

            await _service.AddCompanyAsync(dto);
            SelectedCompany   = null;
            CompanySearchText = string.Empty;
            SelectedRole      = null;
            await LoadVersionCompaniesAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding company to version"); }
    }

    private async Task RemoveCompanyByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;

        int idx = VersionCompanyDisplays.IndexOf(display);

        if(idx < 0 || idx >= VersionCompanies.Count || VersionCompanies[idx].Id == null) return;

        try
        {
            await _service.RemoveCompanyAsync(VersionCompanies[idx].Id!.Value);
            await LoadVersionCompaniesAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error removing company from version"); }
    }

    private void ClearForm()
    {
        Codename = string.Empty;
        VersionString = string.Empty;
        PublicVersion = string.Empty;
        SelectedParentVersion = null;
        ParentVersionSearchText = string.Empty;
        VersionCompanies.Clear();
        VersionCompanyDisplays.Clear();
        SelectedCompany   = null;
        CompanySearchText = string.Empty;
        SelectedRole      = null;
        HasError = false; ErrorMessage = string.Empty;
    }
}
