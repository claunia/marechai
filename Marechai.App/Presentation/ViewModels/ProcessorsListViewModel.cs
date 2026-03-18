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

/// <summary>
///     ViewModel for displaying a list of Processors
/// </summary>
public partial class ProcessorsListViewModel : ObservableObject
{
    private readonly IStringLocalizer                 _localizer;
    private readonly ILogger<ProcessorsListViewModel> _logger;
    private readonly IRegionManager                   _regionManager;
    private readonly ProcessorsService                _processorsService;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _pageTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProcessorListItem> _processorsList = [];

    public ProcessorsListViewModel(ProcessorsService                processorsService, IStringLocalizer localizer,
                                   ILogger<ProcessorsListViewModel> logger,            IRegionManager   regionManager)
    {
        _processorsService         = processorsService;
        _localizer                 = localizer;
        _logger                    = logger;
        _regionManager             = regionManager;
        LoadData                   = new AsyncRelayCommand(LoadDataAsync);
        NavigateToProcessorCommand = new AsyncRelayCommand<ProcessorListItem>(NavigateToProcessorAsync);
    }

    public IAsyncRelayCommand                    LoadData                   { get; }
    public IAsyncRelayCommand<ProcessorListItem> NavigateToProcessorCommand { get; }

    /// <summary>
    ///     Loads all Processors and sorts them alphabetically
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            ProcessorsList.Clear();

            _logger.LogInformation("LoadDataAsync called for Processors");

            PageTitle = _localizer["Processors"];

            // Load Processors from the API
            await LoadProcessorsFromApiAsync();

            _logger.LogInformation("LoadProcessorsFromApiAsync completed. ProcessorsList.Count={Count}",
                                   ProcessorsList.Count);

            if(ProcessorsList.Count == 0)
            {
                ErrorMessage = _localizer["No processors found"].Value;
                HasError     = true;

                _logger.LogWarning("No Processors found");
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading Processors: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load processors. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Loads Processors from the API and sorts them alphabetically
    /// </summary>
    private async Task LoadProcessorsFromApiAsync()
    {
        try
        {
            List<ProcessorDto> processors = await _processorsService.GetAllProcessorsAsync();

            if(processors == null || processors.Count == 0)
            {
                _logger.LogInformation("No Processors returned from API");

                return;
            }

            var processorItems = new List<ProcessorListItem>();

            foreach(ProcessorDto processor in processors)
            {
                var processorItem = new ProcessorListItem
                {
                    Id      = processor.Id      ?? 0,
                    Name    = processor.Name    ?? string.Empty,
                    Company = processor.Company ?? string.Empty
                };

                processorItems.Add(processorItem);

                _logger.LogInformation("Processor: {Name}, Company: {Company}, ID: {Id}",
                                       processor.Name,
                                       processor.Company,
                                       processor.Id);
            }

            // Sort processors alphabetically
            processorItems.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            // Add all processors to the list
            foreach(ProcessorListItem processor in processorItems) ProcessorsList.Add(processor);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading Processors from API");
        }
    }

    /// <summary>
    ///     Navigates to the Processor detail view
    /// </summary>
    private Task NavigateToProcessorAsync(ProcessorListItem? processor)
    {
        if(processor is null) return Task.CompletedTask;

        _logger.LogInformation("Navigating to Processor detail: {ProcessorName} (ID: {ProcessorId})",
                               processor.Name,
                               processor.Id);

        var parameters = new NavigationParameters
        {
            { NavParamKeys.ProcessorId, processor.Id },
            { NavParamKeys.NavigationSource, nameof(ProcessorsListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(ProcessorDetailPage), parameters);

        return Task.CompletedTask;
    }
}