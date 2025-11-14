using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Marechai.App.Services;
using Marechai.Data;

namespace Marechai.App.Presentation;

/// <summary>
///     Wrapper for NewsDto with generated display text
/// </summary>
public class NewsItemViewModel
{
    public required NewsDto News        { get; init; }
    public required string  DisplayText { get; init; }
}

public partial class NewsViewModel : ObservableObject
{
    private readonly IStringLocalizer       _localizer;
    private readonly ILogger<NewsViewModel> _logger;
    private readonly NewsService            _newsService;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private ObservableCollection<NewsItemViewModel> newsList = new();

    public NewsViewModel(NewsService newsService, IStringLocalizer localizer, ILogger<NewsViewModel> logger)
    {
        _newsService = newsService;
        _localizer   = localizer;
        _logger      = logger;
        LoadNews     = new AsyncRelayCommand(LoadNewsAsync);
    }

    public IAsyncRelayCommand LoadNews { get; }

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
                        News        = item,
                        DisplayText = GetLocalizedTextForNewsType((NewsType)(item.Type ?? 0))
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