using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.Data;

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
    ///     Determines if this news item can be navigated to
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
                        or NewsType.UpdatedConsoleInCollection
                        or NewsType.NewBookInDb
                        or NewsType.UpdatedBookInDb
                        or NewsType.NewDocumentInDb
                        or NewsType.UpdatedDocumentInDb
                        or NewsType.NewMagazineInDb
                        or NewsType.UpdatedMagazineInDb
                        or NewsType.NewPersonInDb
                        or NewsType.UpdatedPersonInDb
                        or NewsType.NewSoftwareInDb
                        or NewsType.UpdatedSoftwareInDb
                        or NewsType.NewSoftwareVersionInDb
                        or NewsType.UpdatedSoftwareVersionInDb
                        or NewsType.NewSoftwareReleaseInDb
                        or NewsType.UpdatedSoftwareReleaseInDb
                        or NewsType.NewGpuInDb
                        or NewsType.UpdatedGpuInDb
                        or NewsType.NewSoundSynthInDb
                        or NewsType.UpdatedSoundSynthInDb
                        or NewsType.NewProcessorInDb
                        or NewsType.UpdatedProcessorInDb;
        }
    }
}

public partial class NewsViewModel : ObservableObject
{
    private readonly IStringLocalizer       _localizer;
    private readonly ILogger<NewsViewModel> _logger;
    private readonly IRegionManager         _regionManager;
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
                         IRegionManager regionManager)
    {
        _newsService   = newsService;
        _localizer     = localizer;
        _logger        = logger;
        _regionManager = regionManager;
        LoadNews       = new AsyncRelayCommand(LoadNewsAsync);
    }

    public IAsyncRelayCommand LoadNews { get; }

    [RelayCommand]
    private async Task NavigateToNewsItem(NewsDto news)
    {
        if(news?.Type is null) return;

        var newsType = (NewsType)news.Type.Value;

        if(news.AffectedId is null or <= 0) return;

        int    affectedId = (int)(news.AffectedId ?? 0);
        string viewName;
        var    parameters = new NavigationParameters { { NavParamKeys.NavigationSource, nameof(NewsViewModel) } };

        switch(newsType)
        {
            case NewsType.NewComputerInDb or NewsType.UpdatedComputerInDb
                or NewsType.NewComputerInCollection or NewsType.UpdatedComputerInCollection
                or NewsType.NewConsoleInDb or NewsType.UpdatedConsoleInDb
                or NewsType.NewConsoleInCollection or NewsType.UpdatedConsoleInCollection:
                viewName = nameof(MachineViewPage);
                parameters.Add(NavParamKeys.MachineId, affectedId);

                break;

            case NewsType.NewBookInDb or NewsType.UpdatedBookInDb:
                viewName = nameof(BookViewPage);
                parameters.Add(NavParamKeys.BookId, affectedId);

                break;

            case NewsType.NewDocumentInDb or NewsType.UpdatedDocumentInDb:
                viewName = nameof(DocumentViewPage);
                parameters.Add(NavParamKeys.DocumentId, affectedId);

                break;

            case NewsType.NewMagazineInDb or NewsType.UpdatedMagazineInDb:
                viewName = nameof(MagazineViewPage);
                parameters.Add(NavParamKeys.MagazineId, affectedId);

                break;

            case NewsType.NewPersonInDb or NewsType.UpdatedPersonInDb:
                viewName = nameof(PersonViewPage);
                parameters.Add(NavParamKeys.PersonId, affectedId);

                break;

            case NewsType.NewSoftwareInDb or NewsType.UpdatedSoftwareInDb
                or NewsType.NewSoftwareVersionInDb or NewsType.UpdatedSoftwareVersionInDb:
                viewName = nameof(SoftwareViewPage);
                parameters.Add(NavParamKeys.SoftwareId, affectedId);

                break;

            case NewsType.NewSoftwareReleaseInDb or NewsType.UpdatedSoftwareReleaseInDb:
                viewName = nameof(SoftwareReleaseViewPage);
                parameters.Add(NavParamKeys.SoftwareReleaseId, affectedId);

                break;

            case NewsType.NewGpuInDb or NewsType.UpdatedGpuInDb:
                viewName = nameof(GpuDetailPage);
                parameters.Add(NavParamKeys.GpuId, affectedId);

                break;

            case NewsType.NewSoundSynthInDb or NewsType.UpdatedSoundSynthInDb:
                viewName = nameof(SoundSynthDetailPage);
                parameters.Add(NavParamKeys.SoundSynthId, affectedId);

                break;

            case NewsType.NewProcessorInDb or NewsType.UpdatedProcessorInDb:
                viewName = nameof(ProcessorDetailPage);
                parameters.Add(NavParamKeys.ProcessorId, affectedId);

                break;

            default:
                return;
        }

        _regionManager.RequestNavigate(RegionNames.Content, viewName, parameters);
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
                   NewsType.NewBookInDb                 => _localizer["New book in database"].Value,
                   NewsType.UpdatedBookInDb             => _localizer["Updated book in database"].Value,
                   NewsType.NewDocumentInDb             => _localizer["New document in database"].Value,
                   NewsType.UpdatedDocumentInDb         => _localizer["Updated document in database"].Value,
                   NewsType.NewMagazineInDb             => _localizer["New magazine in database"].Value,
                   NewsType.UpdatedMagazineInDb         => _localizer["Updated magazine in database"].Value,
                   NewsType.NewPersonInDb               => _localizer["New person in database"].Value,
                   NewsType.UpdatedPersonInDb           => _localizer["Updated person in database"].Value,
                   NewsType.NewSoftwareInDb             => _localizer["New software in database"].Value,
                   NewsType.UpdatedSoftwareInDb         => _localizer["Updated software in database"].Value,
                   NewsType.NewSoftwareVersionInDb      => _localizer["New software version in database"].Value,
                   NewsType.UpdatedSoftwareVersionInDb  => _localizer["Updated software version in database"].Value,
                   NewsType.NewSoftwareReleaseInDb      => _localizer["New software release in database"].Value,
                   NewsType.UpdatedSoftwareReleaseInDb  => _localizer["Updated software release in database"].Value,
                   NewsType.NewGpuInDb                  => _localizer["New GPU in database"].Value,
                   NewsType.UpdatedGpuInDb              => _localizer["Updated GPU in database"].Value,
                   NewsType.NewSoundSynthInDb           => _localizer["New sound synthesizer in database"].Value,
                   NewsType.UpdatedSoundSynthInDb       => _localizer["Updated sound synthesizer in database"].Value,
                   NewsType.NewProcessorInDb            => _localizer["New processor in database"].Value,
                   NewsType.UpdatedProcessorInDb        => _localizer["Updated processor in database"].Value,
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