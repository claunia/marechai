#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;

namespace Marechai.App.Presentation.ViewModels;

public partial class SoundSynthsListViewModel : ObservableObject
{
    private readonly IStringLocalizer                  _localizer;
    private readonly ILogger<SoundSynthsListViewModel> _logger;
    private readonly IRegionManager                    _regionManager;
    private readonly SoundSynthsService                _soundSynthsService;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private ObservableCollection<SoundSynthListItem> _soundSynths = [];

    public SoundSynthsListViewModel(SoundSynthsService                soundSynthsService, IRegionManager   regionManager,
                                ILogger<SoundSynthsListViewModel> logger,             IStringLocalizer localizer)
    {
        _soundSynthsService         = soundSynthsService;
        _regionManager              = regionManager;
        _logger                     = logger;
        _localizer                  = localizer;
        LoadData                    = new AsyncRelayCommand(LoadDataAsync);
        NavigateToSoundSynthCommand = new AsyncRelayCommand<SoundSynthListItem>(NavigateToSoundSynthAsync);
    }

    public IAsyncRelayCommand                     LoadData                    { get; }
    public IAsyncRelayCommand<SoundSynthListItem> NavigateToSoundSynthCommand { get; }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            IsDataLoaded = false;
            HasError     = false;
            ErrorMessage = string.Empty;

            List<SoundSynthDto> soundSynths = await _soundSynthsService.GetAllSoundSynthsAsync();

            // Separate special sound synths from regular ones
            var specialSoundSynths = new List<SoundSynthListItem>();
            var regularSoundSynths = new List<SoundSynthListItem>();

            foreach(SoundSynthDto ss in soundSynths)
            {
                string displayName = ss.Name ?? _localizer["Unknown_male"];

                // Replace special database name
                if(displayName == "DB_SOFTWARE") displayName = _localizer["Software"];

                var soundSynthItem = new SoundSynthListItem
                {
                    Id        = ss.Id ?? 0,
                    Name      = displayName,
                    Company   = ss.Company ?? _localizer["Unknown_female"],
                    IsSpecial = ss.Name == "DB_SOFTWARE"
                };

                if(soundSynthItem.IsSpecial)
                    specialSoundSynths.Add(soundSynthItem);
                else
                    regularSoundSynths.Add(soundSynthItem);

                _logger.LogInformation("Sound Synth: {Name}, Company: {Company}, ID: {Id}, IsSpecial: {IsSpecial}",
                                       displayName,
                                       ss.Company,
                                       ss.Id,
                                       soundSynthItem.IsSpecial);
            }

            // Sort regular sound synths alphabetically by name
            regularSoundSynths.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            // Add special sound synths first (Software), then regular sound synths
            SoundSynths.Clear();

            foreach(SoundSynthListItem ss in specialSoundSynths) SoundSynths.Add(ss);

            foreach(SoundSynthListItem ss in regularSoundSynths) SoundSynths.Add(ss);

            _logger.LogInformation("Successfully loaded {Count} Sound Synthesizers", SoundSynths.Count);
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading Sound Synthesizers");
            ErrorMessage = "Failed to load Sound Synthesizers. Please try again later.";
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task NavigateToSoundSynthAsync(SoundSynthListItem? item)
    {
        if(item == null) return Task.CompletedTask;

        _logger.LogInformation("Navigating to Sound Synthesizer {SoundSynthId}", item.Id);

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoundSynthId, item.Id },
            { NavParamKeys.NavigationSource, nameof(SoundSynthsListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoundSynthDetailPage), parameters);

        return Task.CompletedTask;
    }
}