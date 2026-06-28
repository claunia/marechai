#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwarePromoArtViewModel : ObservableObject, IRegionAware
{
    static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"
    };

    readonly IJwtService                                 _jwtService;
    readonly IStringLocalizer                            _localizer;
    readonly ILogger<AdminSoftwarePromoArtViewModel>     _logger;
    readonly SoftwarePromoArtCache                       _softwarePromoArtCache;
    readonly SoftwarePromoArtService                     _softwarePromoArtService;
    readonly SoftwareService                             _softwareService;
    readonly IRegionManager                              _regionManager;
    readonly ITokenService                                _tokenService;
    readonly ImageSourceFactory                           _imageSourceFactory;

    int _softwareId;

    [ObservableProperty]
    private string _softwareName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<AdminSoftwarePromoArtItem> _promoArt = [];

    [ObservableProperty]
    private ObservableCollection<string> _groupSuggestions = [];

    [ObservableProperty]
    private string _groupNameInput = string.Empty;

    [ObservableProperty]
    private string _captionInput = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isUploading;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _hasStatusMessage;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;

    public AdminSoftwarePromoArtViewModel(SoftwarePromoArtService                 softwarePromoArtService,
                                          SoftwarePromoArtCache                   softwarePromoArtCache,
                                          SoftwareService                         softwareService,
                                          ImageSourceFactory                      imageSourceFactory,
                                          IJwtService                             jwtService,
                                          ITokenService                           tokenService,
                                          IStringLocalizer                        localizer,
                                          ILogger<AdminSoftwarePromoArtViewModel> logger,
                                          IRegionManager                          regionManager)
    {
        _softwarePromoArtService = softwarePromoArtService;
        _softwarePromoArtCache   = softwarePromoArtCache;
        _softwareService         = softwareService;
        _imageSourceFactory      = imageSourceFactory;
        _jwtService              = jwtService;
        _tokenService            = tokenService;
        _localizer               = localizer;
        _logger                  = logger;
        _regionManager           = regionManager;

        LoadPromoArtCommand   = new AsyncRelayCommand(LoadPromoArtAsync);
        UploadPromoArtCommand = new AsyncRelayCommand(UploadPromoArtAsync);
        SaveItemCommand       = new AsyncRelayCommand<AdminSoftwarePromoArtItem>(SaveItemAsync);
        DeleteItemCommand     = new AsyncRelayCommand<AdminSoftwarePromoArtItem>(DeleteItemAsync);
        GoBackCommand         = new RelayCommand(GoBack);

        CheckAdminRole();
    }

    public IAsyncRelayCommand LoadPromoArtCommand { get; }
    public IAsyncRelayCommand UploadPromoArtCommand { get; }
    public IAsyncRelayCommand<AdminSoftwarePromoArtItem> SaveItemCommand { get; }
    public IAsyncRelayCommand<AdminSoftwarePromoArtItem> DeleteItemCommand { get; }
    public IRelayCommand GoBackCommand { get; }

    public bool CanUploadPromoArt => !IsUploading && !string.IsNullOrWhiteSpace(GroupNameInput);

    partial void OnGroupNameInputChanged(string value) => OnPropertyChanged(nameof(CanUploadPromoArt));
    partial void OnIsUploadingChanged(bool value) => OnPropertyChanged(nameof(CanUploadPromoArt));

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.SoftwareId, out int softwareId))
            _softwareId = softwareId;

        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.SoftwareName, out string? softwareName))
            SoftwareName = softwareName ?? string.Empty;

        if(IsAdmin)
            await LoadPromoArtAsync();
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
                      roles.Contains("UberAdmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    async Task LoadPromoArtAsync()
    {
        try
        {
            IsLoading        = true;
            HasError         = false;
            ErrorMessage     = string.Empty;
            HasStatusMessage = false;
            PromoArt.Clear();

            if(string.IsNullOrWhiteSpace(SoftwareName))
            {
                SoftwareDto? software = await _softwareService.GetByIdAsync(_softwareId);

                SoftwareName = software?.Name ?? string.Empty;
            }

            List<SoftwarePromoArtGroupDto> groups = await _softwarePromoArtService.GetGroupsAsync();

            GroupSuggestions = new ObservableCollection<string>(groups.Select(g => g.Name ?? string.Empty)
                                                                       .Where(n => !string.IsNullOrWhiteSpace(n))
                                                                       .OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase));

            List<SoftwarePromoArtDto> promoArtDtos = await _softwarePromoArtService.GetPromoArtBySoftwareAsync(_softwareId);

            foreach(SoftwarePromoArtDto promo in promoArtDtos)
            {
                if(promo.Id is null) continue;

                var item = new AdminSoftwarePromoArtItem
                {
                    Id        = promo.Id.Value,
                    GroupName = promo.GroupName ?? string.Empty,
                    Caption   = promo.Caption   ?? string.Empty
                };

                PromoArt.Add(item);

                _ = LoadThumbnailAsync(item);
            }

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading promo art for software {SoftwareId}", _softwareId);
            ErrorMessage = _localizer["SoftwarePromoArtLoadFailed"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task LoadThumbnailAsync(AdminSoftwarePromoArtItem item)
    {
        try
        {
            Stream stream = await _softwarePromoArtCache.GetThumbnailAsync(item.Id);
            item.ThumbnailImageSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading promo art thumbnail {PromoArtId}", item.Id);
        }
    }

    async Task UploadPromoArtAsync()
    {
        if(!CanUploadPromoArt) return;

        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();

            foreach(string extension in _allowedExtensions)
                picker.FileTypeFilter.Add(extension);

#if HAS_UNO
            // Uno Platform handles this automatically
#else
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
#endif

            Windows.Storage.StorageFile? file = await picker.PickSingleFileAsync();

            if(file is null) return;

            IsUploading = true;
            ClearMessages();

            using Stream stream = await file.OpenStreamForReadAsync();
            using var    ms     = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            SoftwarePromoArtDto? result = await _softwarePromoArtService.UploadPromoArtAsync(_softwareId, fileBytes,
                GroupNameInput.Trim(), string.IsNullOrWhiteSpace(CaptionInput) ? null : CaptionInput.Trim());

            if(result is null)
            {
                ShowError(_localizer["SoftwarePromoArtUploadFailed"]);

                return;
            }

            GroupNameInput = string.Empty;
            CaptionInput   = string.Empty;

            ShowStatus(_localizer["SoftwarePromoArtUploadedSuccessfully"], InfoBarSeverity.Success);
            await LoadPromoArtAsync();
            HasStatusMessage = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading promo art for software {SoftwareId}", _softwareId);
            ShowError(_localizer["SoftwarePromoArtUploadFailed"]);
        }
        finally
        {
            IsUploading = false;
        }
    }

    async Task SaveItemAsync(AdminSoftwarePromoArtItem? item)
    {
        if(item is null) return;

        try
        {
            ClearMessages();

            bool succeeded = await _softwarePromoArtService.UpdatePromoArtAsync(item.Id,
                string.IsNullOrWhiteSpace(item.GroupName) ? null : item.GroupName.Trim(),
                string.IsNullOrWhiteSpace(item.Caption) ? null : item.Caption.Trim());

            if(!succeeded)
            {
                ShowError(_localizer["SoftwarePromoArtUpdateFailed"]);

                return;
            }

            ShowStatus(_localizer["SoftwarePromoArtUpdatedSuccessfully"], InfoBarSeverity.Success);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating promo art {PromoArtId}", item.Id);
            ShowError(_localizer["SoftwarePromoArtUpdateFailed"]);
        }
    }

    async Task DeleteItemAsync(AdminSoftwarePromoArtItem? item)
    {
        if(item is null) return;

        try
        {
            ClearMessages();

            bool succeeded = await _softwarePromoArtService.DeletePromoArtAsync(item.Id);

            if(!succeeded)
            {
                ShowError(_localizer["SoftwarePromoArtDeleteFailed"]);

                return;
            }

            PromoArt.Remove(item);
            ShowStatus(_localizer["SoftwarePromoArtDeletedSuccessfully"], InfoBarSeverity.Success);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting promo art {PromoArtId}", item.Id);
            ShowError(_localizer["SoftwarePromoArtDeleteFailed"]);
        }
    }

    void GoBack()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoftwarePage));
    }

    void ClearMessages()
    {
        HasError         = false;
        ErrorMessage     = string.Empty;
        HasStatusMessage = false;
        StatusMessage    = string.Empty;
    }

    void ShowStatus(string message, InfoBarSeverity severity)
    {
        StatusMessage    = message;
        StatusSeverity   = severity;
        HasStatusMessage = !string.IsNullOrWhiteSpace(message);
        HasError         = false;
        ErrorMessage     = string.Empty;
    }

    void ShowError(string message)
    {
        ErrorMessage     = message;
        HasError         = !string.IsNullOrWhiteSpace(message);
        HasStatusMessage = false;
        StatusMessage    = string.Empty;
    }
}
