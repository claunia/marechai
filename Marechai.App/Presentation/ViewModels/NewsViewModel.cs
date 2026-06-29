using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.Data;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Uno.Extensions.Authentication;

namespace Marechai.App.Presentation.ViewModels;

/// <summary>
///     Wrapper for NewsDto with generated display text
/// </summary>
public class NewsItemViewModel
{
    public required NewsDto                     News                  { get; init; }
    public required string                      DisplayText           { get; init; }
    public required string                      ItemNameText          { get; init; }
    public required string                      TimestampText         { get; init; }
    public required string                      StatusGlyph           { get; init; }
    public required string                      EntityGlyph           { get; init; }
    public required SolidColorBrush            StatusBrush          { get; init; }
    public required IAsyncRelayCommand<NewsDto> NavigateToItemCommand { get; init; }
    public bool                                 IsNewItem             { get; init; }

    /// <summary>
    ///     Determines if this news item can be navigated to
    /// </summary>
    public bool CanNavigateToItem { get; init; }
}

public partial class NewsViewModel : ObservableObject
{
    private readonly IStringLocalizer        _localizer;
    private readonly ILogger<NewsViewModel>  _logger;
    private readonly IRegionManager          _regionManager;
    private readonly NewsService             _newsService;
    private readonly IAuthenticationService  _authService;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isAuthenticated;

    [ObservableProperty]
    private ObservableCollection<NewsItemViewModel> _newsList = [];

    public NewsViewModel(NewsService newsService, IStringLocalizer localizer, ILogger<NewsViewModel> logger,
                         IRegionManager regionManager, IAuthenticationService authService)
    {
        _newsService   = newsService;
        _localizer     = localizer;
        _logger        = logger;
        _regionManager = regionManager;
        _authService   = authService;
        LoadNews       = new AsyncRelayCommand(LoadNewsAsync);

        _ = UpdateAuthenticationStateAsync();
    }

    public IAsyncRelayCommand LoadNews { get; }
    public bool HasNewsItems => NewsList.Count > 0;

    private async Task UpdateAuthenticationStateAsync() =>
        IsAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);

    [RelayCommand]
    private void NavigateToComputers() => _regionManager.RequestNavigate(RegionNames.Content, nameof(ComputersPage));

    [RelayCommand]
    private void NavigateToConsoles() => _regionManager.RequestNavigate(RegionNames.Content, nameof(ConsolesPage));

    [RelayCommand]
    private void NavigateToSoftware() => _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwarePage));

    [RelayCommand]
    private void NavigateToAbout() => _regionManager.RequestNavigate(RegionNames.Content, nameof(AboutPage));

    [RelayCommand]
    private void NavigateToSignUp() => _regionManager.RequestNavigate(RegionNames.Content, nameof(RegisterPage));

    [RelayCommand]
    private async Task NavigateToNewsItem(NewsDto news)
    {
        if(!TryCreateNavigation(news, out string? viewName, out NavigationParameters? parameters)) return;

        _regionManager.RequestNavigate(RegionNames.Content, viewName, parameters);
    }

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
                   NewsType.NewMagazineIssueInDb        => _localizer["New magazine issue in database"].Value,
                   NewsType.UpdatedMagazineIssueInDb    => _localizer["Updated magazine issue in database"].Value,
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
                   NewsType.NewSmartphoneInDb           => _localizer["New smartphone in database"].Value,
                   NewsType.UpdatedSmartphoneInDb       => _localizer["Updated smartphone in database"].Value,
                   NewsType.NewSmartphoneInCollection   => _localizer["New smartphone in collection"].Value,
                   NewsType.UpdatedSmartphoneInCollection => _localizer["Updated smartphone in collection"].Value,
                   _                                    => string.Empty
               };
    }

    static bool IsNewNewsType(NewsType type) => type switch
    {
        NewsType.NewComputerInDb             or NewsType.NewConsoleInDb
            or NewsType.NewComputerInCollection or NewsType.NewConsoleInCollection
            or NewsType.NewBookInDb          or NewsType.NewDocumentInDb
            or NewsType.NewMagazineInDb      or NewsType.NewMagazineIssueInDb
            or NewsType.NewPersonInDb        or NewsType.NewSoftwareInDb
            or NewsType.NewSoftwareVersionInDb or NewsType.NewSoftwareReleaseInDb
            or NewsType.NewGpuInDb           or NewsType.NewSoundSynthInDb
            or NewsType.NewProcessorInDb     or NewsType.NewSmartphoneInDb
            or NewsType.NewSmartphoneInCollection => true,
        _ => false
    };

    static string GetStatusGlyph(NewsType type) => IsNewNewsType(type) ? "\uE710" : "\uE895";

    static SolidColorBrush GetStatusBrush(NewsType type) =>
        IsNewNewsType(type)
            ? new SolidColorBrush(Colors.MediumPurple)
            : new SolidColorBrush(Colors.DodgerBlue);

    static string GetEntityGlyph(NewsType type) => type switch
    {
        NewsType.NewComputerInDb or NewsType.UpdatedComputerInDb
            or NewsType.NewComputerInCollection or NewsType.UpdatedComputerInCollection
            or NewsType.NewConsoleInDb or NewsType.UpdatedConsoleInDb
            or NewsType.NewConsoleInCollection or NewsType.UpdatedConsoleInCollection
            => "\uE7F8",
        NewsType.NewSmartphoneInDb or NewsType.UpdatedSmartphoneInDb
            or NewsType.NewSmartphoneInCollection or NewsType.UpdatedSmartphoneInCollection
            => "\uE8EA",
        NewsType.NewBookInDb or NewsType.UpdatedBookInDb
            => "\uE736",
        NewsType.NewDocumentInDb or NewsType.UpdatedDocumentInDb
            => "\uE8A5",
        NewsType.NewMagazineInDb or NewsType.UpdatedMagazineInDb
            => "\uE8F1",
        NewsType.NewMagazineIssueInDb or NewsType.UpdatedMagazineIssueInDb
            => "\uE8A5",
        NewsType.NewPersonInDb or NewsType.UpdatedPersonInDb
            => "\uE77B",
        NewsType.NewSoftwareInDb or NewsType.UpdatedSoftwareInDb
            or NewsType.NewSoftwareVersionInDb or NewsType.UpdatedSoftwareVersionInDb
            or NewsType.NewSoftwareReleaseInDb or NewsType.UpdatedSoftwareReleaseInDb
            => "\uE71D",
        NewsType.NewGpuInDb or NewsType.UpdatedGpuInDb
            => "\uE7F8",
        NewsType.NewSoundSynthInDb or NewsType.UpdatedSoundSynthInDb
            => "\uE189",
        NewsType.NewProcessorInDb or NewsType.UpdatedProcessorInDb
            => "\uE950",
        _ => "\uE8C3"
    };

    static string GetItemName(NewsDto item) =>
        !string.IsNullOrWhiteSpace(item.ItemName) ? item.ItemName! :
        !string.IsNullOrWhiteSpace(item.Name) ? item.Name! :
        !string.IsNullOrWhiteSpace(item.Text) ? item.Text! :
        string.Empty;

    static string GetTimestampText(NewsDto item) =>
        item.Timestamp?.LocalDateTime.ToString("g") ?? string.Empty;

    bool TryCreateNavigation(NewsDto? news, out string viewName, out NavigationParameters parameters)
    {
        viewName   = string.Empty;
        parameters = new NavigationParameters();

        if(news?.Type is null || news.AffectedId is null or <= 0) return false;

        var newsType = (NewsType)news.Type.Value;
        var affectedId = news.AffectedId.Value;

        parameters.Add(NavParamKeys.NavigationSource, nameof(NewsViewModel));

        switch(newsType)
        {
            case NewsType.NewComputerInDb or NewsType.UpdatedComputerInDb
                or NewsType.NewComputerInCollection or NewsType.UpdatedComputerInCollection
                or NewsType.NewConsoleInDb or NewsType.UpdatedConsoleInDb
                or NewsType.NewConsoleInCollection or NewsType.UpdatedConsoleInCollection
                or NewsType.NewSmartphoneInDb or NewsType.UpdatedSmartphoneInDb
                or NewsType.NewSmartphoneInCollection or NewsType.UpdatedSmartphoneInCollection:
                viewName = nameof(MachineViewPage);
                parameters.Add(NavParamKeys.MachineId, (int)affectedId);
                return true;

            case NewsType.NewBookInDb or NewsType.UpdatedBookInDb:
                viewName = nameof(BookViewPage);
                parameters.Add(NavParamKeys.BookId, affectedId);
                return true;

            case NewsType.NewDocumentInDb or NewsType.UpdatedDocumentInDb:
                viewName = nameof(DocumentViewPage);
                parameters.Add(NavParamKeys.DocumentId, affectedId);
                return true;

            case NewsType.NewMagazineInDb or NewsType.UpdatedMagazineInDb:
                viewName = nameof(MagazineViewPage);
                parameters.Add(NavParamKeys.MagazineId, affectedId);
                return true;

            case NewsType.NewMagazineIssueInDb or NewsType.UpdatedMagazineIssueInDb:
                viewName = nameof(MagazineIssueViewPage);
                parameters.Add(NavParamKeys.MagazineIssueId, affectedId);
                return true;

            case NewsType.NewPersonInDb or NewsType.UpdatedPersonInDb:
                viewName = nameof(PersonViewPage);
                parameters.Add(NavParamKeys.PersonId, (int)affectedId);
                return true;

            case NewsType.NewSoftwareInDb or NewsType.UpdatedSoftwareInDb
                or NewsType.NewSoftwareVersionInDb or NewsType.UpdatedSoftwareVersionInDb:
                viewName = nameof(SoftwareViewPage);
                parameters.Add(NavParamKeys.SoftwareId, (int)affectedId);
                return true;

            case NewsType.NewSoftwareReleaseInDb or NewsType.UpdatedSoftwareReleaseInDb:
                viewName = nameof(SoftwareReleaseViewPage);
                parameters.Add(NavParamKeys.SoftwareReleaseId, (int)affectedId);
                return true;

            case NewsType.NewGpuInDb or NewsType.UpdatedGpuInDb:
                viewName = nameof(GpuDetailPage);
                parameters.Add(NavParamKeys.GpuId, (int)affectedId);
                return true;

            case NewsType.NewSoundSynthInDb or NewsType.UpdatedSoundSynthInDb:
                viewName = nameof(SoundSynthDetailPage);
                parameters.Add(NavParamKeys.SoundSynthId, (int)affectedId);
                return true;

            case NewsType.NewProcessorInDb or NewsType.UpdatedProcessorInDb:
                viewName = nameof(ProcessorDetailPage);
                parameters.Add(NavParamKeys.ProcessorId, (int)affectedId);
                return true;

            default:
                return false;
        }
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
            OnPropertyChanged(nameof(HasNewsItems));

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
                    var newsType = (NewsType)(item.Type ?? 0);

                    NewsList.Add(new NewsItemViewModel
                    {
                        News                  = item,
                        DisplayText           = GetLocalizedTextForNewsType(newsType),
                        ItemNameText          = GetItemName(item),
                        TimestampText         = GetTimestampText(item),
                        StatusGlyph           = GetStatusGlyph(newsType),
                        EntityGlyph           = GetEntityGlyph(newsType),
                        StatusBrush           = GetStatusBrush(newsType),
                        IsNewItem             = IsNewNewsType(newsType),
                        CanNavigateToItem     = TryCreateNavigation(item, out _, out _),
                        NavigateToItemCommand = NavigateToNewsItemCommand
                    });
                }
            }

            OnPropertyChanged(nameof(HasNewsItems));
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
