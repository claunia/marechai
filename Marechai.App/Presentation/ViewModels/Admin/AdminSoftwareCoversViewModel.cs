#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BatchJobState = Marechai.Data.Dtos.BatchJobState;
using AdminBatchCommitItemDto = Marechai.ApiClient.Models.AdminBatchCommitItemDto;
using AdminBatchCommitRequestDto = Marechai.ApiClient.Models.AdminBatchCommitRequestDto;
using AdminBatchJobItemResultDto = Marechai.ApiClient.Models.AdminBatchJobItemResultDto;
using AdminBatchJobStatusDto = Marechai.ApiClient.Models.AdminBatchJobStatusDto;
using AdminPendingCoverUploadDto = Marechai.ApiClient.Models.AdminPendingCoverUploadDto;
using SoftwareCoverDto = Marechai.ApiClient.Models.SoftwareCoverDto;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.App.Services.Caching;
using Marechai.Data;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareCoversViewModel : ObservableObject, IRegionAware
{
    const int  MaxImages        = 25;
    const long MaxFileSizeBytes = 50 * 1024 * 1024;

    static readonly HashSet<string> _allowedExtensions =
    [
        ".jpg", ".jpeg", ".png", ".webp", ".avif", ".bmp", ".tif", ".tiff"
    ];

    readonly SoftwareCoversService                    _coversService;
    readonly SoftwareCoverCache                       _coverCache;
    readonly ImageSourceFactory                       _imageSourceFactory;
    readonly IJwtService                               _jwtService;
    readonly ITokenService                             _tokenService;
    readonly IStringLocalizer                          _localizer;
    readonly ILogger<AdminSoftwareCoversViewModel>     _logger;
    readonly IRegionManager                            _regionManager;

    int _releaseId;

    [ObservableProperty]
    private string _releaseTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CoverGridItem> _covers = [];

    [ObservableProperty]
    private ObservableCollection<AdminSoftwareCoverStagedItem> _stagedCovers = [];

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
    private bool _isBatchCommitting;

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
    private bool _isEditing;

    [ObservableProperty]
    private CoverGridItem? _selectedCover;

    [ObservableProperty]
    private SoftwareCoverType _editType = SoftwareCoverType.Front;

    [ObservableProperty]
    private string _editCaption = string.Empty;

    public AdminSoftwareCoversViewModel(SoftwareCoversService                  coversService,
                                        SoftwareCoverCache                      coverCache,
                                        ImageSourceFactory                      imageSourceFactory,
                                        IJwtService                             jwtService,
                                        ITokenService                           tokenService,
                                        IStringLocalizer                        localizer,
                                        ILogger<AdminSoftwareCoversViewModel>   logger,
                                        IRegionManager                          regionManager)
    {
        _coversService      = coversService;
        _coverCache         = coverCache;
        _imageSourceFactory = imageSourceFactory;
        _jwtService         = jwtService;
        _tokenService       = tokenService;
        _localizer          = localizer;
        _logger             = logger;
        _regionManager      = regionManager;

        LoadCoversCommand        = new AsyncRelayCommand(LoadCoversAsync);
        AddCoversCommand         = new AsyncRelayCommand(AddCoversAsync);
        SubmitBatchCommand       = new AsyncRelayCommand(SubmitBatchAsync);
        ClearStagedCoversCommand = new AsyncRelayCommand(ClearStagedCoversAsync);
        RemoveStagedCoverCommand = new AsyncRelayCommand<AdminSoftwareCoverStagedItem>(RemoveStagedCoverAsync);
        DeleteCoverCommand       = new AsyncRelayCommand<CoverGridItem>(DeleteCoverAsync);
        OpenEditCommand          = new RelayCommand<CoverGridItem>(OpenEdit);
        SaveEditCommand          = new AsyncRelayCommand(SaveEditAsync);
        CancelEditCommand        = new RelayCommand(CancelEdit);
        GoBackCommand            = new RelayCommand(GoBack);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                              LoadCoversCommand        { get; }
    public IAsyncRelayCommand                              AddCoversCommand         { get; }
    public IAsyncRelayCommand                              SubmitBatchCommand       { get; }
    public IAsyncRelayCommand                              ClearStagedCoversCommand { get; }
    public IAsyncRelayCommand<AdminSoftwareCoverStagedItem> RemoveStagedCoverCommand { get; }
    public IAsyncRelayCommand<CoverGridItem>                DeleteCoverCommand       { get; }
    public IRelayCommand<CoverGridItem>                     OpenEditCommand          { get; }
    public IAsyncRelayCommand                               SaveEditCommand          { get; }
    public IRelayCommand                                    CancelEditCommand        { get; }
    public IRelayCommand                                    GoBackCommand            { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        _ = ClearStagedCoversAsync();
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        int previousReleaseId = _releaseId;

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareReleaseId, out int releaseId))
            _releaseId = releaseId;

        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.SoftwareReleaseTitle, out string? title))
            ReleaseTitle = title ?? string.Empty;

        if(previousReleaseId != _releaseId)
            _ = ClearStagedCoversAsync();

        if(IsAdmin)
            _ = LoadCoversCommand.ExecuteAsync(null);
    }

    public bool HasStagedCovers => StagedCovers.Count > 0;
    public bool CanAddMoreCovers => !IsUploading && StagedCovers.Count < MaxImages;
    public bool CanSubmitBatch => StagedCovers.Count > 0 &&
                                  !IsUploading &&
                                  !IsBatchCommitting &&
                                  StagedCovers.All(s => s.IsReady);
    public bool CanClearStaged => HasStagedCovers && !IsBatchCommitting && !IsUploading;
    public bool HasBatchProgressText => ShowBatchProgress && BatchTotal > 0;
    public double BatchProgressPercent => BatchTotal <= 0 ? 0 : Math.Min(100, BatchProcessed * 100.0 / BatchTotal);
    public string BatchProgressText => string.Format(_localizer["SoftwareCoversBatchProgressText"], BatchProcessed, BatchTotal);

    public static IReadOnlyList<SoftwareCoverType> CoverTypes { get; } =
        Enum.GetValues<SoftwareCoverType>();

    partial void OnIsUploadingChanged(bool value) => NotifyUploadStateChanged();
    partial void OnIsBatchCommittingChanged(bool value) => NotifyUploadStateChanged();
    partial void OnBatchProcessedChanged(int value) => NotifyBatchProgressChanged();
    partial void OnBatchTotalChanged(int value) => NotifyBatchProgressChanged();
    partial void OnShowBatchProgressChanged(bool value) => OnPropertyChanged(nameof(HasBatchProgressText));

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
                      roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    async Task LoadCoversAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Covers.Clear();

            List<Guid> coverIds = await _coversService.GetCoverIdsByReleaseAsync(_releaseId);

            foreach(Guid coverId in coverIds)
            {
                var item = new CoverGridItem
                {
                    Id = coverId
                };

                SoftwareCoverDto? details = await _coversService.GetCoverDetailsAsync(coverId);

                if(details != null)
                {
                    item.TypeName     = details.TypeName ?? string.Empty;
                    item.Caption      = details.Caption ?? string.Empty;
                    item.RegionNames  = details.RegionNames ?? string.Empty;
                    item.Type         = (SoftwareCoverType)details.Type;
                }

                Covers.Add(item);
                _ = LoadThumbnailAsync(item);
            }

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading covers for release {ReleaseId}", _releaseId);
            ErrorMessage = _localizer["FailedToLoadCovers"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task LoadThumbnailAsync(CoverGridItem item)
    {
        try
        {
            Stream stream = await _coverCache.GetThumbnailAsync(item.Id);
            item.ThumbnailSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading cover thumbnail {Id}", item.Id);
        }
    }

    async Task AddCoversAsync()
    {
        try
        {
            ClearStatusMessage();

            if(StagedCovers.Count >= MaxImages)
            {
                SetStatusMessage(string.Format(_localizer["SoftwareCoversMaxImagesReached"], MaxImages),
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
                SetStatusMessage(_localizer["SoftwareCoversPickerCanceled"], InfoBarSeverity.Informational);

                return;
            }

            int availableSlots = MaxImages - StagedCovers.Count;

            if(files.Count > availableSlots)
            {
                SetStatusMessage(string.Format(_localizer["SoftwareCoversSelectionTrimmed"], availableSlots, MaxImages),
                                 InfoBarSeverity.Warning);
            }

            foreach(Windows.Storage.StorageFile file in files.Take(availableSlots))
                await StageFileAsync(file);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error selecting covers");
            SetStatusMessage(_localizer["FailedToUploadCover"], InfoBarSeverity.Error);
        }
    }

    async Task StageFileAsync(Windows.Storage.StorageFile file)
    {
        string extension = Path.GetExtension(file.Name).ToLowerInvariant();

        if(!_allowedExtensions.Contains(extension))
        {
            SetStatusMessage(string.Format(_localizer["SoftwareCoversUnsupportedFormat"], file.Name),
                             InfoBarSeverity.Warning);

            return;
        }

        var item = new AdminSoftwareCoverStagedItem
        {
            FileName       = file.Name,
            FileSizeText   = string.Empty,
            DimensionsText = string.Empty,
            Status         = AdminMachinePhotoStageStatus.Uploading,
            StatusText     = _localizer["SoftwareCoversUploadingCardStatus"],
            UploadPercent  = 0
        };

        AddStagedCover(item);

        try
        {
            Windows.Storage.FileProperties.BasicProperties properties = await file.GetBasicPropertiesAsync();

            if((long)properties.Size > MaxFileSizeBytes)
            {
                item.FileSizeText  = FormatBytes((long)properties.Size);
                item.Status        = AdminMachinePhotoStageStatus.Error;
                item.StatusText    = _localizer["SoftwareCoversFileRejectedStatus"];
                item.ErrorText     = string.Format(_localizer["SoftwareCoversFileTooLarge"], file.Name);
                item.UploadPercent = 0;

                return;
            }

            item.FileSizeText = FormatBytes((long)properties.Size);

            using Stream stream = await file.OpenStreamForReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            item.UploadPercent = 35;

            (AdminPendingCoverUploadDto? result, string? error) =
                await _coversService.StageAdminPendingCoverAsync(_releaseId, fileBytes, file.Name, file.ContentType);

            if(result == null)
            {
                item.Status        = AdminMachinePhotoStageStatus.Error;
                item.StatusText    = _localizer["SoftwareCoversFileRejectedStatus"];
                item.ErrorText     = string.IsNullOrWhiteSpace(error) ? _localizer["FailedToUploadCover"] : error;
                item.UploadPercent = 0;

                return;
            }

            item.PendingId            = result.Id;
            item.FileSizeText         = FormatBytes(result.SizeBytes is > 0 ? result.SizeBytes.Value : (long)properties.Size);
            item.DimensionsText       = result.Width is > 0 && result.Height is > 0 ? $"{result.Width} x {result.Height}" : string.Empty;
            item.ThumbnailImageSource = await CreateImageSourceFromDataUrlAsync(result.ThumbnailBase64);
            item.Status               = AdminMachinePhotoStageStatus.Ready;
            item.StatusText           = _localizer["SoftwareCoversReadyCardStatus"];
            item.UploadPercent        = 100;
            item.ErrorText            = string.Empty;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error staging cover {FileName}", file.Name);
            item.Status        = AdminMachinePhotoStageStatus.Error;
            item.StatusText    = _localizer["SoftwareCoversFileRejectedStatus"];
            item.ErrorText     = _localizer["FailedToUploadCover"];
            item.UploadPercent = 0;
        }
    }

    async Task SubmitBatchAsync()
    {
        ClearStatusMessage();

        if(!CanSubmitBatch)
        {
            SetStatusMessage(_localizer["SoftwareCoversBatchNotReady"], InfoBarSeverity.Warning);

            return;
        }

        List<AdminSoftwareCoverStagedItem> readyItems = StagedCovers.Where(s => s.IsReady && s.PendingId.HasValue).ToList();

        if(readyItems.Count == 0)
        {
            SetStatusMessage(_localizer["SoftwareCoversBatchNotReady"], InfoBarSeverity.Warning);

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

            foreach(AdminSoftwareCoverStagedItem item in readyItems)
            {
                item.Status        = AdminMachinePhotoStageStatus.Committing;
                item.StatusText    = _localizer["SoftwareCoversCommittingCardStatus"];
                item.UploadPercent = 0;
                item.ErrorText     = string.Empty;
            }

            var request = new AdminBatchCommitRequestDto
            {
                SoftwareReleaseId = _releaseId,
                Items = readyItems.Select(item => new AdminBatchCommitItemDto
                {
                    PendingId = item.PendingId ?? Guid.Empty,
                    Type      = (int)item.SelectedType,
                    Caption   = string.IsNullOrWhiteSpace(item.Caption) ? null : item.Caption
                }).ToList()
            };

            (AdminBatchJobStatusDto? result, string? error) = await _coversService.CommitAdminBatchAsync(request);

            if(result == null)
            {
                foreach(AdminSoftwareCoverStagedItem item in readyItems)
                {
                    item.Status        = AdminMachinePhotoStageStatus.Ready;
                    item.StatusText    = _localizer["SoftwareCoversReadyCardStatus"];
                    item.ErrorText     = string.Empty;
                    item.UploadPercent = 100;
                }

                SetStatusMessage(string.IsNullOrWhiteSpace(error) ? _localizer["SoftwareCoversCommitFailed"] : error,
                                 InfoBarSeverity.Error);

                ShowBatchProgress = false;

                return;
            }

            AdminBatchJobStatusDto? finalStatus =
                result.JobId.HasValue ? await PollBatchUntilCompleteAsync(result.JobId.Value) : null;

            if(finalStatus == null)
            {
                SetStatusMessage(_localizer["SoftwareCoversPollingFailed"], InfoBarSeverity.Error);

                return;
            }

            ApplyBatchStatus(finalStatus);

            List<AdminBatchJobItemResultDto> finalResults = finalStatus.Results ?? [];
            int succeeded = finalResults.Count(r => r.Succeeded ?? false);
            int failed    = finalResults.Count(r => !(r.Succeeded ?? false));

            SetStatusMessage(string.Format(_localizer["SoftwareCoversBatchFinished"], succeeded, failed),
                             failed == 0 ? InfoBarSeverity.Success : InfoBarSeverity.Warning);

            ShowBatchSummary = true;
            HasStatusMessage = false;

            foreach(AdminSoftwareCoverStagedItem item in StagedCovers)
                item.PendingId = null;

            if(succeeded > 0)
                await LoadCoversAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error committing cover batch");
            SetStatusMessage(_localizer["SoftwareCoversCommitFailed"], InfoBarSeverity.Error);
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

    async Task<AdminBatchJobStatusDto?> PollBatchUntilCompleteAsync(Guid jobId)
    {
        while(true)
        {
            AdminBatchJobStatusDto? status = await _coversService.GetAdminBatchStatusAsync(jobId);

            if(status == null)
                return null;

            ApplyBatchStatus(status);

            BatchJobState state = (BatchJobState)(status.State ?? 0);

            if(state is BatchJobState.Completed or BatchJobState.Failed)
                return status;

            await Task.Delay(750);
        }
    }

    void ApplyBatchStatus(AdminBatchJobStatusDto status)
    {
        BatchProcessed   = status.Processed ?? 0;
        BatchTotal       = status.Total ?? 0;
        CurrentPendingId = status.CurrentPendingId;

        Dictionary<Guid, AdminBatchJobItemResultDto> resultMap = (status.Results ?? [])
                                                                  .Where(r => r.PendingId.HasValue &&
                                                                              r.PendingId.Value != Guid.Empty)
                                                                  .ToDictionary(r => r.PendingId!.Value, r => r);

        foreach(AdminSoftwareCoverStagedItem item in StagedCovers)
        {
            if(item.PendingId is not Guid pendingId)
                continue;

            if(resultMap.TryGetValue(pendingId, out AdminBatchJobItemResultDto? result))
            {
                if(result.Succeeded ?? false)
                {
                    item.Status        = AdminMachinePhotoStageStatus.Succeeded;
                    item.StatusText    = _localizer["SoftwareCoversSucceededCardStatus"];
                    item.ErrorText     = string.Empty;
                    item.UploadPercent = 100;
                }
                else
                {
                    item.Status        = AdminMachinePhotoStageStatus.Failed;
                    item.StatusText    = _localizer["SoftwareCoversFailedCardStatus"];
                    item.ErrorText     = string.IsNullOrWhiteSpace(result.Error) ? _localizer["SoftwareCoversCommitFailed"] : result.Error;
                    item.UploadPercent = 100;
                }
            }
            else if(status.CurrentPendingId.HasValue && status.CurrentPendingId.Value == pendingId)
            {
                item.Status        = AdminMachinePhotoStageStatus.Committing;
                item.StatusText    = _localizer["SoftwareCoversProcessingCardStatus"];
                item.UploadPercent = 50;
                item.ErrorText     = string.Empty;
            }
            else if(item.Status == AdminMachinePhotoStageStatus.Committing)
            {
                item.StatusText = _localizer["SoftwareCoversQueuedCardStatus"];
            }
        }
    }

    async Task RemoveStagedCoverAsync(AdminSoftwareCoverStagedItem? item)
    {
        if(item == null || item.Status == AdminMachinePhotoStageStatus.Committing)
            return;

        try
        {
            if(item.PendingId.HasValue && !item.IsTerminal)
            {
                bool deleted = await _coversService.DeleteAdminPendingCoverAsync(item.PendingId.Value);

                if(!deleted)
                {
                    SetStatusMessage(_localizer["SoftwareCoversFailedToRemoveStaged"], InfoBarSeverity.Warning);

                    return;
                }
            }

            RemoveStagedCover(item);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing staged cover {ClientId}", item.ClientId);
            SetStatusMessage(_localizer["SoftwareCoversFailedToRemoveStaged"], InfoBarSeverity.Warning);
        }
    }

    async Task ClearStagedCoversAsync()
    {
        List<AdminSoftwareCoverStagedItem> items = StagedCovers.ToList();

        foreach(AdminSoftwareCoverStagedItem item in items)
        {
            if(item.PendingId.HasValue && !item.IsTerminal)
            {
                try
                {
                    await _coversService.DeleteAdminPendingCoverAsync(item.PendingId.Value);
                }
                catch(Exception ex)
                {
                    _logger.LogDebug(ex, "Best-effort cleanup failed for staged cover {PendingId}",
                                     item.PendingId.Value);
                }
            }

            RemoveStagedCover(item);
        }

        ShowBatchProgress = false;
        ShowBatchSummary  = false;
        BatchProcessed    = 0;
        BatchTotal        = 0;
        CurrentPendingId  = null;
    }

    async Task DeleteCoverAsync(CoverGridItem? item)
    {
        if(item == null) return;

        try
        {
            ClearStatusMessage();

            bool success = await _coversService.DeleteCoverAsync(item.Id);

            if(!success)
            {
                SetStatusMessage(_localizer["FailedToDeleteCover"], InfoBarSeverity.Error);

                return;
            }

            await _coverCache.InvalidateCacheAsync(item.Id);
            await LoadCoversAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting cover {Id}", item.Id);
            SetStatusMessage(_localizer["FailedToDeleteCover"], InfoBarSeverity.Error);
        }
    }

    void OpenEdit(CoverGridItem? item)
    {
        if(item is null) return;

        SelectedCover = item;
        EditType      = item.Type;
        EditCaption   = item.Caption;
        IsEditing     = true;
    }

    async Task SaveEditAsync()
    {
        if(SelectedCover is null) return;

        try
        {
            ClearStatusMessage();

            var dto = new SoftwareCoverDto
            {
                Id                = SelectedCover.Id,
                Type              = (int)EditType,
                Caption           = string.IsNullOrWhiteSpace(EditCaption) ? null : EditCaption,
                OriginalExtension = string.Empty // not updated
            };

            bool success = await _coversService.UpdateCoverAsync(SelectedCover.Id, dto);

            if(!success)
            {
                SetStatusMessage(_localizer["FailedToSaveCover"], InfoBarSeverity.Error);

                return;
            }

            IsEditing = false;
            await LoadCoversAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving cover {Id}", SelectedCover.Id);
            SetStatusMessage(_localizer["FailedToSaveCover"], InfoBarSeverity.Error);
        }
    }

    void CancelEdit() => IsEditing = false;

    void GoBack() => _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoftwareReleasesPage));

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

    void AddStagedCover(AdminSoftwareCoverStagedItem item)
    {
        item.PropertyChanged += OnStagedCoverPropertyChanged;
        StagedCovers.Add(item);
        NotifyUploadStateChanged();
    }

    void RemoveStagedCover(AdminSoftwareCoverStagedItem item)
    {
        item.PropertyChanged -= OnStagedCoverPropertyChanged;
        StagedCovers.Remove(item);
        NotifyUploadStateChanged();
    }

    void OnStagedCoverPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(AdminSoftwareCoverStagedItem.Status) or nameof(AdminSoftwareCoverStagedItem.PendingId))
            NotifyUploadStateChanged();
    }

    void NotifyUploadStateChanged()
    {
        OnPropertyChanged(nameof(HasStagedCovers));
        OnPropertyChanged(nameof(CanAddMoreCovers));
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

public partial class CoverGridItem : ObservableObject
{
    public Guid             Id           { get; set; }
    public string           TypeName     { get; set; } = string.Empty;
    public string           Caption      { get; set; } = string.Empty;
    public string           RegionNames  { get; set; } = string.Empty;
    public SoftwareCoverType Type        { get; set; }

    [ObservableProperty]
    private Microsoft.UI.Xaml.Media.ImageSource? _thumbnailSource;
}
