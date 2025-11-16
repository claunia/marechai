#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Presentation.Models;
using Marechai.App.Services;
using Uno.Extensions.Navigation;

namespace Marechai.App.Presentation.ViewModels;

public partial class SoundSynthsListViewModel : ObservableObject
{
    private readonly ILogger<SoundSynthsListViewModel> _logger;
    private readonly INavigator                        _navigator;
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

    public SoundSynthsListViewModel(SoundSynthsService                soundSynthsService, INavigator navigator,
                                    ILogger<SoundSynthsListViewModel> logger)
    {
        _soundSynthsService         = soundSynthsService;
        _navigator                  = navigator;
        _logger                     = logger;
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

            SoundSynths = new ObservableCollection<SoundSynthListItem>(soundSynths.Select(ss => new SoundSynthListItem
                                                                           {
                                                                               Id      = ss.Id      ?? 0,
                                                                               Name    = ss.Name    ?? "Unknown",
                                                                               Company = ss.Company ?? "Unknown"
                                                                           })
                                                                          .OrderBy(ss => ss.Company)
                                                                          .ThenBy(ss => ss.Name));

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

    private async Task NavigateToSoundSynthAsync(SoundSynthListItem? item)
    {
        if(item == null) return;

        _logger.LogInformation("Navigating to Sound Synthesizer {SoundSynthId}", item.Id);

        await _navigator.NavigateViewModelAsync<SoundSynthDetailViewModel>(this,
                                                                           data: new SoundSynthDetailNavigationParameter
                                                                           {
                                                                               SoundSynthId     = item.Id,
                                                                               NavigationSource = this
                                                                           });
    }

    public class SoundSynthListItem
    {
        public int     Id      { get; set; }
        public string  Name    { get; set; } = string.Empty;
        public string? Company { get; set; }
    }
}