using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Marechai.App.Helpers;
using Marechai.App.Presentation.Models;
using Marechai.App.Services;
using Marechai.Data;
using Uno.Extensions.Navigation;

namespace Marechai.App.Presentation.ViewModels;

/// <summary>
///     Wrapper for NewsDto with generated display text
/// </summary>
public class NewsItemViewModel
{
    public required NewsDto                     News                  { get; init; }
    public required string                      DisplayText           { get; init; }
    public required IAsyncRelayCommand<NewsDto> NavigateToItemCommand { get; init; }

    /// <summary>
    ///     Determines if this news item can be navigated to (only computers and consoles)
    /// </summary>
    public bool CanNavigateToItem
    {
        get
        {
            if(News?.Type is null) return false;
            var type = (NewsType)News.Type.Value;

            return type is NewsType.NewComputerInDb
                        or NewsType.NewConsoleInDb
                        or NewsType.UpdatedComputerInDb
                        or NewsType.UpdatedConsoleInDb
                        or NewsType.NewComputerInCollection
                        or NewsType.NewConsoleInCollection
                        or NewsType.UpdatedComputerInCollection
                        or NewsType.UpdatedConsoleInCollection;
        }
    }
}

public partial class NewsViewModel : ObservableObject
{
    private readonly IStringLocalizer       _localizer;
    private readonly ILogger<NewsViewModel> _logger;
    private readonly INavigator             _navigator;
    private readonly NewsService            _newsService;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<NewsItemViewModel> _newsList = [];

    public NewsViewModel(NewsService newsService, IStringLocalizer localizer, ILogger<NewsViewModel> logger,
                         INavigator  navigator)
    {
        _newsService = newsService;
        _localizer   = localizer;
        _logger      = logger;
        _navigator   = navigator;
        LoadNews     = new AsyncRelayCommand(LoadNewsAsync);
    }

    public IAsyncRelayCommand LoadNews { get; }

    [RelayCommand]
    private async Task NavigateToNewsItem(NewsDto news)
    {
        if(news?.Type is null) return;

        var newsType = (NewsType)news.Type.Value;

        // Only navigate for computer and console news items
        bool isComputerOrConsole = newsType is NewsType.NewComputerInDb
                                            or NewsType.NewConsoleInDb
                                            or NewsType.UpdatedComputerInDb
                                            or NewsType.UpdatedConsoleInDb
                                            or NewsType.NewComputerInCollection
                                            or NewsType.NewConsoleInCollection
                                            or NewsType.UpdatedComputerInCollection
                                            or NewsType.UpdatedConsoleInCollection;

        if(!isComputerOrConsole) return;

        // Extract the machine ID from AffectedId
        if(news.AffectedId is null) return;

        int machineId = UntypedNodeExtractor.ExtractInt(news.AffectedId);

        if(machineId <= 0) return;

        // Navigate to machine view with source information
        var navParam = new MachineViewNavigationParameter
        {
            MachineId        = machineId,
            NavigationSource = this
        };

        await _navigator.NavigateViewModelAsync<MachineViewViewModel>(this, data: navParam);
    }

    /// <summary>
    ///     Helper to extract int from UntypedNode
    /// </summary>
    /// <summary>
    ///     Generates localized text based on NewsType
    /// </summary>
    private string GetLocalizedTextForNewsType(NewsType type)
    {
        return type switch
               {
                   NewsType.NewComputerInDb             => _localizer["New computer in database"].Value,
                   NewsType.NewConsoleInDb              => _localizer["New console in database"].Value,
                   NewsType.NewComputerInCollection     => _localizer["New computer in collection"].Value,
                   NewsType.NewConsoleInCollection      => _localizer["New console in collection"].Value,
                   NewsType.UpdatedComputerInDb         => _localizer["Updated computer in database"].Value,
                   NewsType.UpdatedConsoleInDb          => _localizer["Updated console in database"].Value,
                   NewsType.UpdatedComputerInCollection => _localizer["Updated computer in collection"].Value,
                   NewsType.UpdatedConsoleInCollection  => _localizer["Updated console in collection"].Value,
                   _                                    => string.Empty
               };
    }

    /// <summary>
    ///     Loads the latest news from the API
    /// </summary>
    private async Task LoadNewsAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            NewsList.Clear();

            List<NewsDto> news = await _newsService.GetLatestNewsAsync();

            if(news.Count == 0)
            {
                ErrorMessage = _localizer["No news available"].Value;
                HasError     = true;
            }
            else
            {
                foreach(NewsDto item in news)
                {
                    NewsList.Add(new NewsItemViewModel
                    {
                        News                  = item,
                        DisplayText           = GetLocalizedTextForNewsType((NewsType)(item.Type ?? 0)),
                        NavigateToItemCommand = NavigateToNewsItemCommand
                    });
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading news: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load news. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}