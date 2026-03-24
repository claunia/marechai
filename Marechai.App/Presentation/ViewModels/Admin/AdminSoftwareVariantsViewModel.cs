#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareVariantsViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareVariantsService                     _service;
    private readonly IJwtService                                 _jwtService;
    private readonly IStringLocalizer                            _localizer;
    private readonly ILogger<AdminSoftwareVariantsViewModel>     _logger;
    private readonly ITokenService                               _tokenService;
    private readonly IRegionManager                              _regionManager;

    [ObservableProperty] private ObservableCollection<SoftwareVariantDto> _variants = [];
    [ObservableProperty] private ObservableCollection<SoftwareVariantDto> _filteredVariants = [];
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private SoftwareVariantDto? _selectedVariant;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _variantName = string.Empty;

    private int? _editingId;
    private int  _parentSoftwareId;
    private List<SoftwareVariantDto>? _allVariants;

    public AdminSoftwareVariantsViewModel(SoftwareVariantsService                  service,
                                          IJwtService                              jwtService,
                                          ITokenService                            tokenService,
                                          ILogger<AdminSoftwareVariantsViewModel>  logger,
                                          IStringLocalizer                         localizer,
                                          IRegionManager                           regionManager)
    {
        _service       = service;
        _jwtService    = jwtService;
        _tokenService  = tokenService;
        _logger        = logger;
        _localizer     = localizer;
        _regionManager = regionManager;

        LoadCommand           = new AsyncRelayCommand(LoadAsync);
        OpenAddCommand        = new RelayCommand(OpenAdd);
        OpenEditCommand       = new RelayCommand<SoftwareVariantDto>(OpenEdit);
        DeleteCommand         = new AsyncRelayCommand<SoftwareVariantDto>(DeleteAsync);
        SaveCommand           = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand     = new RelayCommand(CancelEdit);
        OpenSubvariantsCommand = new RelayCommand<SoftwareVariantDto>(OpenSubvariants);
        GoBackCommand          = new RelayCommand(GoBack);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                        LoadCommand            { get; }
    public IRelayCommand                             OpenAddCommand         { get; }
    public IRelayCommand<SoftwareVariantDto>         OpenEditCommand        { get; }
    public IAsyncRelayCommand<SoftwareVariantDto>    DeleteCommand          { get; }
    public IAsyncRelayCommand                        SaveCommand            { get; }
    public IRelayCommand                             CancelEditCommand      { get; }
    public IRelayCommand<SoftwareVariantDto>         OpenSubvariantsCommand { get; }
    public IRelayCommand                             GoBackCommand          { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    void GoBack() => _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoftwarePage));

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();
        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareId, out int softwareId))
            _parentSoftwareId = softwareId;
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.SoftwareName, out string? name))
            PageTitle = string.Format(_localizer["SoftwareVariantsForTitle"], name);
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
            Variants.Clear();
            List<SoftwareVariantDto> response = await _service.GetBySoftwareAsync(_parentSoftwareId);
            _allVariants = response;
            foreach(SoftwareVariantDto item in response) Variants.Add(item);
            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading variants");
            ErrorMessage = _localizer["FailedToLoadSoftwareVariants"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    private void OpenAdd()
    {
        _editingId = null;
        EditPanelTitle = _localizer["AddSoftwareVariantDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    private void OpenEdit(SoftwareVariantDto? item)
    {
        if(item == null) return;
        _editingId     = item.Id;
        EditPanelTitle = _localizer["EditSoftwareVariantDialog_Title"];
        VariantName    = item.Name ?? string.Empty;
        HasError = false; ErrorMessage = string.Empty;
        IsEditing = true;
    }

    private async Task DeleteAsync(SoftwareVariantDto? item)
    {
        if(item?.Id == null) return;
        try { await _service.DeleteAsync(item.Id.Value); await LoadAsync(); }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting variant {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteSoftwareVariant"];
            HasError = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(VariantName))
            {
                ErrorMessage = _localizer["NameIsRequired"]; HasError = true; return;
            }

            var dto = new SoftwareVariantDto
            {
                SoftwareId = _parentSoftwareId,
                Name       = VariantName
            };

            if(_editingId == null)
                await _service.CreateAsync(dto);
            else
            {
                dto.Id = _editingId;
                await _service.UpdateAsync(dto);
            }

            IsEditing = false; ClearForm();
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving variant");
            ErrorMessage = _localizer["FailedToSaveSoftwareVariant"];
            HasError = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing = false; _editingId = null;
        ClearForm();
        HasError = false; ErrorMessage = string.Empty;
    }

    private void OpenSubvariants(SoftwareVariantDto? item)
    {
        if(item?.Id == null) return;
        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareVariantId, item.Id.Value },
            { NavParamKeys.SoftwareVariantName, item.Name ?? string.Empty }
        };
        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoftwareSubvariantsPage), parameters);
    }

    public void ApplyFilter()
    {
        FilteredVariants.Clear();
        IEnumerable<SoftwareVariantDto> source = (IEnumerable<SoftwareVariantDto>?)_allVariants ?? Variants;
        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(v => v.Name != null && v.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
        foreach(SoftwareVariantDto item in source) FilteredVariants.Add(item);
    }

    private void ClearForm()
    {
        VariantName = string.Empty;
        HasError = false; ErrorMessage = string.Empty;
    }
}
