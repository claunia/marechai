#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Marechai.App.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.Data;
using Microsoft.UI.Dispatching;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoundSynthsViewModel : ObservableObject, IRegionAware
{
    private readonly Client                         _apiClient;
    private readonly IJwtService                    _jwtService;
    private readonly IStringLocalizer               _localizer;
    private readonly ILogger<AdminSoundSynthsViewModel> _logger;
    private readonly IRegionManager                 _regionManager;
    private readonly SoundSynthsService             _soundSynthsService;
    private readonly ITokenService                  _tokenService;
    private readonly DispatcherQueue?               _dispatcherQueue;

    [ObservableProperty] private ObservableCollection<SoundSynthDto> _soundSynths = [];
    [ObservableProperty] private ObservableCollection<SoundSynthDto> _filteredSoundSynths = [];
    [ObservableProperty] private List<int> _pageSizeOptions = [10, 25, 50, 100];
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _pageSize = 25;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private SoundSynthDto? _selectedSoundSynth;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isEditingDescription;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    private int? _editingId;

    // --- Description panel state ---
    [ObservableProperty] private string _descriptionMarkdown = string.Empty;
    [ObservableProperty] private int? _descriptionSoundSynthId;
    [ObservableProperty] private ObservableCollection<LanguageItem> _availableLanguages = [];
    [ObservableProperty] private LanguageItem? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<SoundSynthDescriptionDto> _existingTranslations = [];

    public bool CanSaveDescription =>
        DescriptionSoundSynthId.HasValue &&
        SelectedLanguage is not null &&
        !string.IsNullOrWhiteSpace(DescriptionMarkdown);

    // --- Form fields ---
    [ObservableProperty] private string _synthName = string.Empty;
    [ObservableProperty] private string _modelCode = string.Empty;
    [ObservableProperty] private DateTimeOffset? _introduced;
    [ObservableProperty] private int _introducedPrecision;
    [ObservableProperty] private int? _voices;
    [ObservableProperty] private double? _frequency;
    [ObservableProperty] private int? _depth;
    [ObservableProperty] private int? _squareWave;
    [ObservableProperty] private int? _whiteNoise;
    [ObservableProperty] private int? _synthType;

    public List<string> SoundSynthTypeItems { get; } = Enum.GetValues<SoundSynthType>().Select(e => e.Humanize()).ToList();

    public int SynthTypeIndex
    {
        get => SynthType ?? -1;
        set
        {
            SynthType = value >= 0 ? value : null;
            OnPropertyChanged();
        }
    }

    // --- Company picker ---
    [ObservableProperty] private CompanyDto? _selectedCompany;
    [ObservableProperty] private string _companySearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyDto> _companySuggestions = [];
    private List<CompanyDto>? _allCompanies;

    public AdminSoundSynthsViewModel(Client                         apiClient,
                                     IJwtService                    jwtService,
                                     ITokenService                  tokenService,
                                     ILogger<AdminSoundSynthsViewModel> logger,
                                     IStringLocalizer               localizer,
                                     IRegionManager                 regionManager,
                                     SoundSynthsService             soundSynthsService)
    {
        _apiClient          = apiClient;
        _jwtService         = jwtService;
        _tokenService       = tokenService;
        _logger             = logger;
        _localizer          = localizer;
        _regionManager      = regionManager;
        _soundSynthsService = soundSynthsService;
        _dispatcherQueue    = DispatcherQueue.GetForCurrentThread();

        LoadItemsCommand         = new AsyncRelayCommand(LoadItemsAsync);
        OpenAddCommand           = new RelayCommand(OpenAdd);
        OpenEditCommand          = new RelayCommand<SoundSynthDto>(OpenEdit);
        OpenDescriptionCommand   = new RelayCommand<SoundSynthDto>(OpenDescription);
        OpenPhotosCommand        = new RelayCommand<SoundSynthDto>(OpenPhotos);
        OpenVideosCommand        = new RelayCommand<SoundSynthDto>(OpenVideos);
        DeleteCommand            = new AsyncRelayCommand<SoundSynthDto>(DeleteAsync);
        NextPageCommand          = new AsyncRelayCommand(NextPageAsync);
        PreviousPageCommand      = new AsyncRelayCommand(PreviousPageAsync);
        SaveCommand              = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand        = new RelayCommand(CancelEdit);
        SaveDescriptionCommand   = new AsyncRelayCommand(SaveDescriptionAsync);
        CancelDescriptionCommand = new RelayCommand(CancelDescription);
        DeleteTranslationCommand = new AsyncRelayCommand<SoundSynthDescriptionDto>(DeleteTranslationAsync);
        EditTranslationCommand   = new RelayCommand<SoundSynthDescriptionDto>(EditTranslation);

        InitializeLanguages();
        CheckAdminRole();
    }

    public IAsyncRelayCommand LoadItemsCommand { get; }
    public IRelayCommand OpenAddCommand { get; }
    public IRelayCommand<SoundSynthDto> OpenEditCommand { get; }
    public IRelayCommand<SoundSynthDto> OpenDescriptionCommand { get; }
    public IRelayCommand<SoundSynthDto> OpenPhotosCommand { get; }
    public IRelayCommand<SoundSynthDto> OpenVideosCommand { get; }
    public IAsyncRelayCommand<SoundSynthDto> DeleteCommand { get; }
    public IAsyncRelayCommand NextPageCommand { get; }
    public IAsyncRelayCommand PreviousPageCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelEditCommand { get; }
    public IAsyncRelayCommand SaveDescriptionCommand { get; }
    public IRelayCommand CancelDescriptionCommand { get; }
    public IAsyncRelayCommand<SoundSynthDescriptionDto> DeleteTranslationCommand { get; }
    public IRelayCommand<SoundSynthDescriptionDto> EditTranslationCommand { get; }
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage * PageSize < TotalCount;
    public string PageSummary => TotalCount == 0
                                     ? "0 items"
                                     : $"{((CurrentPage - 1) * PageSize) + 1}-{Math.Min(CurrentPage * PageSize, TotalCount)} of {TotalCount}";

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
        {
            _ = LoadItemsCommand.ExecuteAsync(null);
            _ = LoadPickerDataAsync();
        }
    }

    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(string.IsNullOrWhiteSpace(token))
            {
                IsAdmin = false;
                return;
            }

            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("Uberadmin",      StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch { IsAdmin = false; }
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true; HasError = false; ErrorMessage = string.Empty;
            SoundSynths.Clear();
            string? filter = NullIfWhiteSpace(FilterText);
            TotalCount = await _apiClient.SoundSynths.Count.GetAsync(config =>
            {
                if(filter != null)
                    config.QueryParameters.Filters = [filter];
            }) ?? 0;
            int maxPage = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            if(CurrentPage > maxPage)
                CurrentPage = maxPage;
            int skip = (CurrentPage - 1) * PageSize;
            List<SoundSynthDto>? response = await _apiClient.SoundSynths.GetAsync(config =>
            {
                config.QueryParameters.Skip = skip;
                config.QueryParameters.Take = PageSize;
                if(filter != null)
                    config.QueryParameters.Filters = [filter];
            });

            if(response != null)
                foreach(SoundSynthDto item in response)
                    SoundSynths.Add(item);

            ReplaceFilteredSoundSynths(SoundSynths);
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading sound synths");
            ErrorMessage = _localizer["FailedToLoadSoundSynths"];
            HasError     = true;
        }
        finally { IsLoading = false; }
    }

    private void OpenAdd()
    {
        _editingId     = null;
        EditPanelTitle = _localizer["AddSoundSynthDialog_Title"];
        ClearForm();
        IsEditingDescription = false;
        IsEditing = true;
    }

    private async void OpenEdit(SoundSynthDto? item)
    {
        if(item?.Id == null) return;

        try
        {
            SoundSynthDto? full = await _apiClient.SoundSynths[item.Id.Value].GetAsync();

            if(full == null) return;

            _editingId     = item.Id;
            EditPanelTitle = _localizer["EditSoundSynthDialog_Title"];
            IsEditingDescription = false;
            PopulateForm(full);
            IsEditing = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading sound synth details for {Id}", item.Id);
            ErrorMessage = _localizer["FailedToLoadSoundSynths"];
            HasError     = true;
        }
    }

    private void OpenPhotos(SoundSynthDto? item)
    {
        if(item?.Id == null) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoundSynthId, item.Id.Value },
            { NavParamKeys.SoundSynthName, item.Name ?? string.Empty }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoundSynthPhotosPage), parameters);
    }

    private void OpenVideos(SoundSynthDto? item)
    {
        if(item?.Id == null) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoundSynthId, item.Id.Value },
            { NavParamKeys.SoundSynthName, item.Name ?? string.Empty }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminSoundSynthVideosPage), parameters);
    }

    private async Task DeleteAsync(SoundSynthDto? item)
    {
        if(item?.Id == null) return;

        try
        {
            await _apiClient.SoundSynths[item.Id.Value].DeleteAsync();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting sound synth {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteSoundSynth"];
            HasError     = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(SynthName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError     = true;
                return;
            }

            var dto = new SoundSynthDto
            {
                Name       = SynthName,
                CompanyId  = SelectedCompany?.Id,
                ModelCode  = string.IsNullOrWhiteSpace(ModelCode) ? null : ModelCode,
                Introduced = Introduced,
                IntroducedPrecision = IntroducedPrecision,
                Voices     = Voices,
                Frequency  = Frequency,
                Depth      = Depth,
                SquareWave = SquareWave,
                WhiteNoise = WhiteNoise,
                Type       = SynthType
            };

            if(_editingId == null)
                await _apiClient.SoundSynths.PostAsync(dto);
            else
            {
                dto.Id = _editingId;
                await _apiClient.SoundSynths[_editingId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving sound synth");
            ErrorMessage = _localizer["FailedToSaveSoundSynth"];
            HasError     = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing  = false;
        _editingId = null;
        ClearForm();
        HasError     = false;
        ErrorMessage = string.Empty;
    }

    private void InitializeLanguages()
    {
        AvailableLanguages =
        [
            new LanguageItem { Code = "eng", DisplayName = "English" },
            new LanguageItem { Code = "spa", DisplayName = "Español" },
            new LanguageItem { Code = "deu", DisplayName = "Deutsch" },
            new LanguageItem { Code = "fra", DisplayName = "Français" },
            new LanguageItem { Code = "lat", DisplayName = "Latina" },
            new LanguageItem { Code = "por", DisplayName = "Português (Brasil)" }
        ];

        SelectedLanguage = AvailableLanguages[0];
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        if(value == null || ExistingTranslations.Count == 0)
        {
            DescriptionMarkdown = string.Empty;
            OnPropertyChanged(nameof(CanSaveDescription));

            return;
        }

        SoundSynthDescriptionDto? existing = ExistingTranslations.FirstOrDefault(t => t.LanguageCode == value.Code);
        DescriptionMarkdown = existing?.Markdown ?? string.Empty;
        OnPropertyChanged(nameof(CanSaveDescription));
    }

    partial void OnDescriptionMarkdownChanged(string value) => OnPropertyChanged(nameof(CanSaveDescription));

    partial void OnDescriptionSoundSynthIdChanged(int? value) => OnPropertyChanged(nameof(CanSaveDescription));

    public void ApplyFilter()
    {
        _ = ReloadFromFirstPageAsync();
    }

    public void UpdateCompanySuggestions(string query)
    {
        CompanySuggestions.Clear();

        if(_allCompanies == null) return;

        IEnumerable<CompanyDto> source = _allCompanies;

        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(c => c.Name != null &&
                                       c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach(CompanyDto match in source)
            CompanySuggestions.Add(match);
    }

    public async Task LoadPickerDataAsync()
    {
        try { _allCompanies = await _apiClient.Companies.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading companies for picker"); }
    }

    private void OpenDescription(SoundSynthDto? item) => _ = OpenDescriptionAsync(item);

    private async Task OpenDescriptionAsync(SoundSynthDto? item)
    {
        if(item?.Id == null) return;

        try
        {
            await RunOnUiThreadAsync(() =>
            {
                HasError                = false;
                ErrorMessage            = string.Empty;
                DescriptionSoundSynthId = item.Id;
                DescriptionMarkdown     = string.Empty;
                IsEditing               = false;
                ExistingTranslations.Clear();
            });
            await ReloadDescriptionTranslationsAsync(item.Id.Value);
            await RunOnUiThreadAsync(() =>
            {
                SelectedLanguage     = GetDefaultDescriptionLanguage();
                IsEditingDescription = true;
            });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading descriptions for sound synth {Id}", item.Id);
            await RunOnUiThreadAsync(() =>
            {
                DescriptionMarkdown  = string.Empty;
                IsEditingDescription = true;
            });
        }
    }

    private async Task ReloadDescriptionTranslationsAsync(int soundSynthId)
    {
        List<SoundSynthDescriptionDto> translations = await _soundSynthsService.GetDescriptionsAsync(soundSynthId);

        await RunOnUiThreadAsync(() =>
        {
            ExistingTranslations.Clear();

            foreach(SoundSynthDescriptionDto translation in translations.OrderBy(t => GetLanguageDisplayName(t.LanguageCode)))
            {
                translation.Language ??= GetLanguageDisplayName(translation.LanguageCode);
                ExistingTranslations.Add(translation);
            }
        });
    }

    private Task RunOnUiThreadAsync(Action action)
    {
        if(_dispatcherQueue is null || _dispatcherQueue.HasThreadAccess)
        {
            action();

            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();

        if(!_dispatcherQueue.TryEnqueue(() =>
           {
               try
               {
                   action();
                   tcs.SetResult();
               }
               catch(Exception ex)
               {
                   tcs.SetException(ex);
               }
           }))
            tcs.SetException(new InvalidOperationException("Unable to enqueue work on the UI thread."));

        return tcs.Task;
    }

    private LanguageItem GetDefaultDescriptionLanguage()
    {
        return AvailableLanguages.FirstOrDefault(language =>
                   ExistingTranslations.All(translation => translation.LanguageCode != language.Code)) ??
               AvailableLanguages.FirstOrDefault(language => language.Code == "eng") ??
               AvailableLanguages.First();
    }

    private string GetLanguageDisplayName(string? languageCode)
    {
        if(string.IsNullOrWhiteSpace(languageCode)) return string.Empty;

        return AvailableLanguages.FirstOrDefault(language => language.Code == languageCode)?.DisplayName ??
               ExistingTranslations.FirstOrDefault(translation => translation.LanguageCode == languageCode)?.Language ??
               languageCode;
    }

    private async Task SaveDescriptionAsync()
    {
        if(!CanSaveDescription || DescriptionSoundSynthId == null || SelectedLanguage == null) return;

        try
        {
            var dto = new SoundSynthDescriptionDto
            {
                SoundSynthId = DescriptionSoundSynthId.Value,
                Markdown     = DescriptionMarkdown,
                LanguageCode = SelectedLanguage.Code
            };

            (bool succeeded, string? error) =
                await _soundSynthsService.CreateOrUpdateDescriptionAsync(DescriptionSoundSynthId.Value, dto);

            if(!succeeded)
            {
                ErrorMessage = error ?? _localizer["FailedToSaveDescription"];
                HasError     = true;

                return;
            }

            await ReloadDescriptionTranslationsAsync(DescriptionSoundSynthId.Value);
            HasError     = false;
            ErrorMessage = string.Empty;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving sound synth description");
            ErrorMessage = _localizer["FailedToSaveDescription"];
            HasError     = true;
        }
    }

    private void EditTranslation(SoundSynthDescriptionDto? translation)
    {
        if(translation?.LanguageCode == null) return;

        SelectedLanguage    = AvailableLanguages.FirstOrDefault(l => l.Code == translation.LanguageCode);
        DescriptionMarkdown = translation.Markdown ?? string.Empty;
    }

    private async Task DeleteTranslationAsync(SoundSynthDescriptionDto? translation)
    {
        if(DescriptionSoundSynthId == null || translation?.LanguageCode == null) return;

        try
        {
            (bool succeeded, string? error) =
                await _soundSynthsService.DeleteDescriptionAsync(DescriptionSoundSynthId.Value, translation.LanguageCode);

            if(!succeeded)
            {
                ErrorMessage = error ?? _localizer["FailedToDeleteTranslation"];
                HasError     = true;

                return;
            }

            await ReloadDescriptionTranslationsAsync(DescriptionSoundSynthId.Value);
            HasError     = false;
            ErrorMessage = string.Empty;

            if(SelectedLanguage?.Code == translation.LanguageCode)
            {
                DescriptionMarkdown = string.Empty;

                LanguageItem nextLanguage = GetDefaultDescriptionLanguage();

                if(SelectedLanguage?.Code == nextLanguage.Code)
                    OnSelectedLanguageChanged(nextLanguage);
                else
                    SelectedLanguage = nextLanguage;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting sound synth description");
            ErrorMessage = _localizer["FailedToDeleteTranslation"];
            HasError     = true;
        }
    }

    private void CancelDescription()
    {
        IsEditingDescription   = false;
        DescriptionSoundSynthId = null;
        DescriptionMarkdown    = string.Empty;
        ExistingTranslations.Clear();
        SelectedLanguage       = AvailableLanguages.FirstOrDefault(language => language.Code == "eng") ??
                                 AvailableLanguages.FirstOrDefault();
        HasError               = false;
        ErrorMessage           = string.Empty;
    }

    private void ClearForm()
    {
        SynthName         = string.Empty;
        ModelCode          = string.Empty;
        Introduced         = null;
        IntroducedPrecision = 0;
        Voices             = null;
        Frequency          = null;
        Depth              = null;
        SquareWave         = null;
        WhiteNoise         = null;
        SynthType          = null;
        SelectedCompany    = null;
        CompanySearchText  = string.Empty;
        HasError           = false;
        ErrorMessage       = string.Empty;
    }

    private void PopulateForm(SoundSynthDto synth)
    {
        SynthName  = synth.Name      ?? string.Empty;
        ModelCode  = synth.ModelCode  ?? string.Empty;
        Introduced = synth.Introduced;
        IntroducedPrecision = synth.IntroducedPrecision ?? 0;
        Voices     = synth.Voices;
        Frequency  = synth.Frequency;
        Depth      = synth.Depth;
        SquareWave = synth.SquareWave;
        WhiteNoise = synth.WhiteNoise;
        SynthType  = synth.Type;

        if(synth.CompanyId.HasValue && _allCompanies != null)
        {
            CompanyDto? company = _allCompanies.FirstOrDefault(c => c.Id == synth.CompanyId.Value);

            if(company != null)
            {
                CompanySearchText = company.Name ?? string.Empty;
                UpdateCompanySuggestions(CompanySearchText);
                SelectedCompany = CompanySuggestions.FirstOrDefault(c => c.Id == company.Id);
            }
            else
            {
                CompanySearchText = synth.Company ?? string.Empty;
                SelectedCompany   = null;
            }
        }
        else
        {
            CompanySearchText = string.Empty;
            SelectedCompany   = null;
        }
    }

    partial void OnCurrentPageChanged(int value) => NotifyPaginationStateChanged();

    partial void OnPageSizeChanged(int value) => NotifyPaginationStateChanged();

    partial void OnTotalCountChanged(int value) => NotifyPaginationStateChanged();

    private async Task NextPageAsync()
    {
        if(!CanGoNext) return;
        CurrentPage++;
        await LoadItemsAsync();
    }

    private async Task PreviousPageAsync()
    {
        if(!CanGoPrevious) return;
        CurrentPage--;
        await LoadItemsAsync();
    }

    private async Task ReloadFromFirstPageAsync()
    {
        CurrentPage = 1;
        await LoadItemsAsync();
    }

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void ReplaceFilteredSoundSynths(IEnumerable<SoundSynthDto> items)
    {
        FilteredSoundSynths.Clear();

        foreach(SoundSynthDto item in items)
            FilteredSoundSynths.Add(item);
    }

    private void NotifyPaginationStateChanged()
    {
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(PageSummary));
    }
}
