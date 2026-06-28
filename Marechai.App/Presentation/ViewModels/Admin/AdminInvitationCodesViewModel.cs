#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Microsoft.UI.Xaml.Data;
using Windows.ApplicationModel.DataTransfer;

namespace Marechai.App.Presentation.ViewModels.Admin;

[Bindable]
public partial class AdminInvitationCodesViewModel : ObservableObject, IRegionAware
{
    readonly InvitationCodesService _invitationCodesService;
    readonly IJwtService _jwtService;
    readonly ILogger<AdminInvitationCodesViewModel> _logger;
    readonly ITokenService _tokenService;

    [ObservableProperty] bool _isAdmin;
    [ObservableProperty] bool _unusedOnly = true;
    [ObservableProperty] bool _isLoading;
    [ObservableProperty] bool _hasError;
    [ObservableProperty] bool _isDataLoaded;
    [ObservableProperty] string _errorMessage = string.Empty;
    [ObservableProperty] bool _isGenerating;
    [ObservableProperty] string? _lastGeneratedCode;

    public ObservableCollection<InvitationCodeDto> Codes { get; } = [];

    public AdminInvitationCodesViewModel(InvitationCodesService invitationCodesService,
        ILogger<AdminInvitationCodesViewModel> logger, ITokenService tokenService, IJwtService jwtService)
    {
        _invitationCodesService = invitationCodesService;
        _logger                 = logger;
        _tokenService           = tokenService;
        _jwtService             = jwtService;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        UpdateAdminStatus();
        if(IsAdmin) _ = LoadAsync();
    }

    [RelayCommand]
    Task Refresh() => LoadAsync();

    [RelayCommand]
    Task ToggleUnusedOnly() => LoadAsync();

    [RelayCommand]
    async Task GenerateCodeAsync()
    {
        IsGenerating       = true;
        HasError           = false;
        ErrorMessage       = string.Empty;
        LastGeneratedCode  = null;

        (InvitationCodeDto? created, string? error) = await _invitationCodesService.CreateAsync();

        IsGenerating = false;

        if(created is null)
        {
            HasError     = true;
            ErrorMessage = error ?? "Failed to generate invitation code.";

            return;
        }

        LastGeneratedCode = created.Code;

        if(created.Code is not null)
        {
            try
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(created.Code);
                Clipboard.SetContent(dataPackage);
            }
            catch
            {
                // Clipboard access can be denied on some platforms; the code remains visible in the banner.
            }
        }

        await LoadAsync();
    }

    [RelayCommand]
    async Task RevokeCodeAsync(InvitationCodeDto? item)
    {
        if(item?.Code is null) return;

        (bool succeeded, string? error) = await _invitationCodesService.RevokeAsync(item.Code);

        if(!succeeded)
        {
            HasError     = true;
            ErrorMessage = error ?? "Failed to revoke invitation code.";

            return;
        }

        await LoadAsync();
    }

    async Task LoadAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            IsDataLoaded = false;
            Codes.Clear();

            (var codes, string? error) = await _invitationCodesService.GetAllAsync(UnusedOnly ? true : null);

            if(error is not null)
            {
                HasError     = true;
                ErrorMessage = error;
            }

            foreach(InvitationCodeDto code in codes)
                Codes.Add(code);

            IsDataLoaded = !HasError;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading invitation codes");
            HasError     = true;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    void UpdateAdminStatus()
    {
        string token = _tokenService.GetToken();
        IsAdmin = _jwtService.GetRoles(token).Any(r =>
            string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, "UberAdmin", StringComparison.OrdinalIgnoreCase));
    }
}
