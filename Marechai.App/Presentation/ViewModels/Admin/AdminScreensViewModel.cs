#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminScreensViewModel : ObservableObject, IRegionAware
{
    private readonly ApiClient                         _apiClient;
    private readonly IJwtService                       _jwtService;
    private readonly IStringLocalizer                  _localizer;
    private readonly ILogger<AdminScreensViewModel>    _logger;
    private readonly ITokenService                     _tokenService;

    [ObservableProperty] private ObservableCollection<ScreenDto> _screens = [];
    [ObservableProperty] private ObservableCollection<ScreenDto> _filteredScreens = [];
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private ScreenDto? _selectedScreen;
    private List<ScreenDto>? _allScreens;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    private int? _editingId;

    // --- Form fields ---
    [ObservableProperty] private double _diagonal;
    [ObservableProperty] private double? _width;
    [ObservableProperty] private double? _height;
    [ObservableProperty] private long? _effectiveColors;
    [ObservableProperty] private string _screenType = string.Empty;

    // --- Resolution picker ---
    [ObservableProperty] private ResolutionDto? _selectedResolution;
    [ObservableProperty] private ObservableCollection<ResolutionDto> _resolutions = [];
    [ObservableProperty] private string _resolutionFilterText = string.Empty;
    [ObservableProperty] private ObservableCollection<ResolutionDto> _filteredResolutions = [];
    private List<ResolutionDto>? _allResolutions;

    // --- Supported resolutions (junction) ---
    [ObservableProperty] private ObservableCollection<ResolutionByScreenDto> _screenResolutions = [];
    [ObservableProperty] private ObservableCollection<string> _screenResolutionDisplays = [];
    [ObservableProperty] private ObservableCollection<ResolutionDto> _availableSupportedResolutions = [];
    [ObservableProperty] private ResolutionDto? _selectedSupportedResolution;

    public AdminScreensViewModel(ApiClient                         apiClient,
                                 IJwtService                       jwtService,
                                 ITokenService                     tokenService,
                                 ILogger<AdminScreensViewModel>    logger,
                                 IStringLocalizer                  localizer)
    {
        _apiClient    = apiClient;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadItemsCommand  = new AsyncRelayCommand(LoadItemsAsync);
        OpenAddCommand    = new RelayCommand(OpenAdd);
        OpenEditCommand   = new RelayCommand<ScreenDto>(OpenEdit);
        DeleteCommand     = new AsyncRelayCommand<ScreenDto>(DeleteAsync);
        SaveCommand       = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);
        AddSupportedResolutionCommand    = new AsyncRelayCommand(AddSupportedResolutionAsync);
        RemoveSupportedResolutionCommand = new AsyncRelayCommand<string>(RemoveSupportedResolutionByDisplayAsync);

        CheckAdminRole();
    }

    public IAsyncRelayCommand             LoadItemsCommand  { get; }
    public IRelayCommand                  OpenAddCommand    { get; }
    public IRelayCommand<ScreenDto>        OpenEditCommand   { get; }
    public IAsyncRelayCommand<ScreenDto>   DeleteCommand     { get; }
    public IAsyncRelayCommand             SaveCommand       { get; }
    public IRelayCommand                  CancelEditCommand { get; }
    public IAsyncRelayCommand             AddSupportedResolutionCommand    { get; }
    public IAsyncRelayCommand<string>     RemoveSupportedResolutionCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin) _ = LoadItemsCommand.ExecuteAsync(null);
    }

    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(string.IsNullOrWhiteSpace(token)) { IsAdmin = false; return; }

            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("Uberadmin",      StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
        }
        catch { IsAdmin = false; }
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true; HasError = false; ErrorMessage = string.Empty;
            Screens.Clear();

            List<ScreenDto>? response = await _apiClient.Screens.GetAsync();
            _allScreens = response;

            if(response != null)
                foreach(ScreenDto item in response)
                    Screens.Add(item);

            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading screens");
            ErrorMessage = _localizer["FailedToLoadScreens"];
            HasError     = true;
        }
        finally { IsLoading = false; }
    }

    private void OpenAdd()
    {
        _editingId     = null;
        EditPanelTitle = _localizer["AddScreenDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    private async void OpenEdit(ScreenDto? item)
    {
        if(item == null) return;

        _editingId     = item.Id;
        EditPanelTitle = _localizer["EditScreenDialog_Title"];
        PopulateForm(item);

        if(item.Id.HasValue)
            await LoadScreenResolutionsAsync(item.Id.Value);

        IsEditing = true;
    }

    private async Task LoadScreenResolutionsAsync(int screenId)
    {
        ScreenResolutions.Clear();
        ScreenResolutionDisplays.Clear();

        try
        {
            List<ResolutionByScreenDto>? rels =
                await _apiClient.Screens[screenId].Resolutions.GetAsync();

            if(rels != null)
                foreach(ResolutionByScreenDto rel in rels)
                {
                    ScreenResolutions.Add(rel);
                    ScreenResolutionDisplays.Add(FormatResolutionDisplay(rel));
                }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading resolutions for screen {Id}", screenId);
        }

        RefreshAvailableSupportedResolutions();
    }

    private string FormatResolutionDisplay(ResolutionByScreenDto rel)
    {
        var res = rel.Resolution?.ResolutionDto;

        if(res != null)
            return $"{res.Width}x{res.Height}" +
                   (res.Colors.HasValue ? $" {res.Colors} colors" : "") +
                   (res.Chars == true ? " (chars)" : "") +
                   (res.Grayscale == true ? " (gray)" : "");

        if(rel.ResolutionId.HasValue && _allResolutions != null)
        {
            ResolutionDto? lookup = _allResolutions.FirstOrDefault(r => r.Id == rel.ResolutionId.Value);

            if(lookup != null)
                return $"{lookup.Width}x{lookup.Height}" +
                       (lookup.Colors.HasValue ? $" {lookup.Colors} colors" : "") +
                       (lookup.Chars == true ? " (chars)" : "") +
                       (lookup.Grayscale == true ? " (gray)" : "");
        }

        return $"Resolution #{rel.ResolutionId}";
    }

    private void RefreshAvailableSupportedResolutions()
    {
        AvailableSupportedResolutions.Clear();

        if(_allResolutions == null) return;

        HashSet<int> assignedIds = new(ScreenResolutions
                                     .Where(r => r.ResolutionId.HasValue)
                                     .Select(r => r.ResolutionId!.Value));

        foreach(ResolutionDto res in _allResolutions)
            if(res.Id.HasValue && !assignedIds.Contains(res.Id.Value))
                AvailableSupportedResolutions.Add(res);
    }

    private async Task AddSupportedResolutionAsync()
    {
        if(_editingId == null || SelectedSupportedResolution?.Id == null) return;

        try
        {
            var dto = new ResolutionByScreenDto
            {
                ScreenId     = _editingId.Value,
                ResolutionId = SelectedSupportedResolution.Id.Value
            };

            await _apiClient.ResolutionsByScreen.PostAsync(dto);
            SelectedSupportedResolution = null;
            await LoadScreenResolutionsAsync(_editingId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding resolution to screen");
            ErrorMessage = _localizer["FailedToSaveScreen"];
            HasError     = true;
        }
    }

    private async Task RemoveSupportedResolutionByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;

        int index = ScreenResolutionDisplays.IndexOf(display);

        if(index >= 0 && index < ScreenResolutions.Count)
        {
            ResolutionByScreenDto rel = ScreenResolutions[index];

            if(rel.Id == null) return;

            try
            {
                await _apiClient.ResolutionsByScreen[rel.Id.Value].DeleteAsync();
                await LoadScreenResolutionsAsync(_editingId.Value);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error removing resolution from screen");
                ErrorMessage = _localizer["FailedToSaveScreen"];
                HasError     = true;
            }
        }
    }

    private async Task DeleteAsync(ScreenDto? item)
    {
        if(item?.Id == null) return;

        try
        {
            await _apiClient.Screens[item.Id.Value].DeleteAsync();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting screen {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteScreen"];
            HasError     = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(Diagonal <= 0)
            {
                ErrorMessage = _localizer["ScreenDiagonalRequired"];
                HasError     = true;
                return;
            }

            if(SelectedResolution?.Id == null)
            {
                ErrorMessage = _localizer["ScreenResolutionRequired"];
                HasError     = true;
                return;
            }

            var dto = new ScreenDto
            {
                Diagonal           = Diagonal,
                Width              = Width,
                Height             = Height,
                EffectiveColors    = EffectiveColors,
                Type               = string.IsNullOrWhiteSpace(ScreenType) ? null : ScreenType,
                NativeResolutionId = SelectedResolution.Id.Value
            };

            if(_editingId == null)
                await _apiClient.Screens.PostAsync(dto);
            else
            {
                dto.Id = _editingId;
                await _apiClient.Screens[_editingId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving screen");
            ErrorMessage = _localizer["FailedToSaveScreen"];
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

    public void ApplyFilter()
    {
        FilteredScreens.Clear();

        IEnumerable<ScreenDto> source = (IEnumerable<ScreenDto>?)_allScreens ?? Screens;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(s => (s.Type != null &&
                                        s.Type.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       s.Diagonal.ToString()?.Contains(FilterText) == true ||
                                       s.Size?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == true);

        foreach(ScreenDto item in source)
            FilteredScreens.Add(item);
    }

    // --- Resolution picker filter ---
    public void UpdateResolutionFilter(string query)
    {
        FilteredResolutions.Clear();

        if(_allResolutions == null) return;

        IEnumerable<ResolutionDto> source = _allResolutions;

        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(r => $"{r.Width}x{r.Height}".Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(ResolutionDto res in source)
            FilteredResolutions.Add(res);
    }

    public async Task LoadPickerDataAsync()
    {
        if(_allResolutions == null)
        {
            try
            {
                _allResolutions = await _apiClient.Resolutions.GetAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error loading resolutions for picker");
            }
        }
    }

    private void ClearForm()
    {
        Diagonal            = 0;
        Width               = null;
        Height              = null;
        EffectiveColors     = null;
        ScreenType          = string.Empty;
        SelectedResolution  = null;
        ResolutionFilterText = string.Empty;
        ScreenResolutions.Clear();
        ScreenResolutionDisplays.Clear();
        AvailableSupportedResolutions.Clear();
        SelectedSupportedResolution = null;
        HasError            = false;
        ErrorMessage        = string.Empty;
    }

    private void PopulateForm(ScreenDto screen)
    {
        Diagonal        = screen.Diagonal ?? 0;
        Width           = screen.Width;
        Height          = screen.Height;
        EffectiveColors = screen.EffectiveColors;
        ScreenType      = screen.Type ?? string.Empty;

        // Resolution picker
        if(screen.NativeResolutionId.HasValue && _allResolutions != null)
        {
            ResolutionDto? res = _allResolutions.FirstOrDefault(r => r.Id == screen.NativeResolutionId.Value);

            if(res != null)
            {
                ResolutionFilterText = $"{res.Width}x{res.Height}";
                UpdateResolutionFilter(ResolutionFilterText);
                SelectedResolution = FilteredResolutions.FirstOrDefault(r => r.Id == res.Id);
            }
            else
            {
                ResolutionFilterText = string.Empty;
                SelectedResolution   = null;
            }
        }
        else
        {
            ResolutionFilterText = string.Empty;
            SelectedResolution   = null;
        }
    }
}
