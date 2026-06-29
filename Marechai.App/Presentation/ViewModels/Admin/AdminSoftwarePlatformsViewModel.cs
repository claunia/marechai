#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Microsoft.Extensions.Configuration;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwarePlatformsViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwarePlatformsService                    _service;
    private readonly IJwtService                                _jwtService;
    private readonly IStringLocalizer                           _localizer;
    private readonly ILogger<AdminSoftwarePlatformsViewModel>   _logger;
    private readonly ITokenService                              _tokenService;
    private readonly IConfiguration                              _configuration;

    [ObservableProperty] private ObservableCollection<SoftwarePlatformDto> _platforms = [];
    [ObservableProperty] private ObservableCollection<SoftwarePlatformDto> _filteredPlatforms = [];
    [ObservableProperty] private string                                   _filterText = string.Empty;
    [ObservableProperty] private SoftwarePlatformDto?                     _selectedPlatform;
    [ObservableProperty] private bool                                      _isLoading;
    [ObservableProperty] private bool                                      _isDataLoaded;
    [ObservableProperty] private bool                                      _hasError;
    [ObservableProperty] private string                                   _errorMessage = string.Empty;
    [ObservableProperty] private bool                                      _isAdmin;
    [ObservableProperty] private bool                                      _isEditing;
    [ObservableProperty] private string                                   _editPanelTitle = string.Empty;
    [ObservableProperty] private string                                   _platformName = string.Empty;
    [ObservableProperty] private Guid?                                    _logoId;
    [ObservableProperty] private bool                                      _isUploadingLogo;
    [ObservableProperty] private bool                                      _isMerging;
    [ObservableProperty] private SoftwarePlatformDto?                     _selectedMergeTargetPlatform;
    [ObservableProperty] private ObservableCollection<SoftwarePlatformDto> _mergeTargetSuggestions = [];
    [ObservableProperty] private ObservableCollection<SoftwarePlatformSelectionItem> _mergeSourceCandidates = [];
    [ObservableProperty] private string                                   _mergeTargetSearchText = string.Empty;

    private int?                       _editingId;
    private List<SoftwarePlatformDto>? _allPlatforms;

    public bool CanConfirmMerge =>
        SelectedMergeTargetPlatform?.Id is not null && MergeSourceCandidates.Any(candidate => candidate.IsSelected);

    public string? LogoThumbnailUrl => LogoId.HasValue
                                            ? $"{_configuration.GetSection("ApiClient:Url").Value}/assets/photos/platform-logos/thumbs/webp/4k/{LogoId}.webp"
                                            : null;

    public AdminSoftwarePlatformsViewModel(SoftwarePlatformsService                  service,
                                           IJwtService                               jwtService,
                                           ITokenService                             tokenService,
                                           ILogger<AdminSoftwarePlatformsViewModel>  logger,
                                           IStringLocalizer                          localizer,
                                           IConfiguration                             configuration)
    {
        _service      = service;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;
        _configuration = configuration;

        LoadCommand   = new AsyncRelayCommand(LoadAsync);
        OpenAddCommand = new RelayCommand(OpenAdd);
        OpenEditCommand = new RelayCommand<SoftwarePlatformDto>(OpenEdit);
        DeleteCommand  = new AsyncRelayCommand<SoftwarePlatformDto>(DeleteAsync);
        SaveCommand    = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);
        UploadLogoCommand = new AsyncRelayCommand(UploadLogoAsync);
        RemoveLogoCommand = new AsyncRelayCommand(RemoveLogoAsync);
        OpenMergeCommand = new RelayCommand(OpenMerge);
        ConfirmMergeCommand = new AsyncRelayCommand(ConfirmMergeAsync);
        CancelMergeCommand = new RelayCommand(CancelMerge);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                       LoadCommand      { get; }
    public IRelayCommand                            OpenAddCommand   { get; }
    public IRelayCommand<SoftwarePlatformDto>       OpenEditCommand  { get; }
    public IAsyncRelayCommand<SoftwarePlatformDto>  DeleteCommand    { get; }
    public IAsyncRelayCommand                       SaveCommand      { get; }
    public IRelayCommand                            CancelEditCommand { get; }
    public IAsyncRelayCommand                       UploadLogoCommand { get; }
    public IAsyncRelayCommand                       RemoveLogoCommand { get; }
    public IRelayCommand                            OpenMergeCommand  { get; }
    public IAsyncRelayCommand                       ConfirmMergeCommand { get; }
    public IRelayCommand                            CancelMergeCommand { get; }

    partial void OnLogoIdChanged(Guid? value) => OnPropertyChanged(nameof(LogoThumbnailUrl));
    partial void OnSelectedMergeTargetPlatformChanged(SoftwarePlatformDto? value)
    {
        BuildMergeSourceCandidates(value);
        OnPropertyChanged(nameof(CanConfirmMerge));
    }

    public string MergeConfirmDialogTitle   => _localizer["MergeConfirmDialogTitle"];
    public string MergeConfirmDialogMessage => _localizer["MergeConfirmDialogMessage"];
    public string MergeButtonText           => _localizer["MergeButton"];
    public string CancelButtonText          => _localizer["CancelButton"];

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
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Platforms.Clear();

            List<SoftwarePlatformDto> response = await _service.GetAllAsync();
            _allPlatforms = response;

            foreach(SoftwarePlatformDto item in response) Platforms.Add(item);

            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software platforms");
            ErrorMessage = _localizer["FailedToLoadSoftwarePlatforms"];
            HasError     = true;
        }
        finally { IsLoading = false; }
    }

    private void OpenAdd()
    {
        CancelMerge();
        _editingId     = null;
        EditPanelTitle = _localizer["AddSoftwarePlatformDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    private void OpenEdit(SoftwarePlatformDto? item)
    {
        if(item == null) return;

        CancelMerge();
        _editingId     = item.Id;
        EditPanelTitle = _localizer["EditSoftwarePlatformDialog_Title"];
        PlatformName   = item.Name ?? string.Empty;
        LogoId         = item.LogoId;
        HasError       = false;
        ErrorMessage   = string.Empty;
        IsEditing      = true;
    }

    private async Task DeleteAsync(SoftwarePlatformDto? item)
    {
        if(item?.Id == null) return;

        try
        {
            await _service.DeleteAsync(item.Id.Value);
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software platform {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteSoftwarePlatform"];
            HasError     = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(PlatformName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError     = true;
                return;
            }

            var dto = new SoftwarePlatformDto { Name = PlatformName };

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
            _logger.LogError(ex, "Error saving software platform");
            ErrorMessage = _localizer["FailedToSaveSoftwarePlatform"];
            HasError     = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing    = false;
        _editingId   = null;
        ClearForm();
        HasError     = false;
        ErrorMessage = string.Empty;
    }

    private void OpenMerge()
    {
        CancelEdit();
        HasError       = false;
        ErrorMessage   = string.Empty;
        IsMerging      = true;
        MergeTargetSearchText = string.Empty;
        BuildMergeTargetSuggestions(string.Empty);
        SelectedMergeTargetPlatform = null;
        BuildMergeSourceCandidates(null);
    }

    public void UpdateMergeTargetSuggestions(string query)
    {
        MergeTargetSearchText = query;
        BuildMergeTargetSuggestions(query);
    }

    private void BuildMergeTargetSuggestions(string query)
    {
        MergeTargetSuggestions.Clear();

        if(_allPlatforms == null) return;

        IEnumerable<SoftwarePlatformDto> source = _allPlatforms;

        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(platform => platform.Name != null &&
                                              platform.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(SoftwarePlatformDto match in source)
            MergeTargetSuggestions.Add(match);
    }

    private void BuildMergeSourceCandidates(SoftwarePlatformDto? target)
    {
        foreach(SoftwarePlatformSelectionItem candidate in MergeSourceCandidates)
            candidate.SelectionChanged -= OnMergeSourceSelectionChanged;

        MergeSourceCandidates.Clear();

        if(target?.Id == null || _allPlatforms == null) return;

        foreach(SoftwarePlatformDto platform in _allPlatforms.Where(platform => platform.Id != target.Id))
        {
            var candidate = new SoftwarePlatformSelectionItem
            {
                Platform = platform
            };

            candidate.SelectionChanged += OnMergeSourceSelectionChanged;
            MergeSourceCandidates.Add(candidate);
        }
    }

    private void OnMergeSourceSelectionChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(CanConfirmMerge));

    private async Task ConfirmMergeAsync()
    {
        if(SelectedMergeTargetPlatform?.Id is not int targetId) return;

        List<int> sourceIds = MergeSourceCandidates
                              .Where(candidate => candidate.IsSelected && candidate.Platform.Id is not null)
                              .Select(candidate => candidate.Platform.Id!.Value)
                              .ToList();

        if(sourceIds.Count == 0)
        {
            OnPropertyChanged(nameof(CanConfirmMerge));

            return;
        }

        try
        {
            HasError     = false;
            ErrorMessage = string.Empty;

            bool succeeded = await _service.MergeAsync(targetId, sourceIds);

            if(!succeeded)
            {
                ErrorMessage = _localizer["FailedToMergeSoftwarePlatforms"];
                HasError     = true;

                return;
            }

            CancelMerge();
            await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error merging software platforms into {TargetId}", targetId);
            ErrorMessage = _localizer["FailedToMergeSoftwarePlatforms"];
            HasError     = true;
        }
    }

    private void CancelMerge()
    {
        IsMerging = false;
        MergeTargetSearchText = string.Empty;
        SelectedMergeTargetPlatform = null;
        BuildMergeTargetSuggestions(string.Empty);
        BuildMergeSourceCandidates(null);
        HasError     = false;
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(CanConfirmMerge));
    }

    public void ApplyFilter()
    {
        FilteredPlatforms.Clear();
        IEnumerable<SoftwarePlatformDto> source = (IEnumerable<SoftwarePlatformDto>?)_allPlatforms ?? Platforms;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(p => p.Name != null &&
                                       p.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase));

        foreach(SoftwarePlatformDto item in source) FilteredPlatforms.Add(item);
    }

    private void ClearForm()
    {
        PlatformName = string.Empty;
        LogoId       = null;
        HasError     = false;
        ErrorMessage = string.Empty;
    }

    private async Task UploadLogoAsync()
    {
        if(_editingId == null) return;

        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".webp");
            picker.FileTypeFilter.Add(".bmp");

#if !HAS_UNO
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
#endif

            Windows.Storage.StorageFile? file = await picker.PickSingleFileAsync();

            if(file == null) return;

            IsUploadingLogo = true;
            HasError        = false;

            using Stream stream = await file.OpenStreamForReadAsync();
            using var    ms     = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            SoftwarePlatformDto? result =
                await _service.UploadLogoAsync(_editingId.Value, fileBytes, file.Name, file.ContentType);

            if(result == null)
            {
                ErrorMessage = _localizer["FailedToUploadLogo"];
                HasError     = true;

                return;
            }

            LogoId = result.LogoId;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading logo for software platform {Id}", _editingId);
            ErrorMessage = _localizer["FailedToUploadLogo"];
            HasError     = true;
        }
        finally
        {
            IsUploadingLogo = false;
        }
    }

    private async Task RemoveLogoAsync()
    {
        if(_editingId == null) return;

        try
        {
            IsUploadingLogo = true;
            HasError        = false;

            bool success = await _service.DeleteLogoAsync(_editingId.Value);

            if(!success)
            {
                ErrorMessage = _localizer["FailedToRemoveLogo"];
                HasError     = true;

                return;
            }

            LogoId = null;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing logo for software platform {Id}", _editingId);
            ErrorMessage = _localizer["FailedToRemoveLogo"];
            HasError     = true;
        }
        finally
        {
            IsUploadingLogo = false;
        }
    }
}

public partial class SoftwarePlatformSelectionItem : ObservableObject
{
    [ObservableProperty] private SoftwarePlatformDto _platform = null!;
    [ObservableProperty] private bool _isSelected;

    public event EventHandler? SelectionChanged;

    partial void OnIsSelectedChanged(bool value) => SelectionChanged?.Invoke(this, EventArgs.Empty);
}
