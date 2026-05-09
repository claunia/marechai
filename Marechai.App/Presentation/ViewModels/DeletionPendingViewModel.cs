#nullable enable

using System;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels;

/// <summary>
///     Backs <see cref="DeletionPendingPage" />. Shows the deletion-grace status, lets the user cancel
///     the pending deletion, and (optionally) downloads the GDPR data export. Reachable via the
///     <see cref="SettingsPage" /> link or after a server-side 403 with <c>type=deletion-pending</c>.
/// </summary>
public partial class DeletionPendingViewModel : ObservableObject
{
    private readonly AuthService      _authService;
    private readonly IRegionManager   _regionManager;
    private readonly IStringLocalizer _stringLocalizer;

    [ObservableProperty] private AccountDeletionStatusDto? _status;
    [ObservableProperty] private bool                       _isLoading = true;
    [ObservableProperty] private bool                       _isCancelling;
    [ObservableProperty] private bool                       _isDownloading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMessage))]
    private string? _message;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public DeletionPendingViewModel(IRegionManager regionManager, AuthService authService,
                                    IStringLocalizer stringLocalizer)
    {
        _regionManager   = regionManager;
        _authService     = authService;
        _stringLocalizer = stringLocalizer;
        _ = RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        IsLoading = true;
        try { Status = await _authService.GetAccountDeletionStatusAsync() ?? new AccountDeletionStatusDto { IsPending = false }; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        IsCancelling = true;

        try
        {
            bool ok = await _authService.CancelAccountDeletionAsync();
            Message = ok
                          ? _stringLocalizer["DeletionPendingPage.CancelSuccess"]
                          : _stringLocalizer["DeletionPendingPage.CancelFailed"];
            await RefreshAsync();
        }
        finally
        {
            IsCancelling = false;
        }
    }

    [RelayCommand]
    private async Task DownloadAsync()
    {
        IsDownloading = true;

        try
        {
            byte[]? bytes = await _authService.ExportDataAsync();

            if(bytes is null || bytes.Length == 0)
            {
                Message = _stringLocalizer["DeletionPendingPage.DownloadFailed"];

                return;
            }

            // Save via Windows.Storage.Pickers.FileSavePicker, which Uno surfaces on every platform via
            // its OS-native file dialog. The picker requires the page's HWND on desktop; Uno wires that
            // up automatically via WinRT.Interop bridging.
            var picker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedFileName = $"marechai-user-data-{DateTime.UtcNow:yyyyMMdd}",
                DefaultFileExtension = ".json",
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeChoices.Add("JSON", new System.Collections.Generic.List<string> { ".json" });

            // Initialize HWND on desktop targets where the picker requires it. On WASM/iOS/Android this
            // call is a no-op via the IInitializeWithWindow bridging.
            try
            {
                var hwnd = Microsoft.UI.Xaml.Window.Current is { } w
                               ? WinRT.Interop.WindowNative.GetWindowHandle(w)
                               : System.IntPtr.Zero;
                if(hwnd != System.IntPtr.Zero) WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }
            catch { /* Picker still works on platforms that don't need HWND */ }

            Windows.Storage.StorageFile file = await picker.PickSaveFileAsync();

            if(file is null) return;  // User cancelled.

            await Windows.Storage.FileIO.WriteBytesAsync(file, bytes);
            Message = _stringLocalizer["DeletionPendingPage.DownloadSaved"];
        }
        catch(Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsDownloading = false;
        }
    }

    [RelayCommand]
    private void BackToSettings() => _regionManager.RequestNavigate(RegionNames.Content, nameof(SettingsPage));
}
