#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Microsoft.UI.Dispatching;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminGpusViewModel : ObservableObject, IRegionAware
{
    private readonly Client                    _apiClient;
    private readonly GpusService               _gpusService;
    private readonly IJwtService                  _jwtService;
    private readonly IStringLocalizer             _localizer;
    private readonly ILogger<AdminGpusViewModel>  _logger;
    private readonly IRegionManager                _regionManager;
    private readonly ITokenService                _tokenService;
    private readonly DispatcherQueue?             _dispatcherQueue;

    // --- List state ---
    [ObservableProperty]
    private ObservableCollection<GpuDto> _gpus = [];

    [ObservableProperty]
    private ObservableCollection<GpuDto> _filteredGpus = [];

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private GpuDto? _selectedGpu;

    private List<GpuDto>? _allGpus;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    // --- Edit panel state ---
    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _editPanelTitle = string.Empty;

    private int? _editingGpuId;

    // --- Description panel state ---
    [ObservableProperty]
    private bool _isEditingDescription;

    [ObservableProperty]
    private string _descriptionMarkdown = string.Empty;

    [ObservableProperty]
    private int? _descriptionGpuId;

    [ObservableProperty]
    private ObservableCollection<LanguageItem> _availableLanguages = [];

    [ObservableProperty]
    private LanguageItem? _selectedLanguage;

    [ObservableProperty]
    private ObservableCollection<GpuDescriptionDto> _existingTranslations = [];

    public bool CanSaveDescription =>
        DescriptionGpuId.HasValue &&
        SelectedLanguage is not null &&
        !string.IsNullOrWhiteSpace(DescriptionMarkdown);

    // --- Form fields ---
    [ObservableProperty]
    private string _gpuName = string.Empty;

    [ObservableProperty]
    private string _modelCode = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _introduced;

    [ObservableProperty]
    private int _introducedPrecision;

    [ObservableProperty]
    private string _package = string.Empty;

    [ObservableProperty]
    private string _process = string.Empty;

    [ObservableProperty]
    private double? _processNm;

    [ObservableProperty]
    private double? _dieSize;

    [ObservableProperty]
    private long? _transistors;

    // --- Company picker ---
    [ObservableProperty]
    private CompanyDto? _selectedCompany;

    [ObservableProperty]
    private string _companySearchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyDto> _companySuggestions = [];

    private List<CompanyDto>? _allCompanies;

    // --- Resolutions by GPU ---
    [ObservableProperty]
    private ObservableCollection<ResolutionByGpuDto> _gpuResolutions = [];

    [ObservableProperty]
    private ObservableCollection<string> _gpuResolutionDisplays = [];

    [ObservableProperty]
    private ObservableCollection<ResolutionDto> _availableResolutions = [];

    [ObservableProperty]
    private ResolutionDto? _selectedAvailableResolution;

    private List<ResolutionDto>? _allResolutions;

    public AdminGpusViewModel(Client                    apiClient,
                              GpusService               gpusService,
                              IJwtService                  jwtService,
                              ITokenService                tokenService,
                              ILogger<AdminGpusViewModel>  logger,
                              IStringLocalizer             localizer,
                              IRegionManager               regionManager)
    {
        _apiClient    = apiClient;
        _gpusService  = gpusService;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;
        _regionManager = regionManager;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        LoadGpusCommand       = new AsyncRelayCommand(LoadGpusAsync);
        OpenAddGpuCommand     = new RelayCommand(OpenAddGpu);
        OpenEditGpuCommand    = new RelayCommand<GpuDto>(OpenEditGpu);
        OpenDescriptionCommand = new RelayCommand<GpuDto>(OpenDescription);
        OpenPhotosCommand     = new RelayCommand<GpuDto>(OpenPhotos);
        OpenVideosCommand     = new RelayCommand<GpuDto>(OpenVideos);
        DeleteGpuCommand      = new AsyncRelayCommand<GpuDto>(DeleteGpuAsync);
        SaveGpuCommand        = new AsyncRelayCommand(SaveGpuAsync);
        CancelEditCommand     = new RelayCommand(CancelEdit);
        AddResolutionCommand  = new AsyncRelayCommand(AddResolutionAsync);
        RemoveResolutionCommand = new AsyncRelayCommand<ResolutionByGpuDto>(RemoveResolutionAsync);
        RemoveResolutionByIndexCommand = new AsyncRelayCommand<string>(RemoveResolutionByDisplayAsync);
        SaveDescriptionCommand = new AsyncRelayCommand(SaveDescriptionAsync);
        CancelDescriptionCommand = new RelayCommand(CancelDescription);
        DeleteTranslationCommand = new AsyncRelayCommand<GpuDescriptionDto>(DeleteTranslationAsync);
        EditTranslationCommand = new RelayCommand<GpuDescriptionDto>(EditTranslation);

        InitializeLanguages();
        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand          LoadGpusCommand    { get; }
    public IRelayCommand               OpenAddGpuCommand  { get; }
    public IRelayCommand<GpuDto>       OpenEditGpuCommand { get; }
    public IRelayCommand<GpuDto>       OpenDescriptionCommand { get; }
    public IRelayCommand<GpuDto>       OpenPhotosCommand { get; }
    public IRelayCommand<GpuDto>       OpenVideosCommand { get; }
    public IAsyncRelayCommand<GpuDto>  DeleteGpuCommand   { get; }
    public IAsyncRelayCommand          SaveGpuCommand     { get; }
    public IRelayCommand               CancelEditCommand  { get; }
    public IAsyncRelayCommand          AddResolutionCommand    { get; }
    public IAsyncRelayCommand<ResolutionByGpuDto> RemoveResolutionCommand { get; }
    public IAsyncRelayCommand<string>  RemoveResolutionByIndexCommand { get; }
    public IAsyncRelayCommand          SaveDescriptionCommand { get; }
    public IRelayCommand               CancelDescriptionCommand { get; }
    public IAsyncRelayCommand<GpuDescriptionDto> DeleteTranslationCommand { get; }
    public IRelayCommand<GpuDescriptionDto> EditTranslationCommand { get; }

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
        {
            _ = LoadGpusCommand.ExecuteAsync(null);
            _ = LoadPickerDataAsync();
        }
    }

    // --- Role check ---
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

            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    // --- Load GPUs ---
    private async Task LoadGpusAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Gpus.Clear();

            List<GpuDto>? response = await _apiClient.Gpus.GetAsync();
            _allGpus = response;

            if(response != null)
                foreach(GpuDto gpu in response)
                    Gpus.Add(gpu);

            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading GPUs");
            ErrorMessage = _localizer["FailedToLoadGpus"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- Add GPU ---
    private void OpenAddGpu()
    {
        _editingGpuId = null;
        EditPanelTitle = _localizer["AddGpuDialog_Title"];
        IsEditingDescription = false;
        ClearForm();
        IsEditing = true;
    }

    private void OpenPhotos(GpuDto? gpu)
    {
        if(gpu?.Id == null) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.GpuId, gpu.Id.Value },
            { NavParamKeys.GpuName, gpu.Name ?? string.Empty }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminGpuPhotosPage), parameters);
    }

    private void OpenVideos(GpuDto? gpu)
    {
        if(gpu?.Id == null) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.GpuId, gpu.Id.Value },
            { NavParamKeys.GpuName, gpu.Name ?? string.Empty }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminGpuVideosPage), parameters);
    }

    // --- Edit GPU ---
    private async void OpenEditGpu(GpuDto? gpu)
    {
        if(gpu?.Id == null) return;

        try
        {
            // Fetch full details (list endpoint returns partial data)
            GpuDto? full = await _apiClient.Gpus[gpu.Id.Value].GetAsync();

            if(full == null) return;

            _editingGpuId  = gpu.Id;
            EditPanelTitle = _localizer["EditGpuDialog_Title"];
            IsEditingDescription = false;
            PopulateForm(full);

            if(gpu.Id.HasValue)
                await LoadGpuResolutionsAsync(gpu.Id.Value);

            IsEditing = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading GPU details for {Id}", gpu.Id);
            ErrorMessage = _localizer["FailedToLoadGpus"];
            HasError     = true;
        }
    }

    // --- Delete GPU ---
    private async Task DeleteGpuAsync(GpuDto? gpu)
    {
        if(gpu?.Id == null) return;

        try
        {
            await _apiClient.Gpus[gpu.Id.Value].DeleteAsync();
            await LoadGpusAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting GPU {Id}", gpu.Id);
            ErrorMessage = _localizer["FailedToDeleteGpu"];
            HasError     = true;
        }
    }

    // --- Save GPU ---
    private async Task SaveGpuAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(GpuName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError     = true;

                return;
            }

            var dto = new GpuDto
            {
                Name        = GpuName,
                CompanyId   = SelectedCompany?.Id,
                ModelCode   = string.IsNullOrWhiteSpace(ModelCode) ? null : ModelCode,
                Introduced  = Introduced,
                IntroducedPrecision = IntroducedPrecision,
                Package     = string.IsNullOrWhiteSpace(Package)   ? null : Package,
                Process     = string.IsNullOrWhiteSpace(Process)   ? null : Process,
                ProcessNm   = ProcessNm.HasValue ? (float)ProcessNm.Value : null,
                DieSize     = DieSize.HasValue   ? (float)DieSize.Value   : null,
                Transistors = Transistors
            };

            if(_editingGpuId == null)
                await _apiClient.Gpus.PostAsync(dto);
            else
            {
                dto.Id = _editingGpuId;
                await _apiClient.Gpus[_editingGpuId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadGpusAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving GPU");
            ErrorMessage = _localizer["FailedToSaveGpu"];
            HasError     = true;
        }
    }

    // --- Cancel edit ---
    private void CancelEdit()
    {
        IsEditing     = false;
        _editingGpuId = null;
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

        GpuDescriptionDto? existing = ExistingTranslations.FirstOrDefault(t => t.LanguageCode == value.Code);
        DescriptionMarkdown = existing?.Markdown ?? string.Empty;
        OnPropertyChanged(nameof(CanSaveDescription));
    }

    partial void OnDescriptionMarkdownChanged(string value) => OnPropertyChanged(nameof(CanSaveDescription));

    partial void OnDescriptionGpuIdChanged(int? value) => OnPropertyChanged(nameof(CanSaveDescription));

    private void OpenDescription(GpuDto? gpu) => _ = OpenDescriptionAsync(gpu);

    private async Task OpenDescriptionAsync(GpuDto? gpu)
    {
        if(gpu?.Id == null) return;

        try
        {
            await RunOnUiThreadAsync(() =>
            {
                HasError            = false;
                ErrorMessage        = string.Empty;
                DescriptionGpuId    = gpu.Id;
                DescriptionMarkdown = string.Empty;
                IsEditing           = false;
                ExistingTranslations.Clear();
            });
            await ReloadDescriptionTranslationsAsync(gpu.Id.Value);
            await RunOnUiThreadAsync(() =>
            {
                SelectedLanguage     = GetDefaultDescriptionLanguage();
                IsEditingDescription = true;
            });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading descriptions for GPU {Id}", gpu.Id);
            await RunOnUiThreadAsync(() =>
            {
                DescriptionMarkdown  = string.Empty;
                IsEditingDescription = true;
            });
        }
    }

    private async Task ReloadDescriptionTranslationsAsync(int gpuId)
    {
        List<GpuDescriptionDto> translations = await _gpusService.GetDescriptionsAsync(gpuId);

        await RunOnUiThreadAsync(() =>
        {
            ExistingTranslations.Clear();

            foreach(GpuDescriptionDto translation in translations.OrderBy(t => GetLanguageDisplayName(t.LanguageCode)))
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
        if(!CanSaveDescription || DescriptionGpuId == null || SelectedLanguage == null) return;

        try
        {
            var dto = new GpuDescriptionDto
            {
                GpuId        = DescriptionGpuId.Value,
                Markdown     = DescriptionMarkdown,
                LanguageCode = SelectedLanguage.Code
            };

            (bool succeeded, string? error) =
                await _gpusService.CreateOrUpdateDescriptionAsync(DescriptionGpuId.Value, dto);

            if(!succeeded)
            {
                ErrorMessage = error ?? _localizer["FailedToSaveDescription"];
                HasError     = true;

                return;
            }

            await ReloadDescriptionTranslationsAsync(DescriptionGpuId.Value);
            HasError     = false;
            ErrorMessage = string.Empty;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving GPU description");
            ErrorMessage = _localizer["FailedToSaveDescription"];
            HasError     = true;
        }
    }

    private void EditTranslation(GpuDescriptionDto? translation)
    {
        if(translation?.LanguageCode == null) return;

        SelectedLanguage    = AvailableLanguages.FirstOrDefault(l => l.Code == translation.LanguageCode);
        DescriptionMarkdown = translation.Markdown ?? string.Empty;
    }

    private async Task DeleteTranslationAsync(GpuDescriptionDto? translation)
    {
        if(DescriptionGpuId == null || translation?.LanguageCode == null) return;

        try
        {
            (bool succeeded, string? error) =
                await _gpusService.DeleteDescriptionAsync(DescriptionGpuId.Value, translation.LanguageCode);

            if(!succeeded)
            {
                ErrorMessage = error ?? _localizer["FailedToDeleteTranslation"];
                HasError     = true;

                return;
            }

            await ReloadDescriptionTranslationsAsync(DescriptionGpuId.Value);
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
            _logger.LogError(ex, "Error deleting GPU description");
            ErrorMessage = _localizer["FailedToDeleteTranslation"];
            HasError     = true;
        }
    }

    private void CancelDescription()
    {
        IsEditingDescription = false;
        DescriptionGpuId     = null;
        DescriptionMarkdown  = string.Empty;
        ExistingTranslations.Clear();
        SelectedLanguage     = AvailableLanguages.FirstOrDefault(language => language.Code == "eng") ??
                               AvailableLanguages.FirstOrDefault();
        HasError             = false;
        ErrorMessage         = string.Empty;
    }

    // --- Filtering ---
    public void ApplyFilter()
    {
        FilteredGpus.Clear();

        IEnumerable<GpuDto> source = (IEnumerable<GpuDto>?)_allGpus ?? Gpus;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(g => (g.Name != null &&
                                        g.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (g.Company != null &&
                                        g.Company.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (g.ModelCode != null &&
                                        g.ModelCode.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));

        foreach(GpuDto gpu in source)
            FilteredGpus.Add(gpu);
    }

    // --- Company search ---
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

    // --- Load picker data ---
    public async Task LoadPickerDataAsync()
    {
        try
        {
            _allCompanies = await _apiClient.Companies.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading companies for picker");
        }

        try
        {
            _allResolutions = await _apiClient.Resolutions.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading resolutions for picker");
        }
    }

    // --- Resolution management ---
    private async Task LoadGpuResolutionsAsync(int gpuId)
    {
        GpuResolutions.Clear();
        GpuResolutionDisplays.Clear();

        try
        {
            List<ResolutionByGpuDto>? rels =
                await _apiClient.ResolutionsByGpu.Gpus[gpuId].Resolutions.GetAsync();

            if(rels != null)
                foreach(ResolutionByGpuDto rel in rels)
                {
                    GpuResolutions.Add(rel);
                    GpuResolutionDisplays.Add(FormatResolutionDisplay(rel));
                }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading resolutions for GPU {Id}", gpuId);
        }

        RefreshAvailableResolutions();
    }

    private string FormatResolutionDisplay(ResolutionByGpuDto rel)
    {
        // The server now returns ResolutionDto directly (no Kiota composed-type
        // wrapper) thanks to [Required] on ResolutionByGpuDto.Resolution. The
        // wire value is still nullable when the join is missing, so guard.
        var res = rel.Resolution;

        if(res != null)
            return $"{res.Width}x{res.Height}" +
                   (res.Colors.HasValue ? $" {res.Colors} colors" : "") +
                   (res.Chars == true ? " (chars)" : "") +
                   (res.Grayscale == true ? " (gray)" : "");

        // Fallback: look up from _allResolutions
        if(rel.ResolutionId.HasValue && _allResolutions != null)
        {
            ResolutionDto? lookup = _allResolutions.FirstOrDefault(r => r.Id == rel.ResolutionId.Value);

            if(lookup != null)
                return $"{lookup.Width}x{lookup.Height}" +
                       (lookup.Colors.HasValue ? $" {lookup.Colors} colors" : "") +
                       (lookup.Chars == true ? " (chars)" : "") +
                       (lookup.Grayscale == true ? " (gray)" : "");
        }

        return $"Resolution #{rel.ResolutionId}";
    }

    private void RefreshAvailableResolutions()
    {
        AvailableResolutions.Clear();

        if(_allResolutions == null) return;

        HashSet<int> assignedIds = new(GpuResolutions
                                     .Where(r => r.ResolutionId.HasValue)
                                     .Select(r => r.ResolutionId!.Value));

        foreach(ResolutionDto res in _allResolutions)
            if(res.Id.HasValue && !assignedIds.Contains(res.Id.Value))
                AvailableResolutions.Add(res);
    }

    private async Task AddResolutionAsync()
    {
        if(_editingGpuId == null || SelectedAvailableResolution?.Id == null) return;

        try
        {
            var dto = new ResolutionByGpuDto
            {
                GpuId        = _editingGpuId.Value,
                ResolutionId = SelectedAvailableResolution.Id.Value
            };

            await _apiClient.ResolutionsByGpu.PostAsync(dto);
            SelectedAvailableResolution = null;
            await LoadGpuResolutionsAsync(_editingGpuId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding resolution to GPU");
            ErrorMessage = _localizer["FailedToSaveGpu"];
            HasError     = true;
        }
    }

    private async Task RemoveResolutionAsync(ResolutionByGpuDto? rel)
    {
        if(rel?.Id == null || _editingGpuId == null) return;

        try
        {
            await _apiClient.ResolutionsByGpu[rel.Id.Value].DeleteAsync();
            await LoadGpuResolutionsAsync(_editingGpuId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing resolution from GPU");
            ErrorMessage = _localizer["FailedToSaveGpu"];
            HasError     = true;
        }
    }

    private async Task RemoveResolutionByDisplayAsync(string? display)
    {
        if(display == null || _editingGpuId == null) return;

        int index = GpuResolutionDisplays.IndexOf(display);

        if(index >= 0 && index < GpuResolutions.Count)
            await RemoveResolutionAsync(GpuResolutions[index]);
    }

    // --- Helpers ---
    private void ClearForm()
    {
        GpuName           = string.Empty;
        ModelCode          = string.Empty;
        Introduced         = null;
        IntroducedPrecision = 0;
        Package            = string.Empty;
        Process            = string.Empty;
        ProcessNm          = null;
        DieSize            = null;
        Transistors        = null;
        SelectedCompany    = null;
        CompanySearchText  = string.Empty;
        GpuResolutions.Clear();
        GpuResolutionDisplays.Clear();
        AvailableResolutions.Clear();
        SelectedAvailableResolution = null;
        HasError           = false;
        ErrorMessage       = string.Empty;
    }

    private void PopulateForm(GpuDto gpu)
    {
        GpuName           = gpu.Name      ?? string.Empty;
        ModelCode         = gpu.ModelCode  ?? string.Empty;
        Introduced        = gpu.Introduced;
        IntroducedPrecision = gpu.IntroducedPrecision ?? 0;
        Package           = gpu.Package   ?? string.Empty;
        Process           = gpu.Process   ?? string.Empty;
        ProcessNm         = gpu.ProcessNm;
        DieSize           = gpu.DieSize;
        Transistors       = gpu.Transistors;

        // Set company via picker
        if(gpu.CompanyId.HasValue && _allCompanies != null)
        {
            CompanyDto? company = _allCompanies.FirstOrDefault(c => c.Id == gpu.CompanyId.Value);

            if(company != null)
            {
                CompanySearchText = company.Name ?? string.Empty;
                UpdateCompanySuggestions(CompanySearchText);
                SelectedCompany = CompanySuggestions.FirstOrDefault(c => c.Id == company.Id);
            }
            else
            {
                CompanySearchText = gpu.Company ?? string.Empty;
                SelectedCompany   = null;
            }
        }
        else
        {
            CompanySearchText = string.Empty;
            SelectedCompany   = null;
        }
    }
}
