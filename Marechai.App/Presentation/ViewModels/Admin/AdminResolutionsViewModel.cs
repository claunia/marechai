#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminResolutionsViewModel : ObservableObject, IRegionAware
{
    private readonly ApiClient                            _apiClient;
    private readonly IJwtService                          _jwtService;
    private readonly IStringLocalizer                     _localizer;
    private readonly ILogger<AdminResolutionsViewModel>   _logger;
    private readonly ITokenService                        _tokenService;

    [ObservableProperty]
    private ObservableCollection<ResolutionDto> _resolutions = [];

    [ObservableProperty]
    private ObservableCollection<ResolutionDto> _filteredResolutions = [];

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private ResolutionDto? _selectedResolution;

    private List<ResolutionDto>? _allResolutions;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editPanelTitle = string.Empty;

    private int? _editingId;

    // --- Form fields ---
    [ObservableProperty]
    private double _width;

    [ObservableProperty]
    private double _height;

    [ObservableProperty]
    private double? _colors;

    [ObservableProperty]
    private double? _palette;

    [ObservableProperty]
    private bool _chars;

    [ObservableProperty]
    private bool _grayscale;

    public AdminResolutionsViewModel(ApiClient                            apiClient,
                                     IJwtService                          jwtService,
                                     ITokenService                        tokenService,
                                     ILogger<AdminResolutionsViewModel>   logger,
                                     IStringLocalizer                     localizer)
    {
        _apiClient    = apiClient;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadItemsCommand  = new AsyncRelayCommand(LoadItemsAsync);
        OpenAddCommand    = new RelayCommand(OpenAdd);
        OpenEditCommand   = new RelayCommand<ResolutionDto>(OpenEdit);
        DeleteCommand     = new AsyncRelayCommand<ResolutionDto>(DeleteAsync);
        SaveCommand       = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);

        CheckAdminRole();
    }

    public IAsyncRelayCommand               LoadItemsCommand  { get; }
    public IRelayCommand                    OpenAddCommand    { get; }
    public IRelayCommand<ResolutionDto>      OpenEditCommand   { get; }
    public IAsyncRelayCommand<ResolutionDto> DeleteCommand     { get; }
    public IAsyncRelayCommand               SaveCommand       { get; }
    public IRelayCommand                    CancelEditCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
            _ = LoadItemsCommand.ExecuteAsync(null);
    }

    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(string.IsNullOrWhiteSpace(token))
            {
                IsAdmin = false;

                return;
            }

            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("Uberadmin",      StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Resolutions.Clear();

            List<ResolutionDto>? response = await _apiClient.Resolutions.GetAsync();
            _allResolutions = response;

            if(response != null)
                foreach(ResolutionDto item in response)
                    Resolutions.Add(item);

            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading resolutions");
            ErrorMessage = _localizer["FailedToLoadResolutions"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenAdd()
    {
        _editingId     = null;
        EditPanelTitle = _localizer["AddResolutionDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    private void OpenEdit(ResolutionDto? item)
    {
        if(item == null) return;

        _editingId     = item.Id;
        EditPanelTitle = _localizer["EditResolutionDialog_Title"];
        Width          = item.Width ?? 0;
        Height         = item.Height ?? 0;
        Colors         = item.Colors;
        Palette        = item.Palette;
        Chars          = item.Chars ?? false;
        Grayscale      = item.Grayscale ?? false;
        HasError       = false;
        ErrorMessage   = string.Empty;
        IsEditing      = true;
    }

    private async Task DeleteAsync(ResolutionDto? item)
    {
        if(item?.Id == null) return;

        try
        {
            await _apiClient.Resolutions[item.Id.Value].DeleteAsync();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting resolution {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteResolution"];
            HasError     = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(Width < 1 || Height < 1)
            {
                ErrorMessage = _localizer["ResolutionDimensionsRequired"];
                HasError     = true;

                return;
            }

            var dto = new ResolutionDto
            {
                Width     = (int)Width,
                Height    = (int)Height,
                Colors    = Colors.HasValue ? (long)Colors.Value : null,
                Palette   = Palette.HasValue ? (long)Palette.Value : null,
                Chars     = Chars,
                Grayscale = Grayscale
            };

            if(_editingId == null)
                await _apiClient.Resolutions.PostAsync(dto);
            else
            {
                dto.Id = _editingId;
                await _apiClient.Resolutions[_editingId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving resolution");
            ErrorMessage = _localizer["FailedToSaveResolution"];
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
        FilteredResolutions.Clear();

        IEnumerable<ResolutionDto> source = (IEnumerable<ResolutionDto>?)_allResolutions ?? Resolutions;

        if(!string.IsNullOrWhiteSpace(FilterText))
        {
            string filter = FilterText;
            source = source.Where(r =>
                $"{r.Width}x{r.Height}".Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                (r.Colors?.ToString()?.Contains(filter) == true));
        }

        foreach(ResolutionDto item in source)
            FilteredResolutions.Add(item);
    }

    private void ClearForm()
    {
        Width     = 0;
        Height    = 0;
        Colors    = null;
        Palette   = null;
        Chars     = false;
        Grayscale = false;
        HasError     = false;
        ErrorMessage = string.Empty;
    }
}
