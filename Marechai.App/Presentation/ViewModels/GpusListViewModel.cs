#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Marechai.App.Presentation.Models;
using Marechai.App.Services;
using Uno.Extensions.Navigation;

namespace Marechai.App.Presentation.ViewModels;

/// <summary>
///     ViewModel for displaying a list of GPUs
/// </summary>
public partial class GpusListViewModel : ObservableObject
{
    private readonly GpusService                _gpusService;
    private readonly IStringLocalizer           _localizer;
    private readonly ILogger<GpusListViewModel> _logger;
    private readonly INavigator                 _navigator;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<GpuListItem> _gpusList = [];

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _pageTitle = string.Empty;

    public GpusListViewModel(GpusService gpusService, IStringLocalizer localizer, ILogger<GpusListViewModel> logger,
                             INavigator  navigator)
    {
        _gpusService         = gpusService;
        _localizer           = localizer;
        _logger              = logger;
        _navigator           = navigator;
        LoadData             = new AsyncRelayCommand(LoadDataAsync);
        NavigateToGpuCommand = new AsyncRelayCommand<GpuListItem>(NavigateToGpuAsync);
    }

    public IAsyncRelayCommand              LoadData             { get; }
    public IAsyncRelayCommand<GpuListItem> NavigateToGpuCommand { get; }

    /// <summary>
    ///     Loads all GPUs and sorts them with special handling for Framebuffer and Software
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            GpusList.Clear();

            _logger.LogInformation("LoadDataAsync called for GPUs");

            PageTitle = _localizer["GraphicalProcessingUnits"];

            // Load GPUs from the API
            await LoadGpusFromApiAsync();

            _logger.LogInformation("LoadGpusFromApiAsync completed. GpusList.Count={Count}", GpusList.Count);

            if(GpusList.Count == 0)
            {
                ErrorMessage = _localizer["No graphics processing units found"].Value;
                HasError     = true;

                _logger.LogWarning("No GPUs found");
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading GPUs: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load graphics processing units. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     Loads GPUs from the API and sorts them with special handling for Framebuffer and Software
    /// </summary>
    private async Task LoadGpusFromApiAsync()
    {
        try
        {
            List<GpuDto> gpus = await _gpusService.GetAllGpusAsync();

            if(gpus == null || gpus.Count == 0)
            {
                _logger.LogInformation("No GPUs returned from API");

                return;
            }

            // Separate special GPUs from regular ones
            var specialGpus = new List<GpuListItem>();
            var regularGpus = new List<GpuListItem>();

            foreach(GpuDto gpu in gpus)
            {
                string displayName = gpu.Name ?? string.Empty;

                // Replace special database names
                if(displayName == "DB_FRAMEBUFFER")
                    displayName = "Framebuffer";
                else if(displayName == "DB_SOFTWARE")
                    displayName                               = "Software";
                else if(displayName == "DB_NONE") displayName = "None";

                var gpuItem = new GpuListItem
                {
                    Id        = gpu.Id ?? 0,
                    Name      = displayName,
                    Company   = gpu.Company ?? string.Empty,
                    IsSpecial = gpu.Name is "DB_FRAMEBUFFER" or "DB_SOFTWARE" or "DB_NONE"
                };

                if(gpuItem.IsSpecial)
                    specialGpus.Add(gpuItem);
                else
                    regularGpus.Add(gpuItem);

                _logger.LogInformation("GPU: {Name}, Company: {Company}, ID: {Id}, IsSpecial: {IsSpecial}",
                                       displayName,
                                       gpu.Company,
                                       gpu.Id,
                                       gpuItem.IsSpecial);
            }

            // Sort special GPUs: Framebuffer first, then Software, then None
            specialGpus.Sort((a, b) =>
            {
                int orderA = a.Name == "Framebuffer"
                                 ? 0
                                 : a.Name == "Software"
                                     ? 1
                                     : 2;

                int orderB = b.Name == "Framebuffer"
                                 ? 0
                                 : b.Name == "Software"
                                     ? 1
                                     : 2;

                return orderA.CompareTo(orderB);
            });

            // Sort regular GPUs alphabetically
            regularGpus.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            // Add special GPUs first, then regular GPUs
            foreach(GpuListItem gpu in specialGpus) GpusList.Add(gpu);

            foreach(GpuListItem gpu in regularGpus) GpusList.Add(gpu);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading GPUs from API");
        }
    }

    /// <summary>
    ///     Navigates to the GPU detail view
    /// </summary>
    private async Task NavigateToGpuAsync(GpuListItem? gpu)
    {
        if(gpu is null) return;

        _logger.LogInformation("Navigating to GPU detail: {GpuName} (ID: {GpuId})", gpu.Name, gpu.Id);

        // Navigate to GPU detail view with navigation parameter
        var navParam = new GpuDetailNavigationParameter
        {
            GpuId            = gpu.Id,
            NavigationSource = this
        };

        await _navigator.NavigateViewModelAsync<GpuDetailViewModel>(this, data: navParam);
    }
}

/// <summary>
///     Data model for a GPU in the list
/// </summary>
public class GpuListItem
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = string.Empty;
    public string Company   { get; set; } = string.Empty;
    public bool   IsSpecial { get; set; }
}