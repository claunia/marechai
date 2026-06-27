#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Services;
using Marechai.App.Services.Caching;

namespace Marechai.App.Presentation.ViewModels;

public partial class MachinePromoArtDetailViewModel : ObservableObject, IRegionAware
{
    readonly ComputersService                              _computersService;
    readonly ImageSourceFactory                            _imageSourceFactory;
    readonly IStringLocalizer                              _localizer;
    readonly ILogger<MachinePromoArtDetailViewModel>       _logger;
    readonly MachinePromoArtCache                          _machinePromoArtCache;
    readonly MachinePromoArtService                        _machinePromoArtService;
    readonly IRegionManager                                _regionManager;

    [ObservableProperty]
    private string _caption = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _errorOccurred;

    [ObservableProperty]
    private string _groupName = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _machineName = string.Empty;

    [ObservableProperty]
    private Microsoft.UI.Xaml.Media.Imaging.BitmapImage? _promoArtSource;

    public MachinePromoArtDetailViewModel(MachinePromoArtService                  machinePromoArtService,
                                          MachinePromoArtCache                    machinePromoArtCache,
                                          ComputersService                        computersService,
                                          ImageSourceFactory                      imageSourceFactory,
                                          IStringLocalizer                        localizer,
                                          ILogger<MachinePromoArtDetailViewModel> logger,
                                          IRegionManager                          regionManager)
    {
        _machinePromoArtService = machinePromoArtService;
        _machinePromoArtCache   = machinePromoArtCache;
        _computersService       = computersService;
        _imageSourceFactory     = imageSourceFactory;
        _localizer              = localizer;
        _logger                 = logger;
        _regionManager          = regionManager;
    }

    public bool HasCaption => !string.IsNullOrWhiteSpace(Caption);
    public bool HasGroupName => !string.IsNullOrWhiteSpace(GroupName);

    partial void OnCaptionChanged(string value) => OnPropertyChanged(nameof(HasCaption));
    partial void OnGroupNameChanged(string value) => OnPropertyChanged(nameof(HasGroupName));

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<Guid>(NavParamKeys.MachinePromoArtId, out Guid promoArtId))
            _ = LoadPromoArtCommand.ExecuteAsync(promoArtId);
    }

    [RelayCommand]
    public Task GoBack()
    {
        _regionManager.Regions[RegionNames.Content].NavigationService.Journal.GoBack();

        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task LoadPromoArt(Guid promoArtId)
    {
        try
        {
            IsLoading      = true;
            ErrorOccurred  = false;
            ErrorMessage   = string.Empty;
            PromoArtSource = null;
            MachineName    = string.Empty;
            GroupName      = string.Empty;
            Caption        = string.Empty;

            _logger.LogInformation("Loading machine promo art details for {PromoArtId}", promoArtId);

            MachinePromoArtDto? promoArt = await _machinePromoArtService.GetPromoArtDetailsAsync(promoArtId);

            if(promoArt is null)
            {
                ErrorOccurred = true;
                ErrorMessage  = _localizer["PromoArtNotFound"];
                IsLoading     = false;

                return;
            }

            GroupName = promoArt.GroupName ?? string.Empty;
            Caption   = promoArt.Caption   ?? string.Empty;

            int machineId = promoArt.MachineId ?? 0;

            MachineDto? machine = machineId > 0
                                      ? await _computersService.GetMachineByIdAsync(machineId)
                                      : null;
            MachineName = machine?.Name ?? string.Empty;

            await LoadPromoArtImageAsync(promoArtId);

            IsLoading = false;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading machine promo art details for {PromoArtId}", promoArtId);
            ErrorOccurred = true;
            ErrorMessage  = ex.Message;
            IsLoading     = false;
        }
    }

    async Task LoadPromoArtImageAsync(Guid promoArtId)
    {
        try
        {
            Stream stream = await _machinePromoArtCache.GetPromoArtAsync(promoArtId);
            PromoArtSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading machine promo art image {PromoArtId}", promoArtId);
            ErrorOccurred = true;
            ErrorMessage  = _localizer["FailedToLoadPromoArtImage"];
        }
    }
}
