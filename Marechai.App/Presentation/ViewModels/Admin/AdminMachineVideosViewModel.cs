#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Dialogs;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminMachineVideosViewModel : ObservableObject, IRegionAware
{
    readonly IJwtService                          _jwtService;
    readonly IStringLocalizer                     _localizer;
    readonly ILogger<AdminMachineVideosViewModel> _logger;
    readonly MachinesService                      _machinesService;
    readonly IRegionManager                       _regionManager;
    readonly ITokenService                        _tokenService;

    int _machineId;

    [ObservableProperty]
    private string _machineName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<AdminMachineVideoItem> _videos = [];

    [ObservableProperty]
    private string _urlInput = string.Empty;

    [ObservableProperty]
    private string _detectedVideoId = string.Empty;

    [ObservableProperty]
    private string _manualVideoId = string.Empty;

    [ObservableProperty]
    private string _provider = "YouTube";

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

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

    public AdminMachineVideosViewModel(MachinesService                      machinesService,
                                       IJwtService                          jwtService,
                                       ITokenService                        tokenService,
                                       IStringLocalizer                     localizer,
                                       ILogger<AdminMachineVideosViewModel> logger,
                                       IRegionManager                       regionManager)
    {
        _machinesService = machinesService;
        _jwtService       = jwtService;
        _tokenService     = tokenService;
        _localizer        = localizer;
        _logger           = logger;
        _regionManager    = regionManager;

        LoadVideosCommand     = new AsyncRelayCommand(LoadVideosAsync);
        LinkVideoCommand      = new AsyncRelayCommand(LinkVideoAsync);
        SaveVideoTitleCommand = new AsyncRelayCommand<AdminMachineVideoItem>(SaveVideoTitleAsync);
        DeleteVideoCommand    = new AsyncRelayCommand<AdminMachineVideoItem>(DeleteVideoAsync);
        OpenVideoCommand      = new AsyncRelayCommand<AdminMachineVideoItem>(OpenVideoAsync);
        GoBackCommand         = new RelayCommand(GoBack);

        CheckAdminRole();
    }

    public IAsyncRelayCommand LoadVideosCommand { get; }
    public IAsyncRelayCommand LinkVideoCommand { get; }
    public IAsyncRelayCommand<AdminMachineVideoItem> SaveVideoTitleCommand { get; }
    public IAsyncRelayCommand<AdminMachineVideoItem> DeleteVideoCommand { get; }
    public IAsyncRelayCommand<AdminMachineVideoItem> OpenVideoCommand { get; }
    public IRelayCommand GoBackCommand { get; }

    public bool CanLinkVideo =>
        !IsSaving &&
        !string.IsNullOrWhiteSpace(EffectiveProvider) &&
        !string.IsNullOrWhiteSpace(EffectiveVideoId);

    string EffectiveProvider =>
        string.IsNullOrWhiteSpace(ManualVideoId) ? "YouTube" : (Provider ?? string.Empty).Trim();

    string EffectiveVideoId =>
        !string.IsNullOrWhiteSpace(ManualVideoId) ? ManualVideoId.Trim() : DetectedVideoId;

    partial void OnUrlInputChanged(string value)
    {
        DetectedVideoId = TryExtractYouTubeVideoId(value, out string? id) ? id ?? string.Empty : string.Empty;
        OnPropertyChanged(nameof(CanLinkVideo));
    }

    partial void OnManualVideoIdChanged(string value) => OnPropertyChanged(nameof(CanLinkVideo));
    partial void OnProviderChanged(string value) => OnPropertyChanged(nameof(CanLinkVideo));
    partial void OnIsSavingChanged(bool value) => OnPropertyChanged(nameof(CanLinkVideo));

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.MachineId, out int machineId))
            _machineId = machineId;

        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.MachineName, out string? machineName))
            MachineName = machineName ?? string.Empty;

        if(IsAdmin)
            await LoadVideosAsync();
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

            if(!_jwtService.IsTokenValid(token)) { IsAdmin = false; return; }
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

    async Task LoadVideosAsync()
    {
        try
        {
            IsLoading        = true;
            HasError         = false;
            ErrorMessage     = string.Empty;
            HasStatusMessage = false;
            Videos.Clear();

            if(string.IsNullOrWhiteSpace(MachineName))
            {
                MachineDto? machine = await _machinesService.GetMachineByIdAsync(_machineId);

                MachineName = machine?.Name ?? string.Empty;
            }

            List<MachineVideoDto> videoDtos = await _machinesService.GetVideosByMachineAsync(_machineId);

            foreach(MachineVideoDto video in videoDtos.OrderBy(v => v.Title).ThenBy(v => v.VideoId))
            {
                if(video.Id is null) continue;

                Videos.Add(new AdminMachineVideoItem
                {
                    Id          = video.Id.Value,
                    MachineId   = video.MachineId ?? _machineId,
                    MachineName = video.MachineName ?? MachineName,
                    Provider    = video.Provider ?? string.Empty,
                    VideoId     = video.VideoId ?? string.Empty,
                    Title       = video.Title ?? string.Empty
                });
            }

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading machine videos for {MachineId}", _machineId);
            ErrorMessage = _localizer["MachineVideosLoadFailed"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task LinkVideoAsync()
    {
        if(!CanLinkVideo) return;

        try
        {
            IsSaving = true;
            ClearMessages();

            (MachineVideoDto? dto, string? error) = await _machinesService.CreateVideoAsync(_machineId,
                EffectiveProvider,
                EffectiveVideoId,
                string.IsNullOrWhiteSpace(Title) ? null : Title.Trim());

            if(dto is null)
            {
                ShowError(string.IsNullOrWhiteSpace(error) ? _localizer["MachineVideosLinkFailed"] : error);

                return;
            }

            UrlInput        = string.Empty;
            ManualVideoId   = string.Empty;
            Provider        = "YouTube";
            Title           = string.Empty;
            DetectedVideoId = string.Empty;

            ShowStatus(_localizer["MachineVideosLinkedSuccessfully"], InfoBarSeverity.Success);
            await LoadVideosAsync();
            HasStatusMessage = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error linking machine video for {MachineId}", _machineId);
            ShowError(_localizer["MachineVideosLinkFailed"]);
        }
        finally
        {
            IsSaving = false;
        }
    }

    async Task SaveVideoTitleAsync(AdminMachineVideoItem? video)
    {
        if(video is null) return;

        try
        {
            ClearMessages();

            (bool succeeded, string? error) = await _machinesService.UpdateVideoTitleAsync(video.Id,
                string.IsNullOrWhiteSpace(video.Title) ? null : video.Title.Trim());

            if(!succeeded)
            {
                ShowError(string.IsNullOrWhiteSpace(error) ? _localizer["MachineVideosUpdateFailed"] : error);

                return;
            }

            ShowStatus(_localizer["MachineVideosUpdatedSuccessfully"], InfoBarSeverity.Success);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating machine video {VideoId}", video.Id);
            ShowError(_localizer["MachineVideosUpdateFailed"]);
        }
    }

    async Task DeleteVideoAsync(AdminMachineVideoItem? video)
    {
        if(video is null) return;

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer,
               string.IsNullOrWhiteSpace(video.Title) ? video.VideoId : video.Title))
            return;

        try
        {
            ClearMessages();

            (bool succeeded, string? error) = await _machinesService.DeleteVideoAsync(video.Id);

            if(!succeeded)
            {
                ShowError(string.IsNullOrWhiteSpace(error) ? _localizer["MachineVideosDeleteFailed"] : error);

                return;
            }

            Videos.Remove(video);
            ShowStatus(_localizer["MachineVideosDeletedSuccessfully"], InfoBarSeverity.Success);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine video {VideoId}", video.Id);
            ShowError(_localizer["MachineVideosDeleteFailed"]);
        }
    }

    async Task OpenVideoAsync(AdminMachineVideoItem? video)
    {
        if(video?.LaunchUri is null) return;

        try
        {
            await Launcher.LaunchUriAsync(video.LaunchUri);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error opening machine video {VideoId}", video.VideoId);
            ShowError(_localizer["MachineVideosOpenFailed"]);
        }
    }

    void GoBack()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminMachinesPage));
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

    static bool TryExtractYouTubeVideoId(string? input, out string? videoId)
    {
        videoId = null;

        if(string.IsNullOrWhiteSpace(input)) return false;

        string trimmed = input.Trim();

        if(trimmed.Length == 11 &&
           trimmed.All(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_'))
        {
            videoId = trimmed;

            return true;
        }

        if(!Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? uri)) return false;

        string host = uri.Host.ToLowerInvariant();

        if(host is "youtu.be" or "www.youtu.be")
        {
            string candidate = uri.AbsolutePath.Trim('/');

            if(candidate.Length == 11)
            {
                videoId = candidate;

                return true;
            }
        }

        if(host.Contains("youtube.com") || host.Contains("youtube-nocookie.com"))
        {
            if(uri.AbsolutePath.StartsWith("/watch", StringComparison.OrdinalIgnoreCase))
            {
                string query = uri.Query.TrimStart('?');

                foreach(string part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] kvp = part.Split('=', 2);

                    if(kvp.Length == 2 && kvp[0] == "v" && kvp[1].Length == 11)
                    {
                        videoId = Uri.UnescapeDataString(kvp[1]);

                        return true;
                    }
                }
            }

            string[] segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

            if(segments.Length >= 2 &&
               segments[0] is "embed" or "v" or "shorts" or "live" &&
               segments[1].Length == 11)
            {
                videoId = segments[1];

                return true;
            }
        }

        return false;
    }
}
