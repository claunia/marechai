#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoundSynthsViewModel : ObservableObject, IRegionAware
{
    private readonly ApiClient                            _apiClient;
    private readonly IJwtService                          _jwtService;
    private readonly IStringLocalizer                     _localizer;
    private readonly ILogger<AdminSoundSynthsViewModel>   _logger;
    private readonly ITokenService                        _tokenService;

    [ObservableProperty] private ObservableCollection<SoundSynthDto> _soundSynths = [];
    [ObservableProperty] private ObservableCollection<SoundSynthDto> _filteredSoundSynths = [];
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private SoundSynthDto? _selectedSoundSynth;
    private List<SoundSynthDto>? _allSoundSynths;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    private int? _editingId;

    // --- Form fields ---
    [ObservableProperty] private string _synthName = string.Empty;
    [ObservableProperty] private string _modelCode = string.Empty;
    [ObservableProperty] private DateTimeOffset? _introduced;
    [ObservableProperty] private int? _voices;
    [ObservableProperty] private double? _frequency;
    [ObservableProperty] private int? _depth;
    [ObservableProperty] private int? _squareWave;
    [ObservableProperty] private int? _whiteNoise;
    [ObservableProperty] private int? _synthType;

    // --- Company picker ---
    [ObservableProperty] private CompanyDto? _selectedCompany;
    [ObservableProperty] private string _companySearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyDto> _companySuggestions = [];
    private List<CompanyDto>? _allCompanies;

    public AdminSoundSynthsViewModel(ApiClient                            apiClient,
                                     IJwtService                          jwtService,
                                     ITokenService                        tokenService,
                                     ILogger<AdminSoundSynthsViewModel>   logger,
                                     IStringLocalizer                     localizer)
    {
        _apiClient    = apiClient;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadItemsCommand  = new AsyncRelayCommand(LoadItemsAsync);
        OpenAddCommand    = new RelayCommand(OpenAdd);
        OpenEditCommand   = new RelayCommand<SoundSynthDto>(OpenEdit);
        DeleteCommand     = new AsyncRelayCommand<SoundSynthDto>(DeleteAsync);
        SaveCommand       = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand = new RelayCommand(CancelEdit);

        CheckAdminRole();
    }

    public IAsyncRelayCommand                LoadItemsCommand  { get; }
    public IRelayCommand                     OpenAddCommand    { get; }
    public IRelayCommand<SoundSynthDto>       OpenEditCommand   { get; }
    public IAsyncRelayCommand<SoundSynthDto>  DeleteCommand     { get; }
    public IAsyncRelayCommand                SaveCommand       { get; }
    public IRelayCommand                     CancelEditCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
            _ = LoadItemsCommand.ExecuteAsync(null);
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
                      roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
        }
        catch { IsAdmin = false; }
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true; HasError = false; ErrorMessage = string.Empty;
            SoundSynths.Clear();

            List<SoundSynthDto>? response = await _apiClient.SoundSynths.GetAsync();
            _allSoundSynths = response;

            if(response != null)
                foreach(SoundSynthDto item in response)
                    SoundSynths.Add(item);

            ApplyFilter();
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

    public void ApplyFilter()
    {
        FilteredSoundSynths.Clear();

        IEnumerable<SoundSynthDto> source = (IEnumerable<SoundSynthDto>?)_allSoundSynths ?? SoundSynths;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(s => (s.Name != null &&
                                        s.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (s.Company != null &&
                                        s.Company.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (s.ModelCode != null &&
                                        s.ModelCode.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));

        foreach(SoundSynthDto item in source)
            FilteredSoundSynths.Add(item);
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
        if(_allCompanies == null)
        {
            try { _allCompanies = await _apiClient.Companies.GetAsync(); }
            catch(Exception ex) { _logger.LogError(ex, "Error loading companies for picker"); }
        }
    }

    private void ClearForm()
    {
        SynthName         = string.Empty;
        ModelCode          = string.Empty;
        Introduced         = null;
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
}
