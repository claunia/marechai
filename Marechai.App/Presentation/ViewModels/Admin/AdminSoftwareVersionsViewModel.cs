#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareVersionsViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareVersionsService                     _service;
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
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    [ObservableProperty] private string _pageTitle = string.Empty;

    // Form
    [ObservableProperty] private string _codename = string.Empty;
    [ObservableProperty] private string _versionString = string.Empty;
    [ObservableProperty] private string _publicVersion = string.Empty;
    [ObservableProperty] private SoftwareVersionDto? _selectedParentVersion;
    [ObservableProperty] private string _parentVersionSearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<SoftwareVersionDto> _parentVersionSuggestions = [];

    private int? _editingId;
    private int  _parentSoftwareId;
    private List<SoftwareVersionDto>? _allVersions;

    public AdminSoftwareVersionsViewModel(SoftwareVersionsService                  service,
                                          IJwtService                              jwtService,
                                          ITokenService                            tokenService,
                                          ILogger<AdminSoftwareVersionsViewModel>  logger,
                                          IStringLocalizer                         localizer,
                                          IRegionManager                           regionManager)
    {
        _service       = service;
        _jwtService    = jwtService;
        _tokenService  = tokenService;
        _logger        = logger;
        _localizer     = localizer;
        _regionManager = regionManager;

        LoadCommand       = new AsyncRelayCommand(LoadAsync);
        OpenAddCommand    = new RelayCommand(OpenAdd);
        OpenEditCommand   = new RelayCommand<SoftwareVersionDto>(OpenEdit);
        DeleteCommand     = new AsyncRelayCommand<SoftwareVersionDto>(DeleteAsync);
        SaveCommand       = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);
        GoBackCommand     = new RelayCommand(GoBack);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                        LoadCommand       { get; }
    public IRelayCommand                             OpenAddCommand    { get; }
    public IRelayCommand<SoftwareVersionDto>         OpenEditCommand   { get; }
    public IAsyncRelayCommand<SoftwareVersionDto>    DeleteCommand     { get; }
    public IAsyncRelayCommand                        SaveCommand       { get; }
    public IRelayCommand                             CancelEditCommand { get; }
    public IRelayCommand                             GoBackCommand     { get; }

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
        ClearForm();
        UpdateParentVersionSuggestions(string.Empty);
        IsEditing = true;
    }

    private void OpenEdit(SoftwareVersionDto? item)
    {
        if(item == null) return;
        _editingId      = item.Id;
        EditPanelTitle  = _localizer["EditSoftwareVersionDialog_Title"];
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

        HasError = false; ErrorMessage = string.Empty;
        IsEditing = true;
    }

    private async Task DeleteAsync(SoftwareVersionDto? item)
    {
        if(item?.Id == null) return;
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

    private void ClearForm()
    {
        Codename = string.Empty;
        VersionString = string.Empty;
        PublicVersion = string.Empty;
        SelectedParentVersion = null;
        ParentVersionSearchText = string.Empty;
        HasError = false; ErrorMessage = string.Empty;
    }
}
