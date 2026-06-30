#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BatchJobState = Marechai.Data.Dtos.BatchJobState;
using AdminSoftwareScreenshotBatchCommitItemDto = Marechai.ApiClient.Models.AdminSoftwareScreenshotBatchCommitItemDto;
using AdminSoftwareScreenshotBatchCommitRequestDto = Marechai.ApiClient.Models.AdminSoftwareScreenshotBatchCommitRequestDto;
using AdminSoftwareScreenshotBatchJobItemResultDto = Marechai.ApiClient.Models.AdminSoftwareScreenshotBatchJobItemResultDto;
using AdminSoftwareScreenshotBatchJobStatusDto = Marechai.ApiClient.Models.AdminSoftwareScreenshotBatchJobStatusDto;
using AdminPendingSoftwareScreenshotUploadDto = Marechai.ApiClient.Models.AdminPendingSoftwareScreenshotUploadDto;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareScreenshotsViewModel : ObservableObject, IRegionAware
{
    const int  MaxImages        = 25;
    const long MaxFileSizeBytes = 50 * 1024 * 1024;

    static readonly HashSet<string> _allowedExtensions =
    [
        ".jpg", ".jpeg", ".png", ".webp", ".avif", ".bmp", ".tif", ".tiff"
    ];

    readonly Client                                    _apiClient;
    readonly SoftwareScreenshotsService                 _screenshotsService;
    readonly SoftwareScreenshotCache                    _screenshotCache;
    readonly ImageSourceFactory                         _imageSourceFactory;
    readonly IJwtService                                _jwtService;
    readonly ITokenService                              _tokenService;
    readonly IStringLocalizer                           _localizer;
    readonly ILogger<AdminSoftwareScreenshotsViewModel> _logger;
    readonly IRegionManager                              _regionManager;

    List<SoftwareScreenshotDto>? _allScreenshots;
    List<SoftwarePlatformDto>?   _allPlatforms;
    List<SoftwareVersionDto>?    _allVersions;
    int                          _softwareId;

    [ObservableProperty]
    private ObservableCollection<ScreenshotGridItem> _screenshots = [];

    [ObservableProperty]
    private ObservableCollection<AdminSoftwareScreenshotStagedItem> _stagedScreenshots = [];

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
    private string _pageTitle = string.Empty;

    [ObservableProperty]
    private bool _isUploading;

    [ObservableProperty]
    private bool _isBatchCommitting;

    [ObservableProperty]
    private bool _isEditingExisting;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _hasStatusMessage;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;

    [ObservableProperty]
    private int _batchProcessed;

    [ObservableProperty]
    private int _batchTotal;

    [ObservableProperty]
    private Guid? _currentPendingId;

    [ObservableProperty]
    private bool _showBatchProgress;

    [ObservableProperty]
    private bool _showBatchSummary;

    [ObservableProperty]
    private string _canonicalGroupName = string.Empty;

    // Edit panel fields
    [ObservableProperty]
    private string _editCaption = string.Empty;

    [ObservableProperty]
    private SoftwareDto? _selectedSoftware;

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

    public AdminSoftwareScreenshotsViewModel(Client                                      apiClient,
                                              SoftwareScreenshotsService                  screenshotsService,
                                              SoftwareScreenshotCache                     screenshotCache,
                                              ImageSourceFactory                          imageSourceFactory,
                                              IJwtService                                 jwtService,
                                              ITokenService                               tokenService,
                                              IStringLocalizer                            localizer,
                                              ILogger<AdminSoftwareScreenshotsViewModel>  logger,
                                              IRegionManager                              regionManager)
    {
        _apiClient          = apiClient;
        _screenshotsService = screenshotsService;
        _screenshotCache    = screenshotCache;
        _imageSourceFactory = imageSourceFactory;
        _jwtService         = jwtService;
        _tokenService       = tokenService;
        _localizer          = localizer;
        _logger             = logger;
        _regionManager      = regionManager;

        LoadCommand                   = new AsyncRelayCommand(LoadAsync);
        AddScreenshotsCommand         = new AsyncRelayCommand(AddScreenshotsAsync);
        SubmitBatchCommand            = new AsyncRelayCommand(SubmitBatchAsync);
        ClearStagedScreenshotsCommand = new AsyncRelayCommand(ClearStagedScreenshotsAsync);
        RemoveStagedScreenshotCommand = new AsyncRelayCommand<AdminSoftwareScreenshotStagedItem>(RemoveStagedScreenshotAsync);
        DeleteScreenshotCommand       = new AsyncRelayCommand<ScreenshotGridItem>(DeleteScreenshotAsync);
        ViewScreenshotCommand         = new RelayCommand<ScreenshotGridItem>(ViewScreenshot);
        OpenEditCommand               = new RelayCommand<ScreenshotGridItem>(OpenEdit);
        SaveEditCommand               = new AsyncRelayCommand(SaveEditAsync);
        CancelEditCommand             = new RelayCommand(CancelEdit);
        GoBackCommand                 = new RelayCommand(GoBack);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                              LoadCommand                   { get; }
    public IAsyncRelayCommand                              AddScreenshotsCommand         { get; }
    public IAsyncRelayCommand                              SubmitBatchCommand            { get; }
    public IAsyncRelayCommand                              ClearStagedScreenshotsCommand { get; }
    public IAsyncRelayCommand<AdminSoftwareScreenshotStagedItem> RemoveStagedScreenshotCommand { get; }
    public IAsyncRelayCommand<ScreenshotGridItem>           DeleteScreenshotCommand       { get; }
    public IRelayCommand<ScreenshotGridItem>                ViewScreenshotCommand         { get; }
    public IRelayCommand<ScreenshotGridItem>                OpenEditCommand               { get; }
    public IAsyncRelayCommand                               SaveEditCommand               { get; }
    public IRelayCommand                                    CancelEditCommand             { get; }
    public IRelayCommand                                    GoBackCommand                 { get; }

    public bool HasStagedScreenshots => StagedScreenshots.Count > 0;
    public bool CanAddMoreScreenshots => !IsUploading && SelectedSoftware is not null && StagedScreenshots.Count < MaxImages;
    public bool CanSubmitBatch => StagedScreenshots.Count > 0 &&
                                  !IsUploading &&
                                  !IsBatchCommitting &&
                                  StagedScreenshots.All(s => s.IsReady);
    public bool CanClearStaged => HasStagedScreenshots && !IsBatchCommitting && !IsUploading;
    public bool HasBatchProgressText => ShowBatchProgress && BatchTotal > 0;
    public double BatchProgressPercent => BatchTotal <= 0 ? 0 : Math.Min(100, BatchProcessed * 100.0 / BatchTotal);
    public string BatchProgressText => string.Format(_localizer["SoftwareScreenshotsBatchProgressText"], BatchProcessed, BatchTotal);

    partial void OnIsUploadingChanged(bool value) => NotifyUploadStateChanged();
    partial void OnIsBatchCommittingChanged(bool value) => NotifyUploadStateChanged();
    partial void OnBatchProcessedChanged(int value) => NotifyBatchProgressChanged();
    partial void OnBatchTotalChanged(int value) => NotifyBatchProgressChanged();
    partial void OnShowBatchProgressChanged(bool value) => OnPropertyChanged(nameof(HasBatchProgressText));
    partial void OnSelectedSoftwareChanged(SoftwareDto? value) => OnPropertyChanged(nameof(CanAddMoreScreenshots));

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        _ = ClearStagedScreenshotsAsync();
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareId, out int softwareId))
            _softwareId = softwareId;

        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.SoftwareName, out string? name))
            PageTitle = string.Format(_localizer["SoftwareScreenshotsForTitle"], name);

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

            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
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
            SelectedSoftware = await _apiClient.Software[_softwareId].GetAsync();

            var platformResult = await _apiClient.Software.Platforms.GetAsync();
            _allPlatforms = platformResult?.ToList() ?? [];

            var versionsResult = await _apiClient.Software.Versions.GetAsync();
            _allVersions = versionsResult?.ToList() ?? [];

            UpdatePlatformSuggestions(string.Empty);
            UpdateVersionSuggestions(string.Empty);
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

            List<Guid?> guids = await _apiClient.Software[_softwareId].Screenshots.GetAsync() ?? [];
            _allScreenshots = [];

            foreach(Guid? guid in guids)
            {
                if(guid is not Guid id) continue;

                SoftwareScreenshotDto? dto = await _apiClient.Software.Screenshots[id].GetAsync();

                if(dto is not null) _allScreenshots.Add(dto);
            }

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

    async Task AddScreenshotsAsync()
    {
        try
        {
            ClearStatusMessage();

            if(SelectedSoftware is null)
            {
                SetStatusMessage(_localizer["SelectSoftwareRequired"], InfoBarSeverity.Warning);

                return;
            }

            if(StagedScreenshots.Count >= MaxImages)
            {
                SetStatusMessage(string.Format(_localizer["SoftwareScreenshotsMaxImagesReached"], MaxImages),
                                 InfoBarSeverity.Warning);

                return;
            }

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".webp");
            picker.FileTypeFilter.Add(".avif");
            picker.FileTypeFilter.Add(".tiff");
            picker.FileTypeFilter.Add(".tif");
            picker.FileTypeFilter.Add(".bmp");

#if !HAS_UNO
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
#endif

            IReadOnlyList<Windows.Storage.StorageFile>? files = await picker.PickMultipleFilesAsync();

            if(files == null || files.Count == 0)
            {
                SetStatusMessage(_localizer["SoftwareScreenshotsPickerCanceled"], InfoBarSeverity.Informational);

                return;
            }

            int availableSlots = MaxImages - StagedScreenshots.Count;

            if(files.Count > availableSlots)
            {
                SetStatusMessage(string.Format(_localizer["SoftwareScreenshotsSelectionTrimmed"], availableSlots, MaxImages),
                                 InfoBarSeverity.Warning);
            }

            foreach(Windows.Storage.StorageFile file in files.Take(availableSlots))
                await StageFileAsync(file);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error selecting screenshots");
            SetStatusMessage(_localizer["FailedToUploadScreenshot"], InfoBarSeverity.Error);
        }
    }

    async Task StageFileAsync(Windows.Storage.StorageFile file)
    {
        string extension = Path.GetExtension(file.Name).ToLowerInvariant();

        if(!_allowedExtensions.Contains(extension))
        {
            SetStatusMessage(string.Format(_localizer["SoftwareScreenshotsUnsupportedFormat"], file.Name),
                             InfoBarSeverity.Warning);

            return;
        }

        var item = new AdminSoftwareScreenshotStagedItem
        {
            FileName       = file.Name,
            FileSizeText   = string.Empty,
            DimensionsText = string.Empty,
            Status         = AdminMachinePhotoStageStatus.Uploading,
            StatusText     = _localizer["SoftwareScreenshotsUploadingCardStatus"],
            UploadPercent  = 0
        };

        AddStagedScreenshot(item);

        try
        {
            Windows.Storage.FileProperties.BasicProperties properties = await file.GetBasicPropertiesAsync();

            if((long)properties.Size > MaxFileSizeBytes)
            {
                item.FileSizeText  = FormatBytes((long)properties.Size);
                item.Status        = AdminMachinePhotoStageStatus.Error;
                item.StatusText    = _localizer["SoftwareScreenshotsFileRejectedStatus"];
                item.ErrorText     = string.Format(_localizer["SoftwareScreenshotsFileTooLarge"], file.Name);
                item.UploadPercent = 0;

                return;
            }

            item.FileSizeText = FormatBytes((long)properties.Size);

            using Stream stream = await file.OpenStreamForReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            item.UploadPercent = 35;

            (AdminPendingSoftwareScreenshotUploadDto? result, string? error) =
                await _screenshotsService.StageAdminPendingScreenshotAsync((int)(SelectedSoftware!.Id ?? 0), fileBytes, file.Name, file.ContentType);

            if(result == null)
            {
                item.Status        = AdminMachinePhotoStageStatus.Error;
                item.StatusText    = _localizer["SoftwareScreenshotsFileRejectedStatus"];
                item.ErrorText     = string.IsNullOrWhiteSpace(error) ? _localizer["FailedToUploadScreenshot"] : error;
                item.UploadPercent = 0;

                return;
            }

            item.PendingId            = result.Id;
            item.FileSizeText         = FormatBytes(result.SizeBytes is > 0 ? result.SizeBytes.Value : (long)properties.Size);
            item.DimensionsText       = result.Width is > 0 && result.Height is > 0 ? $"{result.Width} x {result.Height}" : string.Empty;
            item.ThumbnailImageSource = await CreateImageSourceFromDataUrlAsync(result.ThumbnailBase64);
            item.Status               = AdminMachinePhotoStageStatus.Ready;
            item.StatusText           = _localizer["SoftwareScreenshotsReadyCardStatus"];
            item.UploadPercent        = 100;
            item.ErrorText            = string.Empty;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error staging screenshot {FileName}", file.Name);
            item.Status        = AdminMachinePhotoStageStatus.Error;
            item.StatusText    = _localizer["SoftwareScreenshotsFileRejectedStatus"];
            item.ErrorText     = _localizer["FailedToUploadScreenshot"];
            item.UploadPercent = 0;
        }
    }

    async Task SubmitBatchAsync()
    {
        ClearStatusMessage();

        if(!CanSubmitBatch || SelectedSoftware is null)
        {
            SetStatusMessage(_localizer["SoftwareScreenshotsBatchNotReady"], InfoBarSeverity.Warning);

            return;
        }

        List<AdminSoftwareScreenshotStagedItem> readyItems =
            StagedScreenshots.Where(s => s.IsReady && s.PendingId.HasValue).ToList();

        if(readyItems.Count == 0)
        {
            SetStatusMessage(_localizer["SoftwareScreenshotsBatchNotReady"], InfoBarSeverity.Warning);

            return;
        }

        try
        {
            IsUploading       = true;
            IsBatchCommitting = true;
            ShowBatchSummary  = false;
            ShowBatchProgress = true;
            BatchProcessed    = 0;
            BatchTotal        = readyItems.Count;
            CurrentPendingId  = null;

            foreach(AdminSoftwareScreenshotStagedItem item in readyItems)
            {
                item.Status        = AdminMachinePhotoStageStatus.Committing;
                item.StatusText    = _localizer["SoftwareScreenshotsCommittingCardStatus"];
                item.UploadPercent = 0;
                item.ErrorText     = string.Empty;
            }

            var request = new AdminSoftwareScreenshotBatchCommitRequestDto
            {
                SoftwareId         = (int)(SelectedSoftware.Id ?? 0),
                SoftwarePlatformId = SelectedPlatform is not null ? (int?)(SelectedPlatform.Id ?? 0) : null,
                SoftwareVersionId  = SelectedVersion is not null ? (int?)(SelectedVersion.Id ?? 0) : null,
                CanonicalGroupName = string.IsNullOrWhiteSpace(CanonicalGroupName) ? null : CanonicalGroupName,
                Items = readyItems.Select(item => new AdminSoftwareScreenshotBatchCommitItemDto
                {
                    PendingId = item.PendingId ?? Guid.Empty,
                    Caption   = string.IsNullOrWhiteSpace(item.Caption) ? null : item.Caption
                }).ToList()
            };

            (AdminSoftwareScreenshotBatchJobStatusDto? result, string? error) =
                await _screenshotsService.CommitAdminBatchAsync(request);

            if(result == null)
            {
                foreach(AdminSoftwareScreenshotStagedItem item in readyItems)
                {
                    item.Status        = AdminMachinePhotoStageStatus.Ready;
                    item.StatusText    = _localizer["SoftwareScreenshotsReadyCardStatus"];
                    item.ErrorText     = string.Empty;
                    item.UploadPercent = 100;
                }

                SetStatusMessage(string.IsNullOrWhiteSpace(error) ? _localizer["SoftwareScreenshotsCommitFailed"] : error,
                                 InfoBarSeverity.Error);

                ShowBatchProgress = false;

                return;
            }

            AdminSoftwareScreenshotBatchJobStatusDto? finalStatus =
                result.JobId.HasValue ? await PollBatchUntilCompleteAsync(result.JobId.Value) : null;

            if(finalStatus == null)
            {
                SetStatusMessage(_localizer["SoftwareScreenshotsPollingFailed"], InfoBarSeverity.Error);

                return;
            }

            ApplyBatchStatus(finalStatus);

            List<AdminSoftwareScreenshotBatchJobItemResultDto> finalResults = finalStatus.Results ?? [];
            int succeeded = finalResults.Count(r => r.Succeeded ?? false);
            int failed    = finalResults.Count(r => !(r.Succeeded ?? false));

            SetStatusMessage(string.Format(_localizer["SoftwareScreenshotsBatchFinished"], succeeded, failed),
                             failed == 0 ? InfoBarSeverity.Success : InfoBarSeverity.Warning);

            ShowBatchSummary = true;
            HasStatusMessage = false;

            foreach(AdminSoftwareScreenshotStagedItem item in StagedScreenshots)
                item.PendingId = null;

            if(succeeded > 0)
                await LoadAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error committing screenshot batch");
            SetStatusMessage(_localizer["SoftwareScreenshotsCommitFailed"], InfoBarSeverity.Error);
        }
        finally
        {
            IsUploading       = false;
            IsBatchCommitting = false;
            CurrentPendingId  = null;
            ShowBatchProgress = false;
            NotifyUploadStateChanged();
        }
    }

    async Task<AdminSoftwareScreenshotBatchJobStatusDto?> PollBatchUntilCompleteAsync(Guid jobId)
    {
        while(true)
        {
            AdminSoftwareScreenshotBatchJobStatusDto? status = await _screenshotsService.GetAdminBatchStatusAsync(jobId);

            if(status == null)
                return null;

            ApplyBatchStatus(status);

            BatchJobState state = (BatchJobState)(status.State ?? 0);

            if(state is BatchJobState.Completed or BatchJobState.Failed)
                return status;

            await Task.Delay(750);
        }
    }

    void ApplyBatchStatus(AdminSoftwareScreenshotBatchJobStatusDto status)
    {
        BatchProcessed   = status.Processed ?? 0;
        BatchTotal       = status.Total ?? 0;
        CurrentPendingId = status.CurrentPendingId;

        Dictionary<Guid, AdminSoftwareScreenshotBatchJobItemResultDto> resultMap = (status.Results ?? [])
                                                                  .Where(r => r.PendingId.HasValue &&
                                                                              r.PendingId.Value != Guid.Empty)
                                                                  .ToDictionary(r => r.PendingId!.Value, r => r);

        foreach(AdminSoftwareScreenshotStagedItem item in StagedScreenshots)
        {
            if(item.PendingId is not Guid pendingId)
                continue;

            if(resultMap.TryGetValue(pendingId, out AdminSoftwareScreenshotBatchJobItemResultDto? result))
            {
                if(result.Succeeded ?? false)
                {
                    item.Status        = AdminMachinePhotoStageStatus.Succeeded;
                    item.StatusText    = _localizer["SoftwareScreenshotsSucceededCardStatus"];
                    item.ErrorText     = string.Empty;
                    item.UploadPercent = 100;
                }
                else
                {
                    item.Status        = AdminMachinePhotoStageStatus.Failed;
                    item.StatusText    = _localizer["SoftwareScreenshotsFailedCardStatus"];
                    item.ErrorText     = string.IsNullOrWhiteSpace(result.Error) ? _localizer["SoftwareScreenshotsCommitFailed"] : result.Error;
                    item.UploadPercent = 100;
                }
            }
            else if(status.CurrentPendingId.HasValue && status.CurrentPendingId.Value == pendingId)
            {
                item.Status        = AdminMachinePhotoStageStatus.Committing;
                item.StatusText    = _localizer["SoftwareScreenshotsProcessingCardStatus"];
                item.UploadPercent = 50;
                item.ErrorText     = string.Empty;
            }
            else if(item.Status == AdminMachinePhotoStageStatus.Committing)
            {
                item.StatusText = _localizer["SoftwareScreenshotsQueuedCardStatus"];
            }
        }
    }

    async Task RemoveStagedScreenshotAsync(AdminSoftwareScreenshotStagedItem? item)
    {
        if(item == null || item.Status == AdminMachinePhotoStageStatus.Committing)
            return;

        try
        {
            if(item.PendingId.HasValue && !item.IsTerminal)
            {
                bool deleted = await _screenshotsService.DeleteAdminPendingScreenshotAsync(item.PendingId.Value);

                if(!deleted)
                {
                    SetStatusMessage(_localizer["SoftwareScreenshotsFailedToRemoveStaged"], InfoBarSeverity.Warning);

                    return;
                }
            }

            RemoveStagedScreenshot(item);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing staged screenshot {ClientId}", item.ClientId);
            SetStatusMessage(_localizer["SoftwareScreenshotsFailedToRemoveStaged"], InfoBarSeverity.Warning);
        }
    }

    async Task ClearStagedScreenshotsAsync()
    {
        List<AdminSoftwareScreenshotStagedItem> items = StagedScreenshots.ToList();

        foreach(AdminSoftwareScreenshotStagedItem item in items)
        {
            if(item.PendingId.HasValue && !item.IsTerminal)
            {
                try
                {
                    await _screenshotsService.DeleteAdminPendingScreenshotAsync(item.PendingId.Value);
                }
                catch(Exception ex)
                {
                    _logger.LogDebug(ex, "Best-effort cleanup failed for staged screenshot {PendingId}",
                                     item.PendingId.Value);
                }
            }

            RemoveStagedScreenshot(item);
        }

        ShowBatchProgress = false;
        ShowBatchSummary  = false;
        BatchProcessed    = 0;
        BatchTotal        = 0;
        CurrentPendingId  = null;
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

        SelectedPlatform = _allPlatforms?.FirstOrDefault(p => p.Id == dto.SoftwarePlatformId);
        SelectedVersion  = _allVersions?.FirstOrDefault(v => v.Id == dto.SoftwareVersionId);

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

    void GoBack() => _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoftwarePage));

    async Task<Microsoft.UI.Xaml.Media.ImageSource?> CreateImageSourceFromDataUrlAsync(string? dataUrl)
    {
        if(string.IsNullOrWhiteSpace(dataUrl))
            return null;

        int commaIndex = dataUrl.IndexOf(',');

        if(commaIndex < 0 || commaIndex == dataUrl.Length - 1)
            return null;

        byte[] bytes = Convert.FromBase64String(dataUrl[(commaIndex + 1)..]);
        await using var stream = new MemoryStream(bytes);

        return await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
    }

    void AddStagedScreenshot(AdminSoftwareScreenshotStagedItem item)
    {
        item.PropertyChanged += OnStagedScreenshotPropertyChanged;
        StagedScreenshots.Add(item);
        NotifyUploadStateChanged();
    }

    void RemoveStagedScreenshot(AdminSoftwareScreenshotStagedItem item)
    {
        item.PropertyChanged -= OnStagedScreenshotPropertyChanged;
        StagedScreenshots.Remove(item);
        NotifyUploadStateChanged();
    }

    void OnStagedScreenshotPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(AdminSoftwareScreenshotStagedItem.Status) or nameof(AdminSoftwareScreenshotStagedItem.PendingId))
            NotifyUploadStateChanged();
    }

    void NotifyUploadStateChanged()
    {
        OnPropertyChanged(nameof(HasStagedScreenshots));
        OnPropertyChanged(nameof(CanAddMoreScreenshots));
        OnPropertyChanged(nameof(CanSubmitBatch));
        OnPropertyChanged(nameof(CanClearStaged));
    }

    void NotifyBatchProgressChanged()
    {
        OnPropertyChanged(nameof(BatchProgressPercent));
        OnPropertyChanged(nameof(BatchProgressText));
        OnPropertyChanged(nameof(HasBatchProgressText));
    }

    void SetStatusMessage(string message, InfoBarSeverity severity)
    {
        StatusMessage    = message;
        StatusSeverity   = severity;
        HasStatusMessage = !string.IsNullOrWhiteSpace(message);
    }

    void ClearStatusMessage()
    {
        StatusMessage    = string.Empty;
        HasStatusMessage = false;
    }

    static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double size = bytes;
        int unit = 0;

        while(size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return unit == 0 ? $"{size:0} {units[unit]}" : $"{size:0.0} {units[unit]}";
    }
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
