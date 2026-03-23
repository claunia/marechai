#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.App.Services.Caching;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareScreenshotsViewModel : ObservableObject, IRegionAware
{
    readonly ApiClient          _apiClient;
    readonly SoftwareScreenshotCache _screenshotCache;
    readonly ImageSourceFactory      _imageSourceFactory;
    readonly IJwtService             _jwtService;
    readonly ITokenService           _tokenService;
    readonly IStringLocalizer        _localizer;
    readonly ILogger<AdminSoftwareScreenshotsViewModel> _logger;
    readonly IRegionManager          _regionManager;

    List<SoftwareScreenshotDto>? _allScreenshots;
    List<SoftwareDto>?           _allSoftware;
    List<SoftwarePlatformDto>?   _allPlatforms;
    List<SoftwareVersionDto>?    _allVersions;

    [ObservableProperty]
    private ObservableCollection<ScreenshotGridItem> _screenshots = [];

    [ObservableProperty]
    private ScreenshotGridItem? _selectedScreenshot;

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
    private bool _isUploading;

    [ObservableProperty]
    private bool _isEditingExisting;

    [ObservableProperty]
    private bool _isEditing;

    // Edit panel fields
    [ObservableProperty]
    private string _editCaption = string.Empty;

    // Software picker
    [ObservableProperty]
    private ObservableCollection<string> _softwareSuggestions = [];

    [ObservableProperty]
    private SoftwareDto? _selectedSoftware;

    [ObservableProperty]
    private string _softwareSearchText = string.Empty;

    // Platform picker
    [ObservableProperty]
    private ObservableCollection<string> _platformSuggestions = [];

    [ObservableProperty]
    private SoftwarePlatformDto? _selectedPlatform;

    [ObservableProperty]
    private string _platformSearchText = string.Empty;

    // Version picker (filtered by selected software)
    [ObservableProperty]
    private ObservableCollection<string> _versionSuggestions = [];

    [ObservableProperty]
    private SoftwareVersionDto? _selectedVersion;

    [ObservableProperty]
    private string _versionSearchText = string.Empty;

    public AdminSoftwareScreenshotsViewModel(ApiClient                                    apiClient,
                                              SoftwareScreenshotCache                      screenshotCache,
                                              ImageSourceFactory                           imageSourceFactory,
                                              IJwtService                                  jwtService,
                                              ITokenService                                tokenService,
                                              IStringLocalizer                             localizer,
                                              ILogger<AdminSoftwareScreenshotsViewModel>   logger,
                                              IRegionManager                               regionManager)
    {
        _apiClient          = apiClient;
        _screenshotCache    = screenshotCache;
        _imageSourceFactory = imageSourceFactory;
        _jwtService         = jwtService;
        _tokenService       = tokenService;
        _localizer          = localizer;
        _logger             = logger;
        _regionManager      = regionManager;

        LoadCommand              = new AsyncRelayCommand(LoadAsync);
        UploadScreenshotCommand  = new AsyncRelayCommand(UploadScreenshotAsync);
        DeleteScreenshotCommand  = new AsyncRelayCommand<ScreenshotGridItem>(DeleteScreenshotAsync);
        ViewScreenshotCommand    = new RelayCommand<ScreenshotGridItem>(ViewScreenshot);
        OpenEditCommand          = new RelayCommand<ScreenshotGridItem>(OpenEdit);
        SaveEditCommand          = new AsyncRelayCommand(SaveEditAsync);
        CancelEditCommand        = new RelayCommand(CancelEdit);
        GoBackCommand            = new RelayCommand(GoBack);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                    LoadCommand             { get; }
    public IAsyncRelayCommand                    UploadScreenshotCommand { get; }
    public IAsyncRelayCommand<ScreenshotGridItem> DeleteScreenshotCommand { get; }
    public IRelayCommand<ScreenshotGridItem>     ViewScreenshotCommand   { get; }
    public IRelayCommand<ScreenshotGridItem>     OpenEditCommand         { get; }
    public IAsyncRelayCommand                    SaveEditCommand         { get; }
    public IRelayCommand                         CancelEditCommand       { get; }
    public IRelayCommand                         GoBackCommand           { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
        {
            _ = LoadPickerDataAsync();
            _ = LoadCommand.ExecuteAsync(null);
        }
    }

    void CheckAdminRole()
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

    async Task LoadPickerDataAsync()
    {
        try
        {
            var softwareResult = await _apiClient.Software.GetAsync();
            _allSoftware = softwareResult?.ToList() ?? [];

            var platformResult = await _apiClient.Software.Platforms.GetAsync();
            _allPlatforms = platformResult?.ToList() ?? [];

            var versionsResult = await _apiClient.Software.Versions.GetAsync();
            _allVersions = versionsResult?.ToList() ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading picker data");
        }
    }

    async Task LoadAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Screenshots.Clear();

            var result = await _apiClient.Software.Screenshots.GetAsync();
            _allScreenshots = result?.ToList() ?? [];

            foreach(SoftwareScreenshotDto dto in _allScreenshots)
            {
                var item = new ScreenshotGridItem
                {
                    Id           = dto.Id ?? Guid.Empty,
                    SoftwareName = dto.SoftwareName ?? string.Empty,
                    PlatformName = dto.PlatformName ?? string.Empty,
                    VersionString = dto.VersionString ?? string.Empty,
                    Caption      = dto.Caption ?? string.Empty
                };

                Screenshots.Add(item);
                _ = LoadThumbnailAsync(item);
            }

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading screenshots");
            ErrorMessage = _localizer["FailedToLoadScreenshots"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task LoadThumbnailAsync(ScreenshotGridItem item)
    {
        try
        {
            Stream stream = await _screenshotCache.GetThumbnailAsync(item.Id);
            item.ThumbnailSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading screenshot thumbnail {Id}", item.Id);
        }
    }

    async Task UploadScreenshotAsync()
    {
        if(SelectedSoftware is null)
        {
            ErrorMessage = _localizer["SelectSoftwareRequired"];
            HasError     = true;

            return;
        }

        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".webp");
            picker.FileTypeFilter.Add(".tiff");
            picker.FileTypeFilter.Add(".tif");
            picker.FileTypeFilter.Add(".bmp");

#if !HAS_UNO
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
#endif

            Windows.Storage.StorageFile? file = await picker.PickSingleFileAsync();

            if(file == null) return;

            IsUploading = true;
            HasError    = false;

            using Stream stream = await file.OpenStreamForReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            var body = new Marechai.App.Software.Screenshots.Upload.UploadPostRequestBody
            {
                File       = fileBytes,
                SoftwareId = (int?)(SelectedSoftware.Id ?? 0),
                SoftwarePlatformId = SelectedPlatform is not null ? (int?)(SelectedPlatform.Id ?? 0) : null,
                SoftwareVersionId  = SelectedVersion is not null ? (int?)(SelectedVersion.Id ?? 0) : null,
                Caption = string.IsNullOrWhiteSpace(EditCaption) ? null : EditCaption
            };

            await _apiClient.Software.Screenshots.Upload.PostAsync(body);

            EditCaption = string.Empty;
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading screenshot");
            ErrorMessage = _localizer["FailedToUploadScreenshot"];
            HasError     = true;
        }
        finally
        {
            IsUploading = false;
        }
    }

    async Task DeleteScreenshotAsync(ScreenshotGridItem? item)
    {
        if(item is null) return;

        try
        {
            HasError     = false;
            ErrorMessage = string.Empty;

            await _apiClient.Software.Screenshots[item.Id.ToString()].DeleteAsync();
            await _screenshotCache.InvalidateCacheAsync(item.Id);
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting screenshot {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteScreenshot"];
            HasError     = true;
        }
    }

    void ViewScreenshot(ScreenshotGridItem? item)
    {
        if(item is null) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.ScreenshotId, item.Id }
        };

        _regionManager.RequestNavigate(RegionNames.Content, "ScreenshotDetailPage", parameters);
    }

    void OpenEdit(ScreenshotGridItem? item)
    {
        if(item is null) return;

        SoftwareScreenshotDto? dto = _allScreenshots?.FirstOrDefault(s => s.Id == item.Id);

        if(dto is null) return;

        IsEditingExisting = true;
        IsEditing         = true;
        EditCaption       = dto.Caption ?? string.Empty;
        SelectedScreenshot = item;

        SelectedSoftware = _allSoftware?.FirstOrDefault(s => s.Id == dto.SoftwareId);
        SelectedPlatform = _allPlatforms?.FirstOrDefault(p => p.Id == dto.SoftwarePlatformId);
        SelectedVersion  = _allVersions?.FirstOrDefault(v => v.Id == dto.SoftwareVersionId);

        UpdateSoftwareSuggestions(string.Empty);
        UpdatePlatformSuggestions(string.Empty);
        UpdateVersionSuggestions(string.Empty);
    }

    async Task SaveEditAsync()
    {
        if(SelectedScreenshot is null) return;

        try
        {
            HasError     = false;
            ErrorMessage = string.Empty;

            var dto = new SoftwareScreenshotDto
            {
                Id                 = SelectedScreenshot.Id,
                SoftwareId         = SelectedSoftware?.Id ?? 0,
                SoftwarePlatformId = SelectedPlatform?.Id,
                SoftwareVersionId  = SelectedVersion?.Id,
                Caption            = string.IsNullOrWhiteSpace(EditCaption) ? null : EditCaption,
                OriginalExtension  = string.Empty // not updated
            };

            await _apiClient.Software.Screenshots[SelectedScreenshot.Id.ToString()].PutAsync(dto);

            IsEditing         = false;
            IsEditingExisting = false;
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving screenshot");
            ErrorMessage = _localizer["FailedToSaveScreenshot"];
            HasError     = true;
        }
    }

    void CancelEdit()
    {
        IsEditing         = false;
        IsEditingExisting = false;
    }

    public void UpdateSoftwareSuggestions(string text)
    {
        SoftwareSuggestions.Clear();

        if(_allSoftware is null) return;

        IEnumerable<SoftwareDto> filtered = string.IsNullOrEmpty(text)
                                                 ? _allSoftware
                                                 : _allSoftware.Where(s =>
                                                       s.Name?.Contains(text,
                                                                        StringComparison
                                                                           .OrdinalIgnoreCase) ??
                                                       false);

        foreach(SoftwareDto s in filtered.OrderBy(s => s.Name))
            SoftwareSuggestions.Add(s.Name ?? string.Empty);
    }

    public void UpdatePlatformSuggestions(string text)
    {
        PlatformSuggestions.Clear();

        if(_allPlatforms is null) return;

        IEnumerable<SoftwarePlatformDto> filtered = string.IsNullOrEmpty(text)
                                                         ? _allPlatforms
                                                         : _allPlatforms.Where(p =>
                                                               p.Name?.Contains(text,
                                                                                StringComparison
                                                                                   .OrdinalIgnoreCase) ??
                                                               false);

        foreach(SoftwarePlatformDto p in filtered.OrderBy(p => p.Name))
            PlatformSuggestions.Add(p.Name ?? string.Empty);
    }

    public void UpdateVersionSuggestions(string text)
    {
        VersionSuggestions.Clear();

        if(_allVersions is null) return;

        // Filter versions by selected software
        IEnumerable<SoftwareVersionDto> filtered = _allVersions;

        if(SelectedSoftware is not null)
            filtered = filtered.Where(v => v.SoftwareId == SelectedSoftware.Id);

        if(!string.IsNullOrEmpty(text))
            filtered = filtered.Where(v =>
                                          (v.VersionString?.Contains(text, StringComparison.OrdinalIgnoreCase) ??
                                           false) ||
                                          (v.PublicVersion?.Contains(text, StringComparison.OrdinalIgnoreCase) ??
                                           false));

        foreach(SoftwareVersionDto v in filtered.OrderBy(v => v.VersionString))
            VersionSuggestions.Add(v.PublicVersion is not null
                                       ? $"{v.PublicVersion} ({v.VersionString})"
                                       : v.VersionString ?? string.Empty);
    }

    public void OnSoftwareSuggestionChosen(string? chosen)
    {
        if(chosen is null) return;

        SelectedSoftware = _allSoftware?.FirstOrDefault(s => s.Name == chosen);
        UpdateVersionSuggestions(string.Empty);
    }

    public void OnPlatformSuggestionChosen(string? chosen)
    {
        if(chosen is null) return;

        SelectedPlatform = _allPlatforms?.FirstOrDefault(p => p.Name == chosen);
    }

    public void OnVersionSuggestionChosen(string? chosen)
    {
        if(chosen is null) return;

        SelectedVersion = _allVersions?.FirstOrDefault(v =>
        {
            string display = v.PublicVersion is not null
                                 ? $"{v.PublicVersion} ({v.VersionString})"
                                 : v.VersionString ?? string.Empty;

            return display == chosen;
        });
    }

    void GoBack() => _regionManager.Regions[RegionNames.Content].NavigationService.Journal.GoBack();
}

public partial class ScreenshotGridItem : ObservableObject
{
    public Guid   Id            { get; set; }
    public string SoftwareName  { get; set; } = string.Empty;
    public string PlatformName  { get; set; } = string.Empty;
    public string VersionString { get; set; } = string.Empty;
    public string Caption       { get; set; } = string.Empty;

    [ObservableProperty]
    private Microsoft.UI.Xaml.Media.ImageSource? _thumbnailSource;
}
