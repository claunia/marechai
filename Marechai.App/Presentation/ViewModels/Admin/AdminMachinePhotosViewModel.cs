#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BatchJobState = Marechai.Data.Dtos.BatchJobState;
using AdminMachinePhotoBatchCommitItemDto = Marechai.ApiClient.Models.AdminMachinePhotoBatchCommitItemDto;
using AdminMachinePhotoBatchCommitRequestDto = Marechai.ApiClient.Models.AdminMachinePhotoBatchCommitRequestDto;
using AdminMachinePhotoBatchJobItemResultDto = Marechai.ApiClient.Models.AdminMachinePhotoBatchJobItemResultDto;
using AdminMachinePhotoBatchJobStatusDto = Marechai.ApiClient.Models.AdminMachinePhotoBatchJobStatusDto;
using AdminPendingMachinePhotoUploadDto = Marechai.ApiClient.Models.AdminPendingMachinePhotoUploadDto;
using LicenseDto = Marechai.ApiClient.Models.LicenseDto;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Dialogs;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminMachinePhotosViewModel : ObservableObject, IRegionAware
{
    const int  MaxImages        = 25;
    const long MaxFileSizeBytes = 50 * 1024 * 1024;

    static readonly HashSet<string> _allowedExtensions =
    [
        ".jpg", ".jpeg", ".png", ".webp", ".avif", ".bmp", ".tif", ".tiff"
    ];

    readonly MachinePhotosService                 _photosService;
    readonly LicensesService                      _licensesService;
    readonly MachinePhotoCache                    _photoCache;
    readonly ImageSourceFactory                   _imageSourceFactory;
    readonly IJwtService                          _jwtService;
    readonly ITokenService                        _tokenService;
    readonly IStringLocalizer                     _localizer;
    readonly ILogger<AdminMachinePhotosViewModel> _logger;
    readonly IRegionManager                       _regionManager;

    int _machineId;

    [ObservableProperty]
    private string _machineName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MachinePhotoDisplayItem> _photos = [];

    [ObservableProperty]
    private ObservableCollection<AdminMachinePhotoStagedItem> _stagedPhotos = [];

    [ObservableProperty]
    private ObservableCollection<LicenseDto> _licenses = [];

    [ObservableProperty]
    private LicenseDto? _selectedLicense;

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

    public AdminMachinePhotosViewModel(MachinePhotosService                 photosService,
                                       LicensesService                      licensesService,
                                       MachinePhotoCache                    photoCache,
                                       ImageSourceFactory                   imageSourceFactory,
                                       IJwtService                          jwtService,
                                       ITokenService                        tokenService,
                                       IStringLocalizer                     localizer,
                                       ILogger<AdminMachinePhotosViewModel> logger,
                                       IRegionManager                       regionManager)
    {
        _photosService      = photosService;
        _licensesService    = licensesService;
        _photoCache         = photoCache;
        _imageSourceFactory = imageSourceFactory;
        _jwtService         = jwtService;
        _tokenService       = tokenService;
        _localizer          = localizer;
        _logger             = logger;
        _regionManager      = regionManager;

        LoadPhotosCommand        = new AsyncRelayCommand(LoadPhotosAsync);
        AddPhotosCommand         = new AsyncRelayCommand(AddPhotosAsync);
        SubmitBatchCommand       = new AsyncRelayCommand(SubmitBatchAsync);
        ClearStagedPhotosCommand = new AsyncRelayCommand(ClearStagedPhotosAsync);
        RemoveStagedPhotoCommand = new AsyncRelayCommand<AdminMachinePhotoStagedItem>(RemoveStagedPhotoAsync);
        DeletePhotoCommand       = new AsyncRelayCommand<MachinePhotoDisplayItem>(DeletePhotoAsync);
        ViewPhotoCommand         = new RelayCommand<MachinePhotoDisplayItem>(ViewPhoto);
        GoBackCommand            = new RelayCommand(GoBack);

        CheckAdminRole();
    }

    public IAsyncRelayCommand LoadPhotosCommand { get; }
    public IAsyncRelayCommand AddPhotosCommand { get; }
    public IAsyncRelayCommand SubmitBatchCommand { get; }
    public IAsyncRelayCommand ClearStagedPhotosCommand { get; }
    public IAsyncRelayCommand<AdminMachinePhotoStagedItem> RemoveStagedPhotoCommand { get; }
    public IAsyncRelayCommand<MachinePhotoDisplayItem> DeletePhotoCommand { get; }
    public IRelayCommand<MachinePhotoDisplayItem> ViewPhotoCommand { get; }
    public IRelayCommand GoBackCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        _ = ClearStagedPhotosAsync();
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        int previousMachineId = _machineId;

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.MachineId, out int machineId))
            _machineId = machineId;

        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.MachineName, out string? name))
            MachineName = name ?? string.Empty;

        if(previousMachineId != _machineId)
            _ = ClearStagedPhotosAsync();

        if(IsAdmin)
        {
            _ = LoadLicensesAsync();
            _ = LoadPhotosCommand.ExecuteAsync(null);
        }
    }

    public bool HasStagedPhotos   => StagedPhotos.Count > 0;
    public bool CanAddMorePhotos  => !IsUploading && StagedPhotos.Count < MaxImages;
    public bool CanSubmitBatch    => SelectedLicense is not null &&
                                     StagedPhotos.Count > 0 &&
                                     !IsUploading &&
                                     !IsBatchCommitting &&
                                     StagedPhotos.All(s => s.IsReady);
    public bool CanClearStaged    => HasStagedPhotos && !IsBatchCommitting && !IsUploading;
    public bool HasBatchProgressText => ShowBatchProgress && BatchTotal > 0;
    public double BatchProgressPercent => BatchTotal <= 0 ? 0 : Math.Min(100, BatchProcessed * 100.0 / BatchTotal);
    public string BatchProgressText => string.Format(_localizer["MachinePhotosBatchProgressText"], BatchProcessed, BatchTotal);

    partial void OnSelectedLicenseChanged(LicenseDto? value) => NotifyUploadStateChanged();
    partial void OnIsUploadingChanged(bool value)            => NotifyUploadStateChanged();
    partial void OnIsBatchCommittingChanged(bool value)      => NotifyUploadStateChanged();
    partial void OnBatchProcessedChanged(int value)          => NotifyBatchProgressChanged();
    partial void OnBatchTotalChanged(int value)              => NotifyBatchProgressChanged();
    partial void OnShowBatchProgressChanged(bool value)      => OnPropertyChanged(nameof(HasBatchProgressText));

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

            if(!_jwtService.IsTokenValid(token)) { IsAdmin = false; return; }
            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    async Task LoadLicensesAsync()
    {
        try
        {
            List<LicenseDto> licenseList = await _licensesService.GetLicensesAsync();

            Licenses.Clear();

            foreach(LicenseDto license in licenseList.OrderBy(l => l.Name))
                Licenses.Add(license);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading licenses");
        }
    }

    async Task LoadPhotosAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Photos.Clear();

            List<Guid> photoIds = await _photosService.GetPhotoIdsAsync(_machineId);

            foreach(Guid photoId in photoIds)
            {
                var item = new MachinePhotoDisplayItem
                {
                    PhotoId = photoId
                };

                MachinePhotoDto? details = await _photosService.GetPhotoDetailsAsync(photoId);

                if(details != null)
                {
                    item.LicenseName = details.LicenseName;
                    item.CameraInfo = !string.IsNullOrEmpty(details.CameraManufacturer) ||
                                      !string.IsNullOrEmpty(details.CameraModel)
                                          ? $"{details.CameraManufacturer} {details.CameraModel}".Trim()
                                          : null;
                    item.UploadDate = details.UploadDate?.ToString("d");
                }

                Photos.Add(item);
                _ = LoadThumbnailAsync(item);
            }

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading photos for machine {MachineId}", _machineId);
            ErrorMessage = _localizer["FailedToLoadPhotos"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task LoadThumbnailAsync(MachinePhotoDisplayItem item)
    {
        try
        {
            Stream stream = await _photoCache.GetThumbnailAsync(item.PhotoId);
            item.ThumbnailImageSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading photo thumbnail {PhotoId}", item.PhotoId);
        }
    }

    async Task AddPhotosAsync()
    {
        try
        {
            ClearStatusMessage();

            if(StagedPhotos.Count >= MaxImages)
            {
                SetStatusMessage(string.Format(_localizer["MachinePhotosMaxImagesReached"], MaxImages),
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
                SetStatusMessage(_localizer["MachinePhotosPickerCanceled"], InfoBarSeverity.Informational);

                return;
            }

            int availableSlots = MaxImages - StagedPhotos.Count;

            if(files.Count > availableSlots)
            {
                SetStatusMessage(string.Format(_localizer["MachinePhotosSelectionTrimmed"], availableSlots, MaxImages),
                                 InfoBarSeverity.Warning);
            }

            foreach(Windows.Storage.StorageFile file in files.Take(availableSlots))
                await StageFileAsync(file);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error selecting machine photos");
            SetStatusMessage(_localizer["FailedToUploadPhoto"], InfoBarSeverity.Error);
        }
    }

    async Task StageFileAsync(Windows.Storage.StorageFile file)
    {
        string extension = Path.GetExtension(file.Name).ToLowerInvariant();

        if(!_allowedExtensions.Contains(extension))
        {
            SetStatusMessage(string.Format(_localizer["MachinePhotosUnsupportedFormat"], file.Name),
                             InfoBarSeverity.Warning);

            return;
        }

        var item = new AdminMachinePhotoStagedItem
        {
            FileName     = file.Name,
            FileSizeText = string.Empty,
            DimensionsText = string.Empty,
            Status       = AdminMachinePhotoStageStatus.Uploading,
            StatusText   = _localizer["MachinePhotosUploadingCardStatus"],
            UploadPercent = 0
        };

        AddStagedPhoto(item);

        try
        {
            Windows.Storage.FileProperties.BasicProperties properties = await file.GetBasicPropertiesAsync();

            if((long)properties.Size > MaxFileSizeBytes)
            {
                item.FileSizeText  = FormatBytes((long)properties.Size);
                item.Status        = AdminMachinePhotoStageStatus.Error;
                item.StatusText    = _localizer["MachinePhotosFileRejectedStatus"];
                item.ErrorText     = string.Format(_localizer["MachinePhotosFileTooLarge"], file.Name);
                item.UploadPercent = 0;

                return;
            }

            item.FileSizeText = FormatBytes((long)properties.Size);

            using Stream stream = await file.OpenStreamForReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            item.UploadPercent = 35;

            (AdminPendingMachinePhotoUploadDto? result, string? error) =
                await _photosService.StageAdminPendingPhotoAsync(_machineId, fileBytes, file.Name, file.ContentType);

            if(result == null)
            {
                item.Status        = AdminMachinePhotoStageStatus.Error;
                item.StatusText    = _localizer["MachinePhotosFileRejectedStatus"];
                item.ErrorText     = string.IsNullOrWhiteSpace(error) ? _localizer["FailedToUploadPhoto"] : error;
                item.UploadPercent = 0;

                return;
            }

            item.PendingId       = result.Id;
            item.FileSizeText    = FormatBytes(result.SizeBytes is > 0 ? result.SizeBytes.Value : (long)properties.Size);
            item.DimensionsText  = result.Width is > 0 && result.Height is > 0 ? $"{result.Width} x {result.Height}" : string.Empty;
            item.ThumbnailImageSource = await CreateImageSourceFromDataUrlAsync(result.ThumbnailBase64);
            item.Status          = AdminMachinePhotoStageStatus.Ready;
            item.StatusText      = _localizer["MachinePhotosReadyCardStatus"];
            item.UploadPercent   = 100;
            item.ErrorText       = string.Empty;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error staging machine photo {FileName}", file.Name);
            item.Status        = AdminMachinePhotoStageStatus.Error;
            item.StatusText    = _localizer["MachinePhotosFileRejectedStatus"];
            item.ErrorText     = _localizer["FailedToUploadPhoto"];
            item.UploadPercent = 0;
        }
    }

    async Task SubmitBatchAsync()
    {
        ClearStatusMessage();

        if(SelectedLicense == null)
        {
            SetStatusMessage(_localizer["SelectLicenseRequired"], InfoBarSeverity.Warning);

            return;
        }

        if(!CanSubmitBatch)
        {
            SetStatusMessage(_localizer["MachinePhotosBatchNotReady"], InfoBarSeverity.Warning);

            return;
        }

        List<AdminMachinePhotoStagedItem> readyItems = StagedPhotos.Where(s => s.IsReady && s.PendingId.HasValue).ToList();

        if(readyItems.Count == 0)
        {
            SetStatusMessage(_localizer["MachinePhotosBatchNotReady"], InfoBarSeverity.Warning);

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

            foreach(AdminMachinePhotoStagedItem item in readyItems)
            {
                item.Status        = AdminMachinePhotoStageStatus.Committing;
                item.StatusText    = _localizer["MachinePhotosCommittingCardStatus"];
                item.UploadPercent = 0;
                item.ErrorText     = string.Empty;
            }

            var request = new AdminMachinePhotoBatchCommitRequestDto
            {
                MachineId = _machineId,
                LicenseId = SelectedLicense.Id ?? 0,
                Items = readyItems.Select(item => new AdminMachinePhotoBatchCommitItemDto
                {
                    PendingId = item.PendingId ?? Guid.Empty,
                    Source    = string.IsNullOrWhiteSpace(item.SourceUrl) ? null : item.SourceUrl
                }).ToList()
            };

            (AdminMachinePhotoBatchJobStatusDto? result, string? error) = await _photosService.CommitAdminBatchAsync(request);

            if(result == null)
            {
                foreach(AdminMachinePhotoStagedItem item in readyItems)
                {
                    item.Status      = AdminMachinePhotoStageStatus.Ready;
                    item.StatusText  = _localizer["MachinePhotosReadyCardStatus"];
                    item.ErrorText   = string.Empty;
                    item.UploadPercent = 100;
                }

                SetStatusMessage(string.IsNullOrWhiteSpace(error) ? _localizer["MachinePhotosCommitFailed"] : error,
                                 InfoBarSeverity.Error);

                ShowBatchProgress = false;

                return;
            }

            AdminMachinePhotoBatchJobStatusDto? finalStatus =
                result.JobId.HasValue ? await PollBatchUntilCompleteAsync(result.JobId.Value) : null;

            if(finalStatus == null)
            {
                SetStatusMessage(_localizer["MachinePhotosPollingFailed"], InfoBarSeverity.Error);

                return;
            }

            ApplyBatchStatus(finalStatus);

            List<AdminMachinePhotoBatchJobItemResultDto> finalResults = finalStatus.Results ?? [];
            int succeeded = finalResults.Count(r => r.Succeeded ?? false);
            int failed    = finalResults.Count(r => !(r.Succeeded ?? false));

            SetStatusMessage(string.Format(_localizer["MachinePhotosBatchFinished"], succeeded, failed),
                             failed == 0 ? InfoBarSeverity.Success : InfoBarSeverity.Warning);

            ShowBatchSummary = true;
            HasStatusMessage = false;

            foreach(AdminMachinePhotoStagedItem item in StagedPhotos)
                item.PendingId = null;

            if(succeeded > 0)
                await LoadPhotosAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error committing machine photo batch");
            SetStatusMessage(_localizer["MachinePhotosCommitFailed"], InfoBarSeverity.Error);
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

    async Task<AdminMachinePhotoBatchJobStatusDto?> PollBatchUntilCompleteAsync(Guid jobId)
    {
        while(true)
        {
            AdminMachinePhotoBatchJobStatusDto? status = await _photosService.GetAdminBatchStatusAsync(jobId);

            if(status == null)
                return null;

            ApplyBatchStatus(status);

            BatchJobState state = (BatchJobState)(status.State ?? 0);

            if(state is BatchJobState.Completed or BatchJobState.Failed)
                return status;

            await Task.Delay(750);
        }
    }

    void ApplyBatchStatus(AdminMachinePhotoBatchJobStatusDto status)
    {
        BatchProcessed   = status.Processed ?? 0;
        BatchTotal       = status.Total ?? 0;
        CurrentPendingId = status.CurrentPendingId;

        Dictionary<Guid, AdminMachinePhotoBatchJobItemResultDto> resultMap = (status.Results ?? [])
                                                                             .Where(r => r.PendingId.HasValue &&
                                                                                         r.PendingId.Value != Guid.Empty)
                                                                             .ToDictionary(r => r.PendingId!.Value, r => r);

        foreach(AdminMachinePhotoStagedItem item in StagedPhotos)
        {
            if(item.PendingId is not Guid pendingId)
                continue;

            if(resultMap.TryGetValue(pendingId, out AdminMachinePhotoBatchJobItemResultDto? result))
            {
                if(result.Succeeded ?? false)
                {
                    item.Status        = AdminMachinePhotoStageStatus.Succeeded;
                    item.StatusText    = _localizer["MachinePhotosSucceededCardStatus"];
                    item.ErrorText     = string.Empty;
                    item.UploadPercent = 100;
                }
                else
                {
                    item.Status        = AdminMachinePhotoStageStatus.Failed;
                    item.StatusText    = _localizer["MachinePhotosFailedCardStatus"];
                    item.ErrorText     = string.IsNullOrWhiteSpace(result.Error) ? _localizer["MachinePhotosCommitFailed"] : result.Error;
                    item.UploadPercent = 100;
                }
            }
            else if(status.CurrentPendingId.HasValue && status.CurrentPendingId.Value == pendingId)
            {
                item.Status        = AdminMachinePhotoStageStatus.Committing;
                item.StatusText    = _localizer["MachinePhotosProcessingCardStatus"];
                item.UploadPercent = 50;
                item.ErrorText     = string.Empty;
            }
            else if(item.Status == AdminMachinePhotoStageStatus.Committing)
            {
                item.StatusText = _localizer["MachinePhotosQueuedCardStatus"];
            }
        }
    }

    async Task RemoveStagedPhotoAsync(AdminMachinePhotoStagedItem? item)
    {
        if(item == null || item.Status == AdminMachinePhotoStageStatus.Committing)
            return;

        try
        {
            if(item.PendingId.HasValue && !item.IsTerminal)
            {
                bool deleted = await _photosService.DeleteAdminPendingPhotoAsync(item.PendingId.Value);

                if(!deleted)
                {
                    SetStatusMessage(_localizer["MachinePhotosFailedToRemoveStaged"], InfoBarSeverity.Warning);

                    return;
                }
            }

            RemoveStagedPhoto(item);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing staged machine photo {ClientId}", item.ClientId);
            SetStatusMessage(_localizer["MachinePhotosFailedToRemoveStaged"], InfoBarSeverity.Warning);
        }
    }

    async Task ClearStagedPhotosAsync()
    {
        List<AdminMachinePhotoStagedItem> items = StagedPhotos.ToList();

        foreach(AdminMachinePhotoStagedItem item in items)
        {
            if(item.PendingId.HasValue && !item.IsTerminal)
            {
                try
                {
                    await _photosService.DeleteAdminPendingPhotoAsync(item.PendingId.Value);
                }
                catch(Exception ex)
                {
                    _logger.LogDebug(ex, "Best-effort cleanup failed for staged machine photo {PendingId}",
                                     item.PendingId.Value);
                }
            }

            RemoveStagedPhoto(item);
        }

        ShowBatchProgress = false;
        ShowBatchSummary  = false;
        BatchProcessed    = 0;
        BatchTotal        = 0;
        CurrentPendingId  = null;
    }

    async Task DeletePhotoAsync(MachinePhotoDisplayItem? item)
    {
        if(item == null) return;

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer,
               string.IsNullOrWhiteSpace(item.UploadDate) ? item.PhotoId.ToString() : item.UploadDate))
            return;

        try
        {
            ClearStatusMessage();

            bool success = await _photosService.DeletePhotoAsync(item.PhotoId);

            if(!success)
            {
                SetStatusMessage(_localizer["FailedToDeletePhoto"], InfoBarSeverity.Error);

                return;
            }

            await LoadPhotosAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo {PhotoId}", item.PhotoId);
            SetStatusMessage(_localizer["FailedToDeletePhoto"], InfoBarSeverity.Error);
        }
    }

    void ViewPhoto(MachinePhotoDisplayItem? item)
    {
        if(item == null) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.PhotoId, item.PhotoId }
        };

        _regionManager.RequestNavigate(RegionNames.Content, "PhotoDetailPage", parameters);
    }

    void GoBack() => _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminMachinesPage));

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

    void AddStagedPhoto(AdminMachinePhotoStagedItem item)
    {
        item.PropertyChanged += OnStagedPhotoPropertyChanged;
        StagedPhotos.Add(item);
        NotifyUploadStateChanged();
    }

    void RemoveStagedPhoto(AdminMachinePhotoStagedItem item)
    {
        item.PropertyChanged -= OnStagedPhotoPropertyChanged;
        StagedPhotos.Remove(item);
        NotifyUploadStateChanged();
    }

    void OnStagedPhotoPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(AdminMachinePhotoStagedItem.Status) or nameof(AdminMachinePhotoStagedItem.PendingId))
            NotifyUploadStateChanged();
    }

    void NotifyUploadStateChanged()
    {
        OnPropertyChanged(nameof(HasStagedPhotos));
        OnPropertyChanged(nameof(CanAddMorePhotos));
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
