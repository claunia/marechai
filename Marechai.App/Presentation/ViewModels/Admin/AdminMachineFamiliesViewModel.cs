#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Presentation.Dialogs;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminMachineFamiliesViewModel : ObservableObject, IRegionAware
{
    private readonly Client                                _apiClient;
    private readonly IJwtService                              _jwtService;
    private readonly IStringLocalizer                         _localizer;
    private readonly ILogger<AdminMachineFamiliesViewModel>   _logger;
    private readonly ITokenService                            _tokenService;

    [ObservableProperty] private ObservableCollection<MachineFamilyDto> _families = [];
    [ObservableProperty] private ObservableCollection<MachineFamilyDto> _filteredFamilies = [];
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private MachineFamilyDto? _selectedFamily;
    private List<MachineFamilyDto>? _allFamilies;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    private int? _editingId;

    [ObservableProperty] private string _familyName = string.Empty;

    // --- Company picker ---
    [ObservableProperty] private CompanyDto? _selectedCompany;
    [ObservableProperty] private string _companySearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyDto> _companySuggestions = [];
    private List<CompanyDto>? _allCompanies;

    public AdminMachineFamiliesViewModel(Client                                apiClient,
                                         IJwtService                              jwtService,
                                         ITokenService                            tokenService,
                                         ILogger<AdminMachineFamiliesViewModel>   logger,
                                         IStringLocalizer                         localizer)
    {
        _apiClient    = apiClient;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadItemsCommand  = new AsyncRelayCommand(LoadItemsAsync);
        OpenAddCommand    = new RelayCommand(OpenAdd);
        OpenEditCommand   = new RelayCommand<MachineFamilyDto>(OpenEdit);
        DeleteCommand     = new AsyncRelayCommand<MachineFamilyDto>(DeleteAsync);
        SaveCommand       = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);

        CheckAdminRole();
    }

    public IAsyncRelayCommand LoadItemsCommand { get; }
    public IRelayCommand OpenAddCommand { get; }
    public IRelayCommand<MachineFamilyDto> OpenEditCommand { get; }
    public IAsyncRelayCommand<MachineFamilyDto> DeleteCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelEditCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();
        if(IsAdmin)
        {
            _ = LoadItemsCommand.ExecuteAsync(null);
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
            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch { IsAdmin = false; }
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true; HasError = false; ErrorMessage = string.Empty;
            Families.Clear();
            List<MachineFamilyDto>? response = await _apiClient.MachineFamilies.GetAsync();
            _allFamilies = response;
            if(response != null) foreach(MachineFamilyDto f in response) Families.Add(f);
            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading machine families");
            ErrorMessage = _localizer["FailedToLoadMachineFamilies"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    private void OpenAdd()
    {
        _editingId = null;
        EditPanelTitle = _localizer["AddMachineFamilyDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    private async void OpenEdit(MachineFamilyDto? item)
    {
        if(item?.Id == null) return;
        try
        {
            // Detail endpoint returns CompanyId (list returns Company name)
            MachineFamilyDto? full = await _apiClient.MachineFamilies[item.Id.Value].GetAsync();
            if(full == null) return;
            _editingId = item.Id;
            EditPanelTitle = _localizer["EditMachineFamilyDialog_Title"];
            FamilyName = full.Name ?? string.Empty;

            if(full.CompanyId.HasValue && _allCompanies != null)
            {
                CompanyDto? c = _allCompanies.FirstOrDefault(x => x.Id == full.CompanyId.Value);
                if(c != null)
                {
                    CompanySearchText = c.Name ?? string.Empty;
                    UpdateCompanySuggestions(CompanySearchText);
                    SelectedCompany = CompanySuggestions.FirstOrDefault(x => x.Id == c.Id);
                }
                else { CompanySearchText = item.Company ?? string.Empty; SelectedCompany = null; }
            }
            else { CompanySearchText = string.Empty; SelectedCompany = null; }

            IsEditing = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading machine family {Id}", item.Id);
            ErrorMessage = _localizer["FailedToLoadMachineFamilies"];
            HasError = true;
        }
    }

    private async Task DeleteAsync(MachineFamilyDto? item)
    {
        if(item?.Id == null) return;

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer, item.Name))
            return;
        try
        {
            await _apiClient.MachineFamilies[item.Id.Value].DeleteAsync();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine family {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteMachineFamily"];
            HasError = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(FamilyName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError = true;
                return;
            }
            if(SelectedCompany?.Id == null)
            {
                ErrorMessage = _localizer["MachineCompanyRequired"];
                HasError = true;
                return;
            }

            var dto = new MachineFamilyDto
            {
                Name      = FamilyName,
                CompanyId = SelectedCompany.Id.Value
            };

            if(_editingId == null)
                await _apiClient.MachineFamilies.PostAsync(dto);
            else
            {
                dto.Id = _editingId;
                await _apiClient.MachineFamilies[_editingId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving machine family");
            ErrorMessage = _localizer["FailedToSaveMachineFamily"];
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
        IEnumerable<MachineFamilyDto> source = (IEnumerable<MachineFamilyDto>?)_allFamilies ?? Families;
        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(f => (f.Name != null && f.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (f.Company != null && f.Company.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));
        foreach(MachineFamilyDto f in source) FilteredFamilies.Add(f);
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

    public async Task LoadPickerDataAsync()
    {
        try { _allCompanies = await _apiClient.Companies.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading companies"); }
    }

    private void ClearForm()
    {
        FamilyName = string.Empty;
        SelectedCompany = null;
        CompanySearchText = string.Empty;
        HasError = false; ErrorMessage = string.Empty;
    }
}
