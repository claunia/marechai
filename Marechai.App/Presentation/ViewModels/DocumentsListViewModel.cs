#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class DocumentsListViewModel : ObservableObject, IRegionAware
{
    private readonly DocumentsService                _documentsService;
    private readonly IDocumentsListFilterContext      _filterContext;
    private readonly IStringLocalizer                _localizer;
    private readonly ILogger<DocumentsListViewModel> _logger;
    private readonly IRegionManager                  _regionManager;

    [ObservableProperty]
    private ObservableCollection<DocumentListItem> _documentsList = [];

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _filterDescription = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _pageTitle = string.Empty;

    public DocumentsListViewModel(DocumentsService                documentsService, IStringLocalizer localizer,
                                  ILogger<DocumentsListViewModel> logger,           IRegionManager   regionManager,
                                  IDocumentsListFilterContext      filterContext)
    {
        _documentsService   = documentsService;
        _localizer          = localizer;
        _logger             = logger;
        _regionManager      = regionManager;
        _filterContext      = filterContext;
        LoadData              = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand         = new AsyncRelayCommand(GoBackAsync);
        NavigateToDocumentCommand = new AsyncRelayCommand<DocumentListItem>(NavigateToDocumentAsync);
    }

    public IAsyncRelayCommand                    LoadData                  { get; }
    public ICommand                              GoBackCommand             { get; }
    public IAsyncRelayCommand<DocumentListItem>  NavigateToDocumentCommand { get; }

    public DocumentListFilterType FilterType
    {
        get => _filterContext.FilterType;
        set => _filterContext.FilterType = value;
    }

    public string FilterValue
    {
        get => _filterContext.FilterValue;
        set => _filterContext.FilterValue = value;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            DocumentsList.Clear();

            UpdateFilterDescription();

            await LoadDocumentsFromApiAsync();

            if(DocumentsList.Count == 0)
            {
                ErrorMessage = _localizer["No documents found for this filter"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading documents: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load documents. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateFilterDescription()
    {
        switch(FilterType)
        {
            case DocumentListFilterType.All:
                PageTitle         = _localizer["All Documents"];
                FilterDescription = _localizer["Browsing all documents in the database"];

                break;

            case DocumentListFilterType.Letter:
                if(!string.IsNullOrEmpty(FilterValue) && FilterValue.Length == 1)
                {
                    PageTitle         = $"{_localizer["Documents Starting with"]} {FilterValue}";
                    FilterDescription = $"{_localizer["Showing documents that start with"]} {FilterValue}";
                }

                break;

            case DocumentListFilterType.Year:
                if(!string.IsNullOrEmpty(FilterValue) && int.TryParse(FilterValue, out int year))
                {
                    PageTitle         = $"{_localizer["Documents from"]} {year}";
                    FilterDescription = $"{_localizer["Showing documents published in"]} {year}";
                }

                break;
        }
    }

    private async Task LoadDocumentsFromApiAsync()
    {
        try
        {
            List<DocumentDto> documents = FilterType switch
                                          {
                                              DocumentListFilterType.Letter when FilterValue.Length == 1 =>
                                                  await _documentsService.GetDocumentsByLetterAsync(FilterValue[0]),

                                              DocumentListFilterType.Year when int.TryParse(FilterValue, out int year)
                                                  => await _documentsService.GetDocumentsByYearAsync(year),

                                              _ => await _documentsService.GetAllDocumentsAsync()
                                          };

            foreach(DocumentDto doc in documents)
            {
                long id   = doc.Id ?? 0;
                int? year = doc.Published?.Year;

                DocumentsList.Add(new DocumentListItem
                {
                    Id    = id,
                    Title = doc.Title ?? string.Empty,
                    Year  = year
                });
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading documents from API");
        }
    }

    private Task GoBackAsync()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        _regionManager.RequestNavigate(RegionNames.Content, nameof(DocumentsPage));

        return Task.CompletedTask;
    }

    private Task NavigateToDocumentAsync(DocumentListItem? document)
    {
        if(document is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.DocumentId, document.Id },
            { NavParamKeys.NavigationSource, nameof(DocumentsListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(DocumentViewPage), parameters);

        return Task.CompletedTask;
    }
}
