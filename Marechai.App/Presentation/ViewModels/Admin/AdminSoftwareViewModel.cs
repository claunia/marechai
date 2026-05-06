#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.Data;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareService                    _service;
    private readonly Client                              _apiClient;
    private readonly SoftwareFamiliesService            _familiesService;
    private readonly IJwtService                        _jwtService;
    private readonly IStringLocalizer                   _localizer;
    private readonly ILogger<AdminSoftwareViewModel>    _logger;
    private readonly ITokenService                      _tokenService;
    private readonly IRegionManager                     _regionManager;

    [ObservableProperty] private ObservableCollection<SoftwareDto> _softwareItems = [];
    [ObservableProperty] private ObservableCollection<SoftwareDto> _filteredSoftware = [];
    [ObservableProperty] private string                            _filterText = string.Empty;
    [ObservableProperty] private SoftwareDto?                      _selectedSoftware;
    [ObservableProperty] private bool                               _isLoading;
    [ObservableProperty] private bool                               _isDataLoaded;
    [ObservableProperty] private bool                               _hasError;
    [ObservableProperty] private string                            _errorMessage = string.Empty;
    [ObservableProperty] private bool                               _isAdmin;
    [ObservableProperty] private bool                               _isEditing;
    [ObservableProperty] private bool                               _isEditingExisting;
    [ObservableProperty] private string                            _editPanelTitle = string.Empty;

    // Form
    [ObservableProperty] private string           _softwareName = string.Empty;
    [ObservableProperty] private int               _kind;
    [ObservableProperty] private int?              _baseSoftwareId;
    [ObservableProperty] private SoftwareFamilyDto? _selectedFamily;
    [ObservableProperty] private string           _familySearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareFamilyDto> _familySuggestions = [];

    // Company roles junction
    [ObservableProperty] private ObservableCollection<SoftwareCompanyRoleDto> _companyRoles = [];
    [ObservableProperty] private ObservableCollection<string>                 _companyRoleDisplays = [];
    [ObservableProperty] private CompanyDto?                                  _selectedCompanyToAdd;
    [ObservableProperty] private string                                      _companySearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyDto>             _companySuggestions = [];
    [ObservableProperty] private SoftwareRoleDto?                            _selectedRoleToAdd;
    [ObservableProperty] private ObservableCollection<SoftwareRoleDto>        _roles = [];

    private int?                        _editingId;
    private List<SoftwareDto>?          _allSoftware;
    private List<SoftwareFamilyDto>?    _allFamilies;
    private List<CompanyDto>?           _allCompanies;

    // Description editing
    [ObservableProperty] private bool                                             _isEditingDescription;
    [ObservableProperty] private string                                          _descriptionMarkdown = string.Empty;
    [ObservableProperty] private int?                                            _descriptionSoftwareId;
    [ObservableProperty] private ObservableCollection<LanguageItem>              _availableLanguages = [];
    [ObservableProperty] private LanguageItem?                                   _selectedLanguage;
    [ObservableProperty] private ObservableCollection<SoftwareDescriptionDto>    _existingTranslations = [];

    public AdminSoftwareViewModel(SoftwareService                   service,
                                  Client                             apiClient,
                                  SoftwareFamiliesService           familiesService,
                                  IJwtService                       jwtService,
                                  ITokenService                     tokenService,
                                  ILogger<AdminSoftwareViewModel>   logger,
                                  IStringLocalizer                  localizer,
                                  IRegionManager                    regionManager)
    {
        _service         = service;
        _apiClient       = apiClient;
        _familiesService = familiesService;
        _jwtService      = jwtService;
        _tokenService    = tokenService;
        _logger          = logger;
        _localizer       = localizer;
        _regionManager   = regionManager;

        LoadCommand         = new AsyncRelayCommand(LoadAsync);
        OpenAddCommand      = new RelayCommand(OpenAdd);
        OpenEditCommand     = new RelayCommand<SoftwareDto>(OpenEdit);
        DeleteCommand       = new AsyncRelayCommand<SoftwareDto>(DeleteAsync);
        SaveCommand         = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand   = new RelayCommand(CancelEdit);
        AddCompanyRoleCommand       = new AsyncRelayCommand(AddCompanyRoleAsync);
        RemoveCompanyRoleByDisplayCommand = new AsyncRelayCommand<string>(RemoveCompanyRoleByDisplayAsync);
        OpenVersionsCommand = new RelayCommand<SoftwareDto>(OpenVersions);
        OpenDescriptionCommand     = new AsyncRelayCommand<SoftwareDto>(OpenDescriptionAsync);
        SaveDescriptionCommand     = new AsyncRelayCommand(SaveDescriptionAsync);
        CancelDescriptionCommand   = new RelayCommand(CancelDescription);
        DeleteTranslationCommand   = new AsyncRelayCommand<SoftwareDescriptionDto>(DeleteTranslationAsync);
        EditTranslationCommand     = new RelayCommand<SoftwareDescriptionDto>(EditTranslation);

        CheckAdminRole();
        InitializeLanguages();
    }

    public IAsyncRelayCommand                LoadCommand       { get; }
    public IRelayCommand                     OpenAddCommand    { get; }
    public IRelayCommand<SoftwareDto>        OpenEditCommand   { get; }
    public IAsyncRelayCommand<SoftwareDto>   DeleteCommand     { get; }
    public IAsyncRelayCommand                SaveCommand       { get; }
    public IRelayCommand                     CancelEditCommand { get; }
    public IAsyncRelayCommand                AddCompanyRoleCommand { get; }
    public IAsyncRelayCommand<string>        RemoveCompanyRoleByDisplayCommand { get; }
    public IRelayCommand<SoftwareDto>        OpenVersionsCommand { get; }
    public IAsyncRelayCommand<SoftwareDto>              OpenDescriptionCommand   { get; }
    public IAsyncRelayCommand                           SaveDescriptionCommand   { get; }
    public IRelayCommand                                CancelDescriptionCommand { get; }
    public IAsyncRelayCommand<SoftwareDescriptionDto>   DeleteTranslationCommand { get; }
    public IRelayCommand<SoftwareDescriptionDto>        EditTranslationCommand   { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();
        if(IsAdmin)
        {
            _ = LoadCommand.ExecuteAsync(null);
            _ = LoadPickerDataAsync();
        }
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
            SoftwareItems.Clear();
            List<SoftwareDto> response = await _service.GetAllAsync();
            _allSoftware = response;
            foreach(SoftwareDto item in response) SoftwareItems.Add(item);
            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software");
            ErrorMessage = _localizer["FailedToLoadSoftware"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    public async Task LoadPickerDataAsync()
    {
        try
        {
            _allFamilies = await _familiesService.GetAllAsync();
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading families for picker"); }

        try
        {
            List<SoftwareRoleDto> rolesResponse = await _service.GetRolesAsync();
            Roles.Clear();
            foreach(SoftwareRoleDto r in rolesResponse) Roles.Add(r);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading software roles"); }
    }

    private void OpenAdd()
    {
        _editingId = null;
        EditPanelTitle = _localizer["AddSoftwareDialog_Title"];
        IsEditingExisting = false;
        ClearForm();
        IsEditing = true;
    }

    private async void OpenEdit(SoftwareDto? item)
    {
        if(item?.Id == null) return;

        _editingId        = item.Id;
        EditPanelTitle    = _localizer["EditSoftwareDialog_Title"];
        IsEditingExisting = true;
        SoftwareName      = item.Name ?? string.Empty;
        Kind              = item.Kind ?? 0;
        BaseSoftwareId    = item.BaseSoftwareId;

        if(item.FamilyId.HasValue && _allFamilies != null)
        {
            SoftwareFamilyDto? family = _allFamilies.FirstOrDefault(f => f.Id == item.FamilyId.Value);
            if(family != null)
            {
                FamilySearchText = family.Name ?? string.Empty;
                UpdateFamilySuggestions(FamilySearchText);
                SelectedFamily = FamilySuggestions.FirstOrDefault(f => f.Id == family.Id);
            }
        }
        else
        {
            FamilySearchText = string.Empty;
            SelectedFamily   = null;
        }

        HasError = false; ErrorMessage = string.Empty;
        IsEditing = true;

        await LoadCompanyRolesAsync(item.Id.Value);
    }

    private async Task DeleteAsync(SoftwareDto? item)
    {
        if(item?.Id == null) return;
        try
        {
            await _service.DeleteAsync(item.Id.Value);
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteSoftware"];
            HasError = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(SoftwareName))
            {
                ErrorMessage = _localizer["NameIsRequired"]; HasError = true; return;
            }

            var dto = new SoftwareDto
            {
                Name              = SoftwareName,
                FamilyId          = SelectedFamily?.Id,
                BaseSoftwareId    = BaseSoftwareId,
                Kind              = Kind
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
            _logger.LogError(ex, "Error saving software");
            ErrorMessage = _localizer["FailedToSaveSoftware"];
            HasError = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing = false; _editingId = null;
        ClearForm();
        HasError = false; ErrorMessage = string.Empty;
    }

    // --- Drill-down navigation ---
    private void OpenVersions(SoftwareDto? item)
    {
        if(item?.Id == null) return;
        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, item.Id.Value },
            { NavParamKeys.SoftwareName, item.Name ?? string.Empty }
        };
        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoftwareVersionsPage), parameters);
    }

    public void ApplyFilter()
    {
        FilteredSoftware.Clear();
        IEnumerable<SoftwareDto> source = (IEnumerable<SoftwareDto>?)_allSoftware ?? SoftwareItems;
        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(s => (s.Name != null && s.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (s.Family != null && s.Family.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));
        foreach(SoftwareDto item in source) FilteredSoftware.Add(item);
    }

    public void UpdateFamilySuggestions(string query)
    {
        FamilySuggestions.Clear();
        if(_allFamilies == null) return;
        IEnumerable<SoftwareFamilyDto> source = _allFamilies;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(f => f.Name != null && f.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(SoftwareFamilyDto match in source) FamilySuggestions.Add(match);
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

    // --- Company roles ---
    private async Task LoadCompanyRolesAsync(int softwareId)
    {
        CompanyRoles.Clear(); CompanyRoleDisplays.Clear();
        try
        {
            List<SoftwareCompanyRoleDto> items = await _service.GetCompanyRolesAsync(softwareId);
            foreach(SoftwareCompanyRoleDto item in items)
            {
                CompanyRoles.Add(item);
                CompanyRoleDisplays.Add($"{item.Company} ({item.Role})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading company roles for software {Id}", softwareId); }
    }

    private async Task AddCompanyRoleAsync()
    {
        if(_editingId == null || SelectedCompanyToAdd == null || SelectedRoleToAdd == null) return;
        try
        {
            var dto = new SoftwareCompanyRoleDto
            {
                SoftwareId = _editingId.Value,
                CompanyId  = SelectedCompanyToAdd.Id.GetValueOrDefault(),
                RoleId     = SelectedRoleToAdd.Id ?? string.Empty
            };
            await _service.AddCompanyRoleAsync(dto);
            await LoadCompanyRolesAsync(_editingId.Value);
            SelectedCompanyToAdd = null;
            CompanySearchText = string.Empty;
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding company role to software"); }
    }

    private async Task RemoveCompanyRoleByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;
        int index = CompanyRoleDisplays.IndexOf(display);
        if(index >= 0 && index < CompanyRoles.Count)
        {
            SoftwareCompanyRoleDto item = CompanyRoles[index];
            await _service.RemoveCompanyRoleAsync(
                item.SoftwareId?.ToString() ?? string.Empty,
                item.CompanyId?.ToString() ?? string.Empty,
                item.RoleId ?? string.Empty);
            await LoadCompanyRolesAsync(_editingId.Value);
        }
    }

    private void ClearForm()
    {
        SoftwareName      = string.Empty;
        Kind              = 0;
        BaseSoftwareId    = null;
        SelectedFamily    = null;
        FamilySearchText  = string.Empty;
        CompanyRoles.Clear();
        CompanyRoleDisplays.Clear();
        SelectedCompanyToAdd = null;
        CompanySearchText = string.Empty;
        SelectedRoleToAdd = null;
        HasError = false; ErrorMessage = string.Empty;
    }

    // --- Description management ---

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

    private async Task OpenDescriptionAsync(SoftwareDto? software)
    {
        if(software?.Id == null) return;

        try
        {
            DescriptionSoftwareId = software.Id;
            DescriptionMarkdown   = string.Empty;
            IsEditing             = false;
            ExistingTranslations.Clear();

            List<SoftwareDescriptionDto>? translations =
                await _apiClient.Software[software.Id.Value].Descriptions.GetAsync();

            if(translations != null)
                foreach(SoftwareDescriptionDto t in translations)
                    ExistingTranslations.Add(t);

            SelectedLanguage = AvailableLanguages.FirstOrDefault(l =>
                                   ExistingTranslations.All(t => t.LanguageCode != l.Code)) ??
                               AvailableLanguages[0];

            SoftwareDescriptionDto? existing =
                ExistingTranslations.FirstOrDefault(t => t.LanguageCode == SelectedLanguage.Code);

            if(existing != null)
                DescriptionMarkdown = existing.Markdown ?? string.Empty;

            IsEditingDescription = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading description for software {Id}", software.Id);
            DescriptionMarkdown  = string.Empty;
            IsEditingDescription = true;
        }
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        if(value == null || ExistingTranslations.Count == 0)
        {
            DescriptionMarkdown = string.Empty;

            return;
        }

        SoftwareDescriptionDto? existing = ExistingTranslations.FirstOrDefault(t => t.LanguageCode == value.Code);
        DescriptionMarkdown = existing?.Markdown ?? string.Empty;
    }

    private async Task SaveDescriptionAsync()
    {
        if(DescriptionSoftwareId == null || SelectedLanguage == null) return;

        try
        {
            var dto = new SoftwareDescriptionDto
            {
                SoftwareId   = DescriptionSoftwareId.Value,
                Markdown     = DescriptionMarkdown,
                LanguageCode = SelectedLanguage.Code
            };

            await _apiClient.Software[DescriptionSoftwareId.Value].Description.PostAsync(dto);

            ExistingTranslations.Clear();

            List<SoftwareDescriptionDto>? translations =
                await _apiClient.Software[DescriptionSoftwareId.Value].Descriptions.GetAsync();

            if(translations != null)
                foreach(SoftwareDescriptionDto t in translations)
                    ExistingTranslations.Add(t);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving description");
            ErrorMessage = _localizer["FailedToSaveDescription"];
            HasError     = true;
        }
    }

    private void EditTranslation(SoftwareDescriptionDto? translation)
    {
        if(translation?.LanguageCode == null) return;

        SelectedLanguage    = AvailableLanguages.FirstOrDefault(l => l.Code == translation.LanguageCode);
        DescriptionMarkdown = translation.Markdown ?? string.Empty;
    }

    private async Task DeleteTranslationAsync(SoftwareDescriptionDto? translation)
    {
        if(DescriptionSoftwareId == null || translation?.LanguageCode == null) return;

        try
        {
            await _apiClient.Software[DescriptionSoftwareId.Value].Description[translation.LanguageCode].DeleteAsync();

            ExistingTranslations.Remove(translation);
            DescriptionMarkdown = string.Empty;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting translation");
            ErrorMessage = _localizer["FailedToDeleteTranslation"];
            HasError     = true;
        }
    }

    private void CancelDescription()
    {
        IsEditingDescription  = false;
        DescriptionSoftwareId = null;
        DescriptionMarkdown   = string.Empty;
        ExistingTranslations.Clear();
    }
}
