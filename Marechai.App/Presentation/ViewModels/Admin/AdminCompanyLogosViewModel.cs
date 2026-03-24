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
using Microsoft.UI.Xaml.Media.Imaging;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminCompanyLogosViewModel : ObservableObject, IRegionAware
{
    readonly CompanyLogosService                      _logosService;
    readonly CompanyLogoCache                         _logoCache;
    readonly ImageSourceFactory                       _imageSourceFactory;
    readonly IJwtService                              _jwtService;
    readonly ITokenService                            _tokenService;
    readonly IStringLocalizer                         _localizer;
    readonly ILogger<AdminCompanyLogosViewModel>      _logger;
    readonly IRegionManager                           _regionManager;

    int _companyId;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyLogoDisplayItem> _logos = [];

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

    // --- Year edit state ---
    [ObservableProperty]
    private bool _isEditingYear;

    [ObservableProperty]
    private int? _editingYear;

    [ObservableProperty]
    private bool _yearIsUnknown;

    int _editingLogoId;

    public AdminCompanyLogosViewModel(CompanyLogosService                      logosService,
                                      CompanyLogoCache                         logoCache,
                                      ImageSourceFactory                       imageSourceFactory,
                                      IJwtService                              jwtService,
                                      ITokenService                            tokenService,
                                      IStringLocalizer                         localizer,
                                      ILogger<AdminCompanyLogosViewModel>      logger,
                                      IRegionManager                           regionManager)
    {
        _logosService       = logosService;
        _logoCache          = logoCache;
        _imageSourceFactory = imageSourceFactory;
        _jwtService         = jwtService;
        _tokenService       = tokenService;
        _localizer          = localizer;
        _logger             = logger;
        _regionManager      = regionManager;

        LoadLogosCommand      = new AsyncRelayCommand(LoadLogosAsync);
        UploadLogoCommand     = new AsyncRelayCommand(UploadLogoAsync);
        DeleteLogoCommand     = new AsyncRelayCommand<CompanyLogoDisplayItem>(DeleteLogoAsync);
        OpenEditYearCommand   = new RelayCommand<CompanyLogoDisplayItem>(OpenEditYear);
        SaveYearCommand       = new AsyncRelayCommand(SaveYearAsync);
        CancelEditYearCommand = new RelayCommand(CancelEditYear);
        GoBackCommand         = new RelayCommand(GoBack);

        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand                          LoadLogosCommand      { get; }
    public IAsyncRelayCommand                          UploadLogoCommand     { get; }
    public IAsyncRelayCommand<CompanyLogoDisplayItem>  DeleteLogoCommand     { get; }
    public IRelayCommand<CompanyLogoDisplayItem>       OpenEditYearCommand   { get; }
    public IAsyncRelayCommand                          SaveYearCommand       { get; }
    public IRelayCommand                               CancelEditYearCommand { get; }
    public IRelayCommand                               GoBackCommand         { get; }

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.CompanyId, out int companyId))
            _companyId = companyId;

        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.CompanyName, out string? name))
            CompanyName = name ?? string.Empty;

        if(IsAdmin)
            _ = LoadLogosCommand.ExecuteAsync(null);
    }

    // --- Role check ---
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

    // --- Load logos ---
    async Task LoadLogosAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Logos.Clear();

            List<CompanyLogoDto> logosList = await _logosService.GetLogosAsync(_companyId);

            foreach(CompanyLogoDto logo in logosList.OrderBy(l => l.Year))
            {
                if(logo.Guid == null) continue;

                var item = new CompanyLogoDisplayItem
                {
                    Id   = logo.Id ?? 0,
                    Guid = logo.Guid.Value,
                    Year = logo.Year
                };

                Logos.Add(item);

                // Fire-and-forget logo rendering (ObservableObject triggers UI update)
                _ = LoadLogoImageAsync(item);
            }

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading logos for company {CompanyId}", _companyId);
            ErrorMessage = _localizer["FailedToLoadLogos"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task LoadLogoImageAsync(CompanyLogoDisplayItem item)
    {
        try
        {
            Stream logoStream = await _logoCache.GetLogoAsync(item.Guid);
            BitmapImage? logoSource = await _imageSourceFactory.CreateSvgImageSourceAsync(logoStream);
            item.LogoSource = logoSource;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading logo image for GUID {Guid}", item.Guid);
        }
    }

    // --- Upload logo ---
    async Task UploadLogoAsync()
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".svg");

            // WinUI 3 requires window handle initialization
#if HAS_UNO
            // Uno Platform handles this automatically
#else
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
            byte[] svgBytes = ms.ToArray();

            // Ask for year — use null if unknown
            int? year = null; // Default to null; the user can set year after upload via Edit Year

            CompanyLogoDto? result = await _logosService.UploadLogoAsync(_companyId, svgBytes, year);

            if(result == null)
            {
                ErrorMessage = _localizer["FailedToUploadLogo"];
                HasError     = true;

                return;
            }

            // Reload logos to show the new one
            await LoadLogosAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading logo");
            ErrorMessage = _localizer["FailedToUploadLogo"];
            HasError     = true;
        }
        finally
        {
            IsUploading = false;
        }
    }

    // --- Delete logo ---
    async Task DeleteLogoAsync(CompanyLogoDisplayItem? item)
    {
        if(item == null) return;

        try
        {
            HasError     = false;
            ErrorMessage = string.Empty;

            bool success = await _logosService.DeleteLogoAsync(item.Id);

            if(!success)
            {
                ErrorMessage = _localizer["FailedToDeleteLogo"];
                HasError     = true;

                return;
            }

            // Invalidate cached SVG
            await _logoCache.InvalidateCacheAsync(item.Guid);

            // Reload
            await LoadLogosAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting logo {LogoId}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteLogo"];
            HasError     = true;
        }
    }

    // --- Edit year ---
    void OpenEditYear(CompanyLogoDisplayItem? item)
    {
        if(item == null) return;

        _editingLogoId = item.Id;
        EditingYear    = item.Year;
        YearIsUnknown  = item.Year == null;
        IsEditingYear  = true;
    }

    async Task SaveYearAsync()
    {
        try
        {
            HasError     = false;
            ErrorMessage = string.Empty;

            int? year = YearIsUnknown ? null : EditingYear;

            bool success = await _logosService.ChangeYearAsync(_editingLogoId, year);

            if(!success)
            {
                ErrorMessage = _localizer["FailedToSaveYear"];
                HasError     = true;

                return;
            }

            IsEditingYear = false;
            await LoadLogosAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving year for logo {LogoId}", _editingLogoId);
            ErrorMessage = _localizer["FailedToSaveYear"];
            HasError     = true;
        }
    }

    void CancelEditYear()
    {
        IsEditingYear = false;
        HasError      = false;
        ErrorMessage  = string.Empty;
    }

    // --- Navigation ---
    void GoBack() => _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminCompaniesPage));
}
