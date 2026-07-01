#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Dialogs;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Microsoft.Kiota.Abstractions;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminMagazineIssuesViewModel : ObservableObject, IRegionAware
{
    static readonly HashSet<string> _allowedCoverExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".tiff", ".tif", ".bmp"
    };

    private readonly Client                                  _apiClient;
    private readonly MagazinesService                        _magazinesService;
    private readonly IJwtService                              _jwtService;
    private readonly IStringLocalizer                         _localizer;
    private readonly ILogger<AdminMagazineIssuesViewModel>    _logger;
    private readonly ITokenService                            _tokenService;
    private readonly IRegionManager                           _regionManager;

    private long _magazineId;

    // --- List state ---
    [ObservableProperty] private string _magazineTitle = string.Empty;
    [ObservableProperty] private ObservableCollection<MagazineIssueDto> _issues = [];
    [ObservableProperty] private MagazineIssueDto? _selectedIssue;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;

    // --- Edit panel state ---
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isEditingExisting;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    private long? _editingIssueId;

    // --- Form fields ---
    [ObservableProperty] private string _caption = string.Empty;
    [ObservableProperty] private string _nativeCaption = string.Empty;
    [ObservableProperty] private int? _issueNumber;
    [ObservableProperty] private DateTimeOffset? _published;
    [ObservableProperty] private int _publishedPrecision;
    [ObservableProperty] private string _productCode = string.Empty;
    [ObservableProperty] private int? _pages;
    [ObservableProperty] private string _internetArchiveUrl = string.Empty;

    // --- Cover state ---
    [ObservableProperty] private bool _hasCover;
    [ObservableProperty] private bool _isUploadingCover;
    [ObservableProperty] private string _coverMessage = string.Empty;
    [ObservableProperty] private bool _hasCoverMessage;

    // --- People junction ---
    [ObservableProperty] private ObservableCollection<PersonByMagazineDto> _issuePeople = [];
    [ObservableProperty] private ObservableCollection<string> _issuePeopleDisplays = [];
    [ObservableProperty] private ObservableCollection<PersonDto> _availablePeople = [];
    [ObservableProperty] private PersonDto? _selectedAvailablePerson;
    [ObservableProperty] private ObservableCollection<DocumentRoleDto> _availableRoles = [];
    [ObservableProperty] private DocumentRoleDto? _selectedPersonRole;
    private List<PersonDto>? _allPeopleList;

    // --- Machines junction ---
    [ObservableProperty] private ObservableCollection<MagazineByMachineDto> _issueMachines = [];
    [ObservableProperty] private ObservableCollection<string> _issueMachineDisplays = [];
    [ObservableProperty] private ObservableCollection<MachineDto> _availableMachines = [];
    [ObservableProperty] private MachineDto? _selectedAvailableMachine;
    private List<MachineDto>? _allMachinesList;

    // --- Machine Families junction ---
    [ObservableProperty] private ObservableCollection<MagazineByMachineFamilyDto> _issueMachineFamilies = [];
    [ObservableProperty] private ObservableCollection<string> _issueMachineFamilyDisplays = [];
    [ObservableProperty] private ObservableCollection<MachineFamilyDto> _availableMachineFamilies = [];
    [ObservableProperty] private MachineFamilyDto? _selectedAvailableMachineFamily;
    private List<MachineFamilyDto>? _allMachineFamiliesList;

    // --- Software junction ---
    [ObservableProperty] private ObservableCollection<MagazineBySoftwareDto> _issueSoftware = [];
    [ObservableProperty] private ObservableCollection<string> _issueSoftwareDisplays = [];
    [ObservableProperty] private ObservableCollection<SoftwareDto> _availableSoftware = [];
    [ObservableProperty] private SoftwareDto? _selectedAvailableSoftware;
    private List<SoftwareDto>? _allSoftwareList;

    // --- Document roles ---
    private List<DocumentRoleDto>? _allRolesList;

    public AdminMagazineIssuesViewModel(Client                                apiClient,
                                        MagazinesService                      magazinesService,
                                        IJwtService                            jwtService,
                                        ITokenService                          tokenService,
                                        ILogger<AdminMagazineIssuesViewModel>  logger,
                                        IStringLocalizer                       localizer,
                                        IRegionManager                        regionManager)
    {
        _apiClient        = apiClient;
        _magazinesService = magazinesService;
        _jwtService       = jwtService;
        _tokenService     = tokenService;
        _logger           = logger;
        _localizer        = localizer;
        _regionManager    = regionManager;

        LoadIssuesCommand   = new AsyncRelayCommand(LoadIssuesAsync);
        OpenAddIssueCommand  = new RelayCommand(OpenAddIssue);
        OpenEditIssueCommand = new RelayCommand<MagazineIssueDto>(OpenEditIssue);
        DeleteIssueCommand   = new AsyncRelayCommand<MagazineIssueDto>(DeleteIssueAsync);
        SaveIssueCommand     = new AsyncRelayCommand(SaveIssueAsync);
        CancelEditCommand    = new RelayCommand(CancelEdit);
        GoBackCommand        = new RelayCommand(GoBack);

        UploadCoverCommand = new AsyncRelayCommand(UploadCoverAsync);
        DeleteCoverCommand = new AsyncRelayCommand(DeleteCoverAsync);

        AddPersonCommand           = new AsyncRelayCommand(AddPersonAsync);
        RemovePersonCommand        = new AsyncRelayCommand<string>(RemovePersonByDisplayAsync);
        AddMachineCommand          = new AsyncRelayCommand(AddMachineAsync);
        RemoveMachineCommand       = new AsyncRelayCommand<string>(RemoveMachineByDisplayAsync);
        AddMachineFamilyCommand    = new AsyncRelayCommand(AddMachineFamilyAsync);
        RemoveMachineFamilyCommand = new AsyncRelayCommand<string>(RemoveMachineFamilyByDisplayAsync);
        AddSoftwareCommand         = new AsyncRelayCommand(AddSoftwareAsync);
        RemoveSoftwareCommand      = new AsyncRelayCommand<string>(RemoveSoftwareByDisplayAsync);

        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand                  LoadIssuesCommand    { get; }
    public IRelayCommand                       OpenAddIssueCommand  { get; }
    public IRelayCommand<MagazineIssueDto>     OpenEditIssueCommand { get; }
    public IAsyncRelayCommand<MagazineIssueDto> DeleteIssueCommand  { get; }
    public IAsyncRelayCommand                  SaveIssueCommand     { get; }
    public IRelayCommand                       CancelEditCommand    { get; }
    public IRelayCommand                       GoBackCommand        { get; }

    public IAsyncRelayCommand UploadCoverCommand { get; }
    public IAsyncRelayCommand DeleteCoverCommand { get; }

    public IAsyncRelayCommand         AddPersonCommand           { get; }
    public IAsyncRelayCommand<string> RemovePersonCommand        { get; }
    public IAsyncRelayCommand         AddMachineCommand          { get; }
    public IAsyncRelayCommand<string> RemoveMachineCommand       { get; }
    public IAsyncRelayCommand         AddMachineFamilyCommand    { get; }
    public IAsyncRelayCommand<string> RemoveMachineFamilyCommand { get; }
    public IAsyncRelayCommand         AddSoftwareCommand         { get; }
    public IAsyncRelayCommand<string> RemoveSoftwareCommand      { get; }

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(navigationContext.Parameters.TryGetValue<long>(NavParamKeys.MagazineId, out long magazineId))
            _magazineId = magazineId;

        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.MagazineTitle, out string? title))
            MagazineTitle = title ?? string.Empty;

        if(IsAdmin)
        {
            _ = LoadIssuesCommand.ExecuteAsync(null);
            _ = LoadPickerDataAsync();
        }
    }

    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();
            if(string.IsNullOrWhiteSpace(token)) { IsAdmin = false; return; }
            IEnumerable<string> roles = _jwtService.GetRoles(token);
            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch { IsAdmin = false; }
    }

    private void GoBack() => _regionManager.RequestNavigate(RegionNames.Content, nameof(Presentation.Views.Admin.AdminMagazinesPage));

    // --- Load issues ---
    private async Task LoadIssuesAsync()
    {
        try
        {
            IsLoading = true; HasError = false; ErrorMessage = string.Empty;
            Issues.Clear();
            List<MagazineIssueDto> response = await _magazinesService.GetIssuesByMagazineAsync(_magazineId);
            foreach(MagazineIssueDto i in response) Issues.Add(i);
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading issues for magazine {Id}", _magazineId);
            ErrorMessage = _localizer["FailedToLoadMagazineIssues"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    // --- Add issue ---
    private void OpenAddIssue()
    {
        _editingIssueId = null;
        EditPanelTitle = _localizer["AddMagazineIssueDialog_Title"];
        ClearForm();
        IsEditingExisting = false;
        IsEditing         = true;
    }

    // --- Edit issue ---
    private void OpenEditIssue(MagazineIssueDto? issue)
    {
        if(issue?.Id == null) return;

        _editingIssueId = issue.Id;
        EditPanelTitle = _localizer["EditMagazineIssueDialog_Title"];
        PopulateForm(issue);
        _ = LoadAllJunctionsAsync(issue.Id.Value);
        IsEditingExisting = true;
        IsEditing         = true;
    }

    // --- Delete issue ---
    private async Task DeleteIssueAsync(MagazineIssueDto? issue)
    {
        if(issue?.Id == null) return;

        string itemName = issue.Caption ?? $"{MagazineTitle} #{issue.IssueNumber}";

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer, itemName))
            return;

        try
        {
            await _magazinesService.DeleteIssueAsync(issue.Id.Value);
            await LoadIssuesAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting magazine issue {Id}", issue.Id);
            ErrorMessage = _localizer["FailedToDeleteMagazineIssue"];
            HasError = true;
        }
    }

    // --- Save issue ---
    private async Task SaveIssueAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(Caption))
            {
                ErrorMessage = _localizer["MagazineIssueCaptionRequired"];
                HasError = true;
                return;
            }

            var dto = new MagazineIssueDto
            {
                MagazineId         = _magazineId,
                Caption            = Caption,
                NativeCaption      = string.IsNullOrWhiteSpace(NativeCaption) ? null : NativeCaption,
                IssueNumber        = IssueNumber,
                Published          = Published,
                PublishedPrecision = PublishedPrecision,
                ProductCode        = string.IsNullOrWhiteSpace(ProductCode) ? null : ProductCode,
                Pages              = Pages,
                InternetArchiveUrl = string.IsNullOrWhiteSpace(InternetArchiveUrl) ? null : InternetArchiveUrl.Trim()
            };

            if(_editingIssueId == null)
                await _magazinesService.CreateIssueAsync(dto);
            else
            {
                dto.Id = _editingIssueId;
                await _magazinesService.UpdateIssueAsync(_editingIssueId.Value, dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadIssuesAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving magazine issue");
            ErrorMessage = _localizer["FailedToSaveMagazineIssue"];
            HasError = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing          = false;
        IsEditingExisting  = false;
        _editingIssueId    = null;
        ClearForm();
        HasError     = false;
        ErrorMessage = string.Empty;
    }

    // ======================== COVER ========================

    private async Task UploadCoverAsync()
    {
        if(_editingIssueId == null) return;

        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();

            foreach(string extension in _allowedCoverExtensions)
                picker.FileTypeFilter.Add(extension);

#if HAS_UNO
            // Uno Platform handles this automatically
#else
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
#endif

            Windows.Storage.StorageFile? file = await picker.PickSingleFileAsync();

            if(file is null) return;

            IsUploadingCover = true;
            HasCoverMessage  = false;
            CoverMessage     = string.Empty;

            using Stream stream = await file.OpenStreamForReadAsync();
            using var    ms     = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            string contentType = Path.GetExtension(file.Name).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"            => "image/png",
                ".webp"           => "image/webp",
                ".tiff" or ".tif" => "image/tiff",
                ".bmp"            => "image/bmp",
                _                 => "application/octet-stream"
            };

            MagazineIssueDto? result =
                await _magazinesService.UploadIssueCoverAsync(_editingIssueId.Value, fileBytes, file.Name,
                                                               contentType);

            if(result is not null)
            {
                HasCover        = true;
                CoverMessage     = _localizer["MagazineIssueCoverUploadedSuccessfully"];
                HasCoverMessage  = true;
            }
            else
            {
                CoverMessage    = _localizer["MagazineIssueCoverUploadFailed"];
                HasCoverMessage = true;
                HasError        = true;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading cover for magazine issue {Id}", _editingIssueId);
            CoverMessage    = _localizer["MagazineIssueCoverUploadFailed"];
            HasCoverMessage = true;
            HasError        = true;
        }
        finally { IsUploadingCover = false; }
    }

    private async Task DeleteCoverAsync()
    {
        if(_editingIssueId == null) return;

        if(!await ConfirmationDialogHelper.ConfirmDeleteAsync(_localizer, $"{MagazineTitle} {Caption} cover"))
            return;

        try
        {
            bool succeeded = await _magazinesService.DeleteIssueCoverAsync(_editingIssueId.Value);

            if(succeeded)
            {
                HasCover        = false;
                CoverMessage     = _localizer["MagazineIssueCoverDeleted"];
                HasCoverMessage  = true;
            }
            else
            {
                CoverMessage    = _localizer["MagazineIssueCoverDeleteFailed"];
                HasCoverMessage = true;
                HasError        = true;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting cover for magazine issue {Id}", _editingIssueId);
            CoverMessage    = _localizer["MagazineIssueCoverDeleteFailed"];
            HasCoverMessage = true;
            HasError        = true;
        }
    }

    // ======================== JUNCTION MANAGEMENT ========================

    private async Task LoadAllJunctionsAsync(long issueId)
    {
        await Task.WhenAll(
            LoadIssuePeopleAsync(issueId),
            LoadIssueMachinesAsync(issueId),
            LoadIssueMachineFamiliesAsync(issueId),
            LoadIssueSoftwareAsync(issueId)
        );
    }

    // --- People ---
    private async Task LoadIssuePeopleAsync(long issueId)
    {
        IssuePeople.Clear(); IssuePeopleDisplays.Clear();

        try
        {
            List<PersonByMagazineDto> items = await _magazinesService.GetPeopleByMagazineAsync(issueId);

            foreach(PersonByMagazineDto p in items)
            {
                IssuePeople.Add(p);
                string name = p.DisplayName ?? p.Alias ?? $"{p.Name} {p.Surname}".Trim();
                IssuePeopleDisplays.Add($"{name} ({p.Role})");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading people for magazine issue"); }
    }

    private async Task AddPersonAsync()
    {
        if(_editingIssueId == null || SelectedAvailablePerson?.Id == null || SelectedPersonRole?.Id == null)
            return;

        try
        {
            await _magazinesService.AddPersonToMagazineAsync(new PersonByMagazineDto
            {
                PersonId   = SelectedAvailablePerson.Id,
                MagazineId = _editingIssueId,
                RoleId     = SelectedPersonRole.Id
            });

            SelectedAvailablePerson = null;
            await LoadIssuePeopleAsync(_editingIssueId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding person to magazine issue");
            ErrorMessage = _localizer["FailedToSaveMagazineIssue"];
            HasError = true;
        }
    }

    private async Task RemovePersonByDisplayAsync(string? display)
    {
        if(display == null || _editingIssueId == null) return;

        int idx = IssuePeopleDisplays.IndexOf(display);

        if(idx >= 0 && idx < IssuePeople.Count && IssuePeople[idx].Id.HasValue)
        {
            try
            {
                await _magazinesService.RemovePersonFromMagazineAsync(IssuePeople[idx].Id!.Value);
                await LoadIssuePeopleAsync(_editingIssueId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing person from magazine issue"); }
        }
    }

    // --- Machines ---
    private async Task LoadIssueMachinesAsync(long issueId)
    {
        IssueMachines.Clear(); IssueMachineDisplays.Clear();

        try
        {
            List<MagazineByMachineDto> items = await _magazinesService.GetMachinesByMagazineAsync(issueId);

            foreach(MagazineByMachineDto m in items)
            {
                IssueMachines.Add(m);
                IssueMachineDisplays.Add(m.Machine ?? $"Machine #{m.MachineId}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machines for magazine issue"); }

        RefreshAvailableMachines();
    }

    private async Task AddMachineAsync()
    {
        if(_editingIssueId == null || SelectedAvailableMachine?.Id == null) return;

        try
        {
            await _magazinesService.AddMachineToMagazineAsync(new MagazineByMachineDto
            {
                MagazineId = _editingIssueId,
                MachineId  = SelectedAvailableMachine.Id
            });

            SelectedAvailableMachine = null;
            await LoadIssueMachinesAsync(_editingIssueId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine to magazine issue");
            ErrorMessage = _localizer["FailedToSaveMagazineIssue"];
            HasError = true;
        }
    }

    private async Task RemoveMachineByDisplayAsync(string? display)
    {
        if(display == null || _editingIssueId == null) return;

        int idx = IssueMachineDisplays.IndexOf(display);

        if(idx >= 0 && idx < IssueMachines.Count && IssueMachines[idx].Id.HasValue)
        {
            try
            {
                await _magazinesService.RemoveMachineFromMagazineAsync(IssueMachines[idx].Id!.Value);
                await LoadIssueMachinesAsync(_editingIssueId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing machine from magazine issue"); }
        }
    }

    // --- Machine Families ---
    private async Task LoadIssueMachineFamiliesAsync(long issueId)
    {
        IssueMachineFamilies.Clear(); IssueMachineFamilyDisplays.Clear();

        try
        {
            List<MagazineByMachineFamilyDto> items =
                await _magazinesService.GetMachineFamiliesByMagazineAsync(issueId);

            foreach(MagazineByMachineFamilyDto f in items)
            {
                IssueMachineFamilies.Add(f);
                IssueMachineFamilyDisplays.Add(f.MachineFamily ?? $"Family #{f.MachineFamilyId}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machine families for magazine issue"); }

        RefreshAvailableMachineFamilies();
    }

    private async Task AddMachineFamilyAsync()
    {
        if(_editingIssueId == null || SelectedAvailableMachineFamily?.Id == null) return;

        try
        {
            await _magazinesService.AddMachineFamilyToMagazineAsync(new MagazineByMachineFamilyDto
            {
                MagazineId      = _editingIssueId,
                MachineFamilyId = SelectedAvailableMachineFamily.Id
            });

            SelectedAvailableMachineFamily = null;
            await LoadIssueMachineFamiliesAsync(_editingIssueId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding machine family to magazine issue");
            ErrorMessage = _localizer["FailedToSaveMagazineIssue"];
            HasError = true;
        }
    }

    private async Task RemoveMachineFamilyByDisplayAsync(string? display)
    {
        if(display == null || _editingIssueId == null) return;

        int idx = IssueMachineFamilyDisplays.IndexOf(display);

        if(idx >= 0 && idx < IssueMachineFamilies.Count && IssueMachineFamilies[idx].Id.HasValue)
        {
            try
            {
                await _magazinesService.RemoveMachineFamilyFromMagazineAsync(
                    IssueMachineFamilies[idx].Id!.Value);

                await LoadIssueMachineFamiliesAsync(_editingIssueId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing machine family from magazine issue"); }
        }
    }

    // --- Software ---
    private async Task LoadIssueSoftwareAsync(long issueId)
    {
        IssueSoftware.Clear(); IssueSoftwareDisplays.Clear();

        try
        {
            List<MagazineBySoftwareDto> items = await _magazinesService.GetSoftwareByMagazineAsync(issueId);

            foreach(MagazineBySoftwareDto s in items)
            {
                IssueSoftware.Add(s);
                IssueSoftwareDisplays.Add(s.Software ?? $"Software #{s.SoftwareId}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading software for magazine issue"); }

        RefreshAvailableSoftware();
    }

    private async Task AddSoftwareAsync()
    {
        if(_editingIssueId == null || SelectedAvailableSoftware?.Id == null) return;

        try
        {
            await _magazinesService.AddSoftwareToMagazineAsync(new MagazineBySoftwareDto
            {
                MagazineId = _editingIssueId,
                SoftwareId = SelectedAvailableSoftware.Id.Value
            });

            SelectedAvailableSoftware = null;
            await LoadIssueSoftwareAsync(_editingIssueId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding software to magazine issue");
            ErrorMessage = _localizer["FailedToSaveMagazineIssue"];
            HasError = true;
        }
    }

    private async Task RemoveSoftwareByDisplayAsync(string? display)
    {
        if(display == null || _editingIssueId == null) return;

        int idx = IssueSoftwareDisplays.IndexOf(display);

        if(idx >= 0 && idx < IssueSoftware.Count && IssueSoftware[idx].Id.HasValue)
        {
            try
            {
                await _magazinesService.RemoveSoftwareFromMagazineAsync(IssueSoftware[idx].Id!.Value);
                await LoadIssueSoftwareAsync(_editingIssueId.Value);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error removing software from magazine issue"); }
        }
    }

    // ======================== HELPERS ========================

    private void RefreshAvailableMachines()
    {
        AvailableMachines.Clear();

        if(_allMachinesList == null) return;

        HashSet<int> assigned =
            new(IssueMachines.Where(m => m.MachineId.HasValue).Select(m => m.MachineId!.Value));

        foreach(MachineDto m in _allMachinesList)
            if(m.Id.HasValue && !assigned.Contains(m.Id.Value))
                AvailableMachines.Add(m);
    }

    private void RefreshAvailableMachineFamilies()
    {
        AvailableMachineFamilies.Clear();

        if(_allMachineFamiliesList == null) return;

        HashSet<int> assigned =
            new(IssueMachineFamilies.Where(f => f.MachineFamilyId.HasValue)
                                    .Select(f => f.MachineFamilyId!.Value));

        foreach(MachineFamilyDto f in _allMachineFamiliesList)
            if(f.Id.HasValue && !assigned.Contains(f.Id.Value))
                AvailableMachineFamilies.Add(f);
    }

    private void RefreshAvailableSoftware()
    {
        AvailableSoftware.Clear();

        if(_allSoftwareList == null) return;

        HashSet<int> assigned =
            new(IssueSoftware.Where(s => s.SoftwareId.HasValue).Select(s => s.SoftwareId!.Value));

        foreach(SoftwareDto s in _allSoftwareList)
            if(s.Id.HasValue && !assigned.Contains(s.Id.Value))
                AvailableSoftware.Add(s);
    }

    // --- Load picker data ---
    public async Task LoadPickerDataAsync()
    {
        try
        {
            _allRolesList = await _magazinesService.GetDocumentRolesAsync();
            AvailableRoles.Clear();
            foreach(DocumentRoleDto r in _allRolesList) AvailableRoles.Add(r);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading document roles"); }

        try { _allPeopleList = await _apiClient.People.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading people"); }

        try { _allMachinesList = await _apiClient.Machines.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machines"); }

        try { _allMachineFamiliesList = await _apiClient.MachineFamilies.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading machine families"); }

        try { _allSoftwareList = await _apiClient.Software.GetAsync(); }
        catch(Exception ex) { _logger.LogError(ex, "Error loading software"); }
    }

    // --- People search ---
    public void UpdatePeopleSuggestions(string query)
    {
        AvailablePeople.Clear();
        if(_allPeopleList == null) return;

        IEnumerable<PersonDto> source = _allPeopleList;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(p =>
                (p.Name != null && p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (p.Surname != null && p.Surname.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (p.DisplayName != null && p.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)));

        foreach(PersonDto p in source) AvailablePeople.Add(p);
    }

    // --- Form helpers ---
    private void ClearForm()
    {
        Caption            = string.Empty;
        NativeCaption      = string.Empty;
        IssueNumber        = null;
        Published          = null;
        PublishedPrecision = 0;
        ProductCode        = string.Empty;
        Pages              = null;
        InternetArchiveUrl = string.Empty;
        HasCover           = false;
        CoverMessage       = string.Empty;
        HasCoverMessage    = false;

        IssuePeople.Clear();           IssuePeopleDisplays.Clear();
        IssueMachines.Clear();         IssueMachineDisplays.Clear();
        IssueMachineFamilies.Clear();  IssueMachineFamilyDisplays.Clear();
        IssueSoftware.Clear();         IssueSoftwareDisplays.Clear();
        AvailableMachines.Clear(); AvailableMachineFamilies.Clear(); AvailableSoftware.Clear();
        SelectedAvailablePerson        = null;
        SelectedAvailableMachine       = null;
        SelectedAvailableMachineFamily = null;
        SelectedAvailableSoftware      = null;
        SelectedPersonRole             = null;

        HasError     = false;
        ErrorMessage = string.Empty;
    }

    private void PopulateForm(MagazineIssueDto issue)
    {
        Caption            = issue.Caption       ?? string.Empty;
        NativeCaption      = issue.NativeCaption  ?? string.Empty;
        IssueNumber        = issue.IssueNumber;
        Published          = issue.Published;
        PublishedPrecision = issue.PublishedPrecision ?? 0;
        ProductCode        = issue.ProductCode    ?? string.Empty;
        Pages              = issue.Pages;
        InternetArchiveUrl = issue.InternetArchiveUrl ?? string.Empty;
        HasCover           = issue.CoverGuid.HasValue;
        CoverMessage       = string.Empty;
        HasCoverMessage    = false;
    }
}
