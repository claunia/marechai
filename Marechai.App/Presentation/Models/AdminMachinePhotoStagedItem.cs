using System;
using Microsoft.UI.Xaml.Media;

namespace Marechai.App.Presentation.Models;

public enum AdminMachinePhotoStageStatus
{
    Uploading,
    Ready,
    Error,
    Committing,
    Succeeded,
    Failed
}

public partial class AdminMachinePhotoStagedItem : ObservableObject
{
    public Guid ClientId { get; } = Guid.NewGuid();

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _fileSizeText = string.Empty;

    [ObservableProperty]
    private string _dimensionsText = string.Empty;

    [ObservableProperty]
    private string _sourceUrl = string.Empty;

    [ObservableProperty]
    private Guid? _pendingId;

    [ObservableProperty]
    private ImageSource? _thumbnailImageSource;

    [ObservableProperty]
    private string _errorText = string.Empty;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private double _uploadPercent;

    [ObservableProperty]
    private AdminMachinePhotoStageStatus _status = AdminMachinePhotoStageStatus.Uploading;

    public bool ShowThumbnail => ThumbnailImageSource is not null && Status != AdminMachinePhotoStageStatus.Error;
    public bool ShowProgress  => Status is AdminMachinePhotoStageStatus.Uploading or AdminMachinePhotoStageStatus.Committing;
    public bool ShowError     => !string.IsNullOrWhiteSpace(ErrorText);
    public bool IsReady       => Status == AdminMachinePhotoStageStatus.Ready;
    public bool IsTerminal    => Status is AdminMachinePhotoStageStatus.Succeeded or AdminMachinePhotoStageStatus.Failed;
    public bool CanRemove     => Status != AdminMachinePhotoStageStatus.Committing;

    partial void OnThumbnailImageSourceChanged(ImageSource? value) => NotifyStateChanged();
    partial void OnErrorTextChanged(string value)                  => NotifyStateChanged();
    partial void OnStatusChanged(AdminMachinePhotoStageStatus value) => NotifyStateChanged();

    void NotifyStateChanged()
    {
        OnPropertyChanged(nameof(ShowThumbnail));
        OnPropertyChanged(nameof(ShowProgress));
        OnPropertyChanged(nameof(ShowError));
        OnPropertyChanged(nameof(IsReady));
        OnPropertyChanged(nameof(IsTerminal));
        OnPropertyChanged(nameof(CanRemove));
    }
}
