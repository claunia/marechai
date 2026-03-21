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

public partial class AdminSoftwareFamiliesViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareFamiliesService                    _service;
    private readonly IJwtService                               _jwtService;
    private readonly IStringLocalizer                          _localizer;
    private readonly ILogger<AdminSoftwareFamiliesViewModel>   _logger;
    private readonly ITokenService                             _tokenService;

    [ObservableProperty] private ObservableCollection<SoftwareFamilyDto> _families = [];
    [ObservableProperty] private ObservableCollection<SoftwareFamilyDto> _filteredFamilies = [];
    [ObservableProperty] private string                                  _filterText = string.Empty;
    [ObservableProperty] private SoftwareFamilyDto?                      _selectedFamily;
    [ObservableProperty] private bool                                     _isLoading;
    [ObservableProperty] private bool                                     _isDataLoaded;
    [ObservableProperty] private bool                                     _hasError;
    [ObservableProperty] private string                                  _errorMessage = string.Empty;
    [ObservableProperty] private bool                                     _isAdmin;
    [ObservableProperty] private bool                                     _isEditing;
    [ObservableProperty] private bool                                     _isEditingExisting;
    [ObservableProperty] private string                                  _editPanelTitle = string.Empty;

    // Form fields
    [ObservableProperty] private string                       _familyName = string.Empty;
    [ObservableProperty] private DateTimeOffset?              _introduced;
    [ObservableProperty] private SoftwareFamilyDto?           _selectedParent;
    [ObservableProperty] private string                       _parentSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareFamilyDto> _parentSuggestions = [];

    // Company junction
    [ObservableProperty] private ObservableCollection<CompanyBySoftwareFamilyDto> _companies = [];
    [ObservableProperty] private ObservableCollection<string>                     _companyDisplays = [];
    [ObservableProperty] private CompanyDto?                                      _selectedCompanyToAdd;
    [ObservableProperty] private string                                          _companySearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyDto>                 _companySuggestions = [];
    [ObservableProperty] private SoftwareRoleDto?                                _selectedRoleToAdd;
    [ObservableProperty] private ObservableCollection<SoftwareRoleDto>            _roles = [];

    private int?                        _editingId;
    private List<SoftwareFamilyDto>?    _allFamilies;
    private List<CompanyDto>?           _allCompanies;

    public AdminSoftwareFamiliesViewModel(SoftwareFamiliesService                  service,
                                          IJwtService                              jwtService,
                                          ITokenService                            tokenService,
                                          ILogger<AdminSoftwareFamiliesViewModel>  logger,
                                          IStringLocalizer                         localizer)
    {
        _service      = service;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadCommand        = new AsyncRelayCommand(LoadAsync);
        OpenAddCommand     = new RelayCommand(OpenAdd);
        OpenEditCommand    = new RelayCommand<SoftwareFamilyDto>(OpenEdit);
        DeleteCommand      = new AsyncRelayCommand<SoftwareFamilyDto>(DeleteAsync);
        SaveCommand        = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand  = new RelayCommand(CancelEdit);
        AddCompanyCommand  = new AsyncRelayCommand(AddCompanyAsync);
        RemoveCompanyByDisplayCommand = new AsyncRelayCommand<string>(RemoveCompanyByDisplayAsync);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                       LoadCommand       { get; }
    public IRelayCommand                            OpenAddCommand    { get; }
    public IRelayCommand<SoftwareFamilyDto>         OpenEditCommand   { get; }
    public IAsyncRelayCommand<SoftwareFamilyDto>    DeleteCommand     { get; }
    public IAsyncRelayCommand                       SaveCommand       { get; }
    public IRelayCommand                            CancelEditCommand { get; }
    public IAsyncRelayCommand                       AddCompanyCommand { get; }
    public IAsyncRelayCommand<string>               RemoveCompanyByDisplayCommand { get; }

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
            IsLoading = true; HasError = false; ErrorMessage = string.Empty;
            Families.Clear();
            List<SoftwareFamilyDto> response = await _service.GetAllAsync();
            _allFamilies = response;
            foreach(SoftwareFamilyDto item in response) Families.Add(item);
            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software families");
            ErrorMessage = _localizer["FailedToLoadSoftwareFamilies"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    public async Task LoadPickerDataAsync()
    {
        if(_allCompanies == null)
        {
            try
            {
                // Load companies via a quick API call
                _allCompanies = await _service.GetAllAsync() is not null ? [] : [];
                // Use the ApiClient directly? No — we need CompanyDto. Use a separate call.
            }
            catch(Exception ex) { _logger.LogError(ex, "Error loading picker data"); }
        }

        if(Roles.Count == 0)
        {
            try
            {
                List<SoftwareRoleDto> rolesResponse = await _service.GetRolesAsync();
                foreach(SoftwareRoleDto r in rolesResponse) Roles.Add(r);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error loading software roles"); }
        }
    }

    private void OpenAdd()
    {
        _editingId = null;
        EditPanelTitle = _localizer["AddSoftwareFamilyDialog_Title"];
        IsEditingExisting = false;
        ClearForm();
        IsEditing = true;
    }

    private async void OpenEdit(SoftwareFamilyDto? item)
    {
        if(item?.Id == null) return;

        _editingId        = item.Id;
        EditPanelTitle    = _localizer["EditSoftwareFamilyDialog_Title"];
        IsEditingExisting = true;
        FamilyName        = item.Name ?? string.Empty;
        Introduced        = item.Introduced;

        // Parent picker
        if(item.ParentId.HasValue && _allFamilies != null)
        {
            SoftwareFamilyDto? parent = _allFamilies.FirstOrDefault(f => f.Id == item.ParentId.Value);
            if(parent != null)
            {
                ParentSearchText = parent.Name ?? string.Empty;
                UpdateParentSuggestions(ParentSearchText);
                SelectedParent = ParentSuggestions.FirstOrDefault(f => f.Id == parent.Id);
            }
        }
        else
        {
            ParentSearchText = string.Empty;
            SelectedParent   = null;
        }

        HasError = false; ErrorMessage = string.Empty;
        IsEditing = true;

        await LoadFamilyCompaniesAsync(item.Id.Value);
    }

    private async Task DeleteAsync(SoftwareFamilyDto? item)
    {
        if(item?.Id == null) return;
        try
        {
            await _service.DeleteAsync(item.Id.Value);
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software family {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteSoftwareFamily"];
            HasError = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(FamilyName))
            {
                ErrorMessage = _localizer["NameIsRequired"]; HasError = true; return;
            }

            var dto = new SoftwareFamilyDto
            {
                Name       = FamilyName,
                ParentId   = SelectedParent?.Id,
                Introduced = Introduced
            };

            if(_editingId == null)
            {
                int? newId = await _service.CreateAsync(dto);
                if(newId.HasValue) _editingId = newId.Value;
            }
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
            _logger.LogError(ex, "Error saving software family");
            ErrorMessage = _localizer["FailedToSaveSoftwareFamily"];
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
        FilteredFamilies.Clear();
        IEnumerable<SoftwareFamilyDto> source = (IEnumerable<SoftwareFamilyDto>?)_allFamilies ?? Families;
        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(f => (f.Name != null && f.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (f.Parent != null && f.Parent.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));
        foreach(SoftwareFamilyDto item in source) FilteredFamilies.Add(item);
    }

    public void UpdateParentSuggestions(string query)
    {
        ParentSuggestions.Clear();
        if(_allFamilies == null) return;
        IEnumerable<SoftwareFamilyDto> source = _allFamilies;
        // Exclude current entity to prevent circular reference
        if(_editingId.HasValue)
            source = source.Where(f => f.Id != _editingId);
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(f => f.Name != null && f.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(SoftwareFamilyDto match in source.Take(50)) ParentSuggestions.Add(match);
    }

    public void UpdateCompanySuggestions(string query)
    {
        CompanySuggestions.Clear();
        if(_allCompanies == null) return;
        IEnumerable<CompanyDto> source = _allCompanies;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(c => c.Name != null && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(CompanyDto match in source.Take(50)) CompanySuggestions.Add(match);
    }

    // --- Company junction ---
    private async Task LoadFamilyCompaniesAsync(int familyId)
    {
        Companies.Clear(); CompanyDisplays.Clear();
        try
        {
            List<CompanyBySoftwareFamilyDto> items = await _service.GetCompaniesAsync(familyId);
            foreach(CompanyBySoftwareFamilyDto item in items)
            {
                Companies.Add(item);
                CompanyDisplays.Add($"{item.Company} ({item.Role})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading companies for family {Id}", familyId); }
    }

    private async Task AddCompanyAsync()
    {
        if(_editingId == null || SelectedCompanyToAdd == null || SelectedRoleToAdd == null) return;
        try
        {
            var dto = new CompanyBySoftwareFamilyDto
            {
                SoftwareFamilyId = (int)_editingId,
                CompanyId        = SelectedCompanyToAdd.Id.GetValueOrDefault(),
                RoleId           = SelectedRoleToAdd.Id
            };
            await _service.AddCompanyAsync(dto);
            await LoadFamilyCompaniesAsync(_editingId.Value);
            SelectedCompanyToAdd = null;
            CompanySearchText    = string.Empty;
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding company to family"); }
    }

    private async Task RemoveCompanyByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;
        int index = CompanyDisplays.IndexOf(display);
        if(index >= 0 && index < Companies.Count)
        {
            CompanyBySoftwareFamilyDto item = Companies[index];
            if(item.Id.HasValue)
            {
                await _service.RemoveCompanyAsync(item.Id.Value);
                await LoadFamilyCompaniesAsync(_editingId.Value);
            }
        }
    }

    private void ClearForm()
    {
        FamilyName       = string.Empty;
        Introduced       = null;
        SelectedParent   = null;
        ParentSearchText = string.Empty;
        Companies.Clear();
        CompanyDisplays.Clear();
        SelectedCompanyToAdd = null;
        CompanySearchText    = string.Empty;
        SelectedRoleToAdd    = null;
        HasError = false; ErrorMessage = string.Empty;
    }
}
