#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminMachinesViewModel : ObservableObject, IRegionAware
{
    private readonly ApiClient                          _apiClient;
    private readonly IJwtService                        _jwtService;
    private readonly IStringLocalizer                   _localizer;
    private readonly ILogger<AdminMachinesViewModel>    _logger;
    private readonly IRegionManager                     _regionManager;
    private readonly ITokenService                      _tokenService;

    // --- List state ---
    [ObservableProperty] private ObservableCollection<MachineDto> _machines = [];
    [ObservableProperty] private ObservableCollection<MachineDto> _filteredMachines = [];
    [ObservableProperty] private string _filterText = string.Empty;
    [ObservableProperty] private MachineDto? _selectedMachine;
    private List<MachineDto>? _allMachines;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isEditingExisting;
    [ObservableProperty] private string _editPanelTitle = string.Empty;
    private int? _editingId;

    // --- Base form fields ---
    [ObservableProperty] private string _machineName = string.Empty;
    [ObservableProperty] private string _model = string.Empty;
    [ObservableProperty] private int _machineTypeIndex;
    [ObservableProperty] private DateTimeOffset? _introduced;
    [ObservableProperty] private List<string> _machineTypeItems = [];

    // --- Company picker ---
    [ObservableProperty] private CompanyDto? _selectedCompany;
    [ObservableProperty] private string _companySearchText = string.Empty;
    [ObservableProperty] private ObservableCollection<CompanyDto> _companySuggestions = [];
    private List<CompanyDto>? _allCompanies;

    // --- Family picker ---
    [ObservableProperty] private MachineFamilyDto? _selectedFamily;
    [ObservableProperty] private ObservableCollection<MachineFamilyDto> _families = [];

    // --- GPU junction ---
    [ObservableProperty] private ObservableCollection<GpuByMachineDto> _machineGpus = [];
    [ObservableProperty] private ObservableCollection<string> _machineGpuDisplays = [];
    [ObservableProperty] private ObservableCollection<GpuDto> _availableGpus = [];
    [ObservableProperty] private GpuDto? _selectedAvailableGpu;
    private List<GpuDto>? _allGpusList;

    // --- Processor junction ---
    [ObservableProperty] private ObservableCollection<ProcessorByMachineDto> _machineProcessors = [];
    [ObservableProperty] private ObservableCollection<string> _machineProcessorDisplays = [];
    [ObservableProperty] private ObservableCollection<ProcessorDto> _availableProcessors = [];
    [ObservableProperty] private ProcessorDto? _selectedAvailableProcessor;
    [ObservableProperty] private double? _processorSpeed;
    private List<ProcessorDto>? _allProcessorsList;

    // --- Sound synth junction ---
    [ObservableProperty] private ObservableCollection<SoundSynthByMachineDto> _machineSounds = [];
    [ObservableProperty] private ObservableCollection<string> _machineSoundDisplays = [];
    [ObservableProperty] private ObservableCollection<SoundSynthDto> _availableSounds = [];
    [ObservableProperty] private SoundSynthDto? _selectedAvailableSound;
    private List<SoundSynthDto>? _allSoundsList;

    // --- Screen junction ---
    [ObservableProperty] private ObservableCollection<ScreenByMachineDto> _machineScreens = [];
    [ObservableProperty] private ObservableCollection<string> _machineScreenDisplays = [];
    [ObservableProperty] private ObservableCollection<ScreenDto> _availableScreens = [];
    [ObservableProperty] private ScreenDto? _selectedAvailableScreen;
    private List<ScreenDto>? _allScreensList;

    // --- Memory (inline creation) ---
    [ObservableProperty] private ObservableCollection<MemoryByMachineDto> _machineMemories = [];
    [ObservableProperty] private ObservableCollection<string> _machineMemoryDisplays = [];
    [ObservableProperty] private int _memoryTypeIndex;
    [ObservableProperty] private int _memoryUsageIndex;
    [ObservableProperty] private long? _memorySize;
    [ObservableProperty] private double? _memorySpeed;

    // --- Storage (inline creation) ---
    [ObservableProperty] private ObservableCollection<StorageByMachineDto> _machineStorage = [];
    [ObservableProperty] private ObservableCollection<string> _machineStorageDisplays = [];
    [ObservableProperty] private int _storageTypeIndex;
    [ObservableProperty] private int _storageInterfaceIndex;
    [ObservableProperty] private long? _storageCapacity;

    // --- Enum items for ComboBoxes ---
    public List<string> MemoryTypeItems { get; private set; } = [];
    public List<string> MemoryUsageItems { get; private set; } = [];
    public List<string> StorageTypeItems { get; private set; } = [];
    public List<string> StorageInterfaceItems { get; private set; } = [];

    public AdminMachinesViewModel(ApiClient                          apiClient,
                                  IJwtService                        jwtService,
                                  ITokenService                      tokenService,
                                  ILogger<AdminMachinesViewModel>    logger,
                                  IStringLocalizer                   localizer,
                                  IRegionManager                     regionManager)
    {
        _apiClient      = apiClient;
        _jwtService     = jwtService;
        _tokenService   = tokenService;
        _logger         = logger;
        _localizer      = localizer;
        _regionManager  = regionManager;

        MachineTypeItems  = [localizer["MachineTypeUnknown"], localizer["MachineTypeComputer"], localizer["MachineTypeConsole"]];

        // Memory/Storage enum display names use enum value names directly for now
        MemoryTypeItems     = ["Unknown","DRAM","FPM","EDO","VRAM","SDRAM","DDR","DDR2","DDR3","DDR4","RDRAM","SGRAM","PSRAM","SRAM","ROM","PROM","EPROM","EEPROM","NAND","NOR","ReRAM","CBRAM","DWM","NanoRAM","Millipede","FJG","PunchedPaper","DrumMemory","MagneticCore","PlatedWire","CoreRope","ThinFilm","Twistor","Bubble"];
        MemoryUsageItems    = ["Unknown","Bootloader","Firmware","Work","Video","Sound","Wavetable","StorageBuffer","Save","Configuration","Unified"];
        StorageTypeItems    = ["Unknown"]; // Simplified — full 149 values would be impractical in a dropdown
        StorageInterfaceItems = ["Unknown","ACSI","ATA","SCSI","USB","FireWire","SATA","SSA","SAS","FC","PCIe","M.2","SataExpress","CompactFlash","SDCard","MMC","IDE","Proprietary","Parallel","Serial","NVMe"];

        LoadItemsCommand       = new AsyncRelayCommand(LoadItemsAsync);
        OpenAddCommand         = new RelayCommand(OpenAdd);
        OpenEditCommand        = new RelayCommand<MachineDto>(OpenEdit);
        DeleteCommand          = new AsyncRelayCommand<MachineDto>(DeleteAsync);
        SaveCommand            = new AsyncRelayCommand(SaveAsync);
        CancelEditCommand      = new RelayCommand(CancelEdit);

        // Junction commands
        AddGpuCommand          = new AsyncRelayCommand(AddGpuAsync);
        RemoveGpuCommand       = new AsyncRelayCommand<string>(RemoveGpuByDisplayAsync);
        AddProcessorCommand    = new AsyncRelayCommand(AddProcessorAsync);
        RemoveProcessorCommand = new AsyncRelayCommand<string>(RemoveProcessorByDisplayAsync);
        AddSoundCommand        = new AsyncRelayCommand(AddSoundAsync);
        RemoveSoundCommand     = new AsyncRelayCommand<string>(RemoveSoundByDisplayAsync);
        AddScreenCommand       = new AsyncRelayCommand(AddScreenAsync);
        RemoveScreenCommand    = new AsyncRelayCommand<string>(RemoveScreenByDisplayAsync);
        AddMemoryCommand       = new AsyncRelayCommand(AddMemoryAsync);
        RemoveMemoryCommand    = new AsyncRelayCommand<string>(RemoveMemoryByDisplayAsync);
        AddStorageCommand      = new AsyncRelayCommand(AddStorageAsync);
        RemoveStorageCommand   = new AsyncRelayCommand<string>(RemoveStorageByDisplayAsync);
        OpenPhotosCommand      = new RelayCommand<MachineDto>(OpenPhotos);

        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand LoadItemsCommand { get; }
    public IRelayCommand OpenAddCommand { get; }
    public IRelayCommand<MachineDto> OpenEditCommand { get; }
    public IAsyncRelayCommand<MachineDto> DeleteCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelEditCommand { get; }

    public IAsyncRelayCommand AddGpuCommand { get; }
    public IAsyncRelayCommand<string> RemoveGpuCommand { get; }
    public IAsyncRelayCommand AddProcessorCommand { get; }
    public IAsyncRelayCommand<string> RemoveProcessorCommand { get; }
    public IAsyncRelayCommand AddSoundCommand { get; }
    public IAsyncRelayCommand<string> RemoveSoundCommand { get; }
    public IAsyncRelayCommand AddScreenCommand { get; }
    public IAsyncRelayCommand<string> RemoveScreenCommand { get; }
    public IAsyncRelayCommand AddMemoryCommand { get; }
    public IAsyncRelayCommand<string> RemoveMemoryCommand { get; }
    public IAsyncRelayCommand AddStorageCommand { get; }
    public IAsyncRelayCommand<string> RemoveStorageCommand { get; }
    public IRelayCommand<MachineDto> OpenPhotosCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();
        if(IsAdmin) _ = LoadItemsCommand.ExecuteAsync(null);
    }

    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();
            if(string.IsNullOrWhiteSpace(token)) { IsAdmin = false; return; }
            IEnumerable<string> roles = _jwtService.GetRoles(token);
            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
        }
        catch { IsAdmin = false; }
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            IsLoading = true; HasError = false; ErrorMessage = string.Empty;
            Machines.Clear();
            List<MachineDto>? response = await _apiClient.Machines.GetAsync();
            _allMachines = response;
            if(response != null) foreach(MachineDto m in response) Machines.Add(m);
            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading machines");
            ErrorMessage = _localizer["FailedToLoadMachines"];
            HasError = true;
        }
        finally { IsLoading = false; }
    }

    private void OpenAdd()
    {
        _editingId = null;
        EditPanelTitle = _localizer["AddMachineDialog_Title"];
        ClearForm();
        IsEditingExisting = false;
        IsEditing = true;
    }

    private async void OpenEdit(MachineDto? item)
    {
        if(item?.Id == null) return;
        try
        {
            MachineDto? full = await _apiClient.Machines[item.Id.Value].GetAsync();
            if(full == null) return;
            _editingId = item.Id;
            EditPanelTitle = _localizer["EditMachineDialog_Title"];
            PopulateForm(full);
            await LoadAllJunctionsAsync(item.Id.Value);
            IsEditingExisting = true;
            IsEditing = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading machine {Id}", item.Id);
            ErrorMessage = _localizer["FailedToLoadMachines"];
            HasError = true;
        }
    }

    private async Task DeleteAsync(MachineDto? item)
    {
        if(item?.Id == null) return;
        try
        {
            await _apiClient.Machines[item.Id.Value].DeleteAsync();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine {Id}", item.Id);
            ErrorMessage = _localizer["FailedToDeleteMachine"];
            HasError = true;
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(MachineName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError = true;
                return;
            }
            if(SelectedCompany?.Id == null)
            {
                ErrorMessage = _localizer["MachineCompanyRequired"];
                HasError = true;
                return;
            }

            var dto = new MachineDto
            {
                Name       = MachineName,
                Model      = string.IsNullOrWhiteSpace(Model) ? null : Model,
                CompanyId  = SelectedCompany.Id.Value,
                Type       = MachineTypeIndex,
                Introduced = Introduced,
                FamilyId   = SelectedFamily?.Id
            };

            if(_editingId == null)
                await _apiClient.Machines.PostAsync(dto);
            else
            {
                dto.Id = _editingId;
                await _apiClient.Machines[_editingId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadItemsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving machine");
            ErrorMessage = _localizer["FailedToSaveMachine"];
            HasError = true;
        }
    }

    private void CancelEdit()
    {
        IsEditing = false; _editingId = null;
        IsEditingExisting = false;
        ClearForm();
        HasError = false; ErrorMessage = string.Empty;
    }

    public void ApplyFilter()
    {
        FilteredMachines.Clear();
        IEnumerable<MachineDto> source = (IEnumerable<MachineDto>?)_allMachines ?? Machines;
        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(m => (m.Name != null && m.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (m.Company != null && m.Company.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (m.Model != null && m.Model.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));
        foreach(MachineDto m in source) FilteredMachines.Add(m);
    }

    public void UpdateCompanySuggestions(string query)
    {
        CompanySuggestions.Clear();
        if(_allCompanies == null) return;
        IEnumerable<CompanyDto> source = _allCompanies;
        if(!string.IsNullOrWhiteSpace(query))
            source = source.Where(c => c.Name != null && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        foreach(CompanyDto match in source) CompanySuggestions.Add(match);
    }

    public async Task LoadPickerDataAsync()
    {
        if(_allCompanies == null)
            try { _allCompanies = await _apiClient.Companies.GetAsync(); }
            catch(Exception ex) { _logger.LogError(ex, "Error loading companies"); }

        if(Families.Count == 0)
            try
            {
                List<MachineFamilyDto>? fams = await _apiClient.MachineFamilies.GetAsync();
                if(fams != null) foreach(MachineFamilyDto f in fams) Families.Add(f);
            }
            catch(Exception ex) { _logger.LogError(ex, "Error loading families"); }

        if(_allGpusList == null)
            try { _allGpusList = await _apiClient.Gpus.GetAsync(); }
            catch(Exception ex) { _logger.LogError(ex, "Error loading GPUs"); }

        if(_allProcessorsList == null)
            try { _allProcessorsList = await _apiClient.Processors.GetAsync(); }
            catch(Exception ex) { _logger.LogError(ex, "Error loading processors"); }

        if(_allSoundsList == null)
            try { _allSoundsList = await _apiClient.SoundSynths.GetAsync(); }
            catch(Exception ex) { _logger.LogError(ex, "Error loading sound synths"); }

        if(_allScreensList == null)
            try { _allScreensList = await _apiClient.Screens.GetAsync(); }
            catch(Exception ex) { _logger.LogError(ex, "Error loading screens"); }
    }

    // ======================== JUNCTION MANAGEMENT ========================

    private async Task LoadAllJunctionsAsync(int machineId)
    {
        await Task.WhenAll(
            LoadMachineGpusAsync(machineId),
            LoadMachineProcessorsAsync(machineId),
            LoadMachineSoundsAsync(machineId),
            LoadMachineScreensAsync(machineId),
            LoadMachineMemoriesAsync(machineId),
            LoadMachineStorageAsync(machineId)
        );
    }

    // --- GPUs ---
    private async Task LoadMachineGpusAsync(int machineId)
    {
        MachineGpus.Clear(); MachineGpuDisplays.Clear();
        try
        {
            List<GpuByMachineDto>? items = await _apiClient.Machines.Gpus.ByMachine[machineId].GetAsync();
            if(items != null) foreach(GpuByMachineDto g in items)
            {
                MachineGpus.Add(g);
                MachineGpuDisplays.Add($"{g.Company} {g.Name}".Trim());
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading GPUs for machine"); }
        RefreshAvailable(_allGpusList, MachineGpus.Select(g => g.GpuId), AvailableGpus);
    }

    private async Task AddGpuAsync()
    {
        if(_editingId == null || SelectedAvailableGpu?.Id == null) return;
        try
        {
            await _apiClient.Machines.Gpus.PostAsync(new GpuByMachineDto { GpuId = SelectedAvailableGpu.Id, MachineId = _editingId });
            SelectedAvailableGpu = null;
            await LoadMachineGpusAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding GPU"); ErrorMessage = _localizer["FailedToSaveMachine"]; HasError = true; }
    }

    private async Task RemoveGpuByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;
        int idx = MachineGpuDisplays.IndexOf(display);
        if(idx >= 0 && idx < MachineGpus.Count && MachineGpus[idx].Id.HasValue)
        {
            try { await _apiClient.Machines.Gpus[MachineGpus[idx].Id.Value].DeleteAsync(); await LoadMachineGpusAsync(_editingId.Value); }
            catch(Exception ex) { _logger.LogError(ex, "Error removing GPU"); }
        }
    }

    // --- Processors ---
    private async Task LoadMachineProcessorsAsync(int machineId)
    {
        MachineProcessors.Clear(); MachineProcessorDisplays.Clear();
        try
        {
            List<ProcessorByMachineDto>? items = await _apiClient.ProcessorsByMachine.ByMachine[machineId].GetAsync();
            if(items != null) foreach(ProcessorByMachineDto p in items)
            {
                MachineProcessors.Add(p);
                string speed = p.Speed.HasValue ? $" @ {p.Speed}MHz" : "";
                MachineProcessorDisplays.Add($"{p.Company} {p.Name}{speed}".Trim());
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading processors for machine"); }
        RefreshAvailable(_allProcessorsList, MachineProcessors.Select(p => p.ProcessorId), AvailableProcessors);
    }

    private async Task AddProcessorAsync()
    {
        if(_editingId == null || SelectedAvailableProcessor?.Id == null) return;
        try
        {
            await _apiClient.ProcessorsByMachine.PostAsync(new ProcessorByMachineDto
            {
                ProcessorId = SelectedAvailableProcessor.Id, MachineId = _editingId,
                Speed = ProcessorSpeed.HasValue ? (float)ProcessorSpeed.Value : null
            });
            SelectedAvailableProcessor = null; ProcessorSpeed = null;
            await LoadMachineProcessorsAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding processor"); ErrorMessage = _localizer["FailedToSaveMachine"]; HasError = true; }
    }

    private async Task RemoveProcessorByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;
        int idx = MachineProcessorDisplays.IndexOf(display);
        if(idx >= 0 && idx < MachineProcessors.Count && MachineProcessors[idx].Id.HasValue)
        {
            try { await _apiClient.ProcessorsByMachine[MachineProcessors[idx].Id.Value].DeleteAsync(); await LoadMachineProcessorsAsync(_editingId.Value); }
            catch(Exception ex) { _logger.LogError(ex, "Error removing processor"); }
        }
    }

    // --- Sound Synths ---
    private async Task LoadMachineSoundsAsync(int machineId)
    {
        MachineSounds.Clear(); MachineSoundDisplays.Clear();
        try
        {
            List<SoundSynthByMachineDto>? items = await _apiClient.SoundSynthsByMachine.ByMachine[machineId].GetAsync();
            if(items != null) foreach(SoundSynthByMachineDto s in items)
            {
                MachineSounds.Add(s);
                MachineSoundDisplays.Add($"{s.Company} {s.Name}".Trim());
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading sounds for machine"); }
        RefreshAvailable(_allSoundsList, MachineSounds.Select(s => s.SoundSynthId), AvailableSounds);
    }

    private async Task AddSoundAsync()
    {
        if(_editingId == null || SelectedAvailableSound?.Id == null) return;
        try
        {
            await _apiClient.SoundSynthsByMachine.PostAsync(new SoundSynthByMachineDto { SoundSynthId = SelectedAvailableSound.Id, MachineId = _editingId });
            SelectedAvailableSound = null;
            await LoadMachineSoundsAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding sound synth"); ErrorMessage = _localizer["FailedToSaveMachine"]; HasError = true; }
    }

    private async Task RemoveSoundByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;
        int idx = MachineSoundDisplays.IndexOf(display);
        if(idx >= 0 && idx < MachineSounds.Count && MachineSounds[idx].Id.HasValue)
        {
            try { await _apiClient.SoundSynthsByMachine[MachineSounds[idx].Id.Value].DeleteAsync(); await LoadMachineSoundsAsync(_editingId.Value); }
            catch(Exception ex) { _logger.LogError(ex, "Error removing sound synth"); }
        }
    }

    // --- Screens ---
    private async Task LoadMachineScreensAsync(int machineId)
    {
        MachineScreens.Clear(); MachineScreenDisplays.Clear();
        try
        {
            List<ScreenByMachineDto>? items = await _apiClient.Machines[machineId].Screens.GetAsync();
            if(items != null) foreach(ScreenByMachineDto s in items)
            {
                MachineScreens.Add(s);
                MachineScreenDisplays.Add($"{s.Screen?.ScreenDto?.Diagonal}\" {s.Screen?.ScreenDto?.Type}".Trim());
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading screens for machine"); }
        RefreshAvailable(_allScreensList, MachineScreens.Select(s => s.ScreenId), AvailableScreens);
    }

    private async Task AddScreenAsync()
    {
        if(_editingId == null || SelectedAvailableScreen?.Id == null) return;
        try
        {
            await _apiClient.ScreensByMachine.PostAsync(new ScreenByMachineDto { ScreenId = SelectedAvailableScreen.Id, MachineId = _editingId });
            SelectedAvailableScreen = null;
            await LoadMachineScreensAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding screen"); ErrorMessage = _localizer["FailedToSaveMachine"]; HasError = true; }
    }

    private async Task RemoveScreenByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;
        int idx = MachineScreenDisplays.IndexOf(display);
        if(idx >= 0 && idx < MachineScreens.Count && MachineScreens[idx].Id.HasValue)
        {
            try { await _apiClient.ScreensByMachine[MachineScreens[idx].Id.Value].DeleteAsync(); await LoadMachineScreensAsync(_editingId.Value); }
            catch(Exception ex) { _logger.LogError(ex, "Error removing screen"); }
        }
    }

    // --- Memory (inline creation) ---
    private async Task LoadMachineMemoriesAsync(int machineId)
    {
        MachineMemories.Clear(); MachineMemoryDisplays.Clear();
        try
        {
            List<MemoryByMachineDto>? items = await _apiClient.Machines[machineId].Memories.GetAsync();
            if(items != null) foreach(MemoryByMachineDto m in items)
            {
                MachineMemories.Add(m);
                string type = m.Type.HasValue && m.Type.Value < MemoryTypeItems.Count ? MemoryTypeItems[m.Type.Value] : "?";
                string usage = m.Usage.HasValue && m.Usage.Value < MemoryUsageItems.Count ? MemoryUsageItems[m.Usage.Value] : "?";
                string size = m.Size.HasValue ? $" {FormatBytes(m.Size.Value)}" : "";
                MachineMemoryDisplays.Add($"{type} ({usage}){size}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading memories for machine"); }
    }

    private async Task AddMemoryAsync()
    {
        if(_editingId == null) return;
        try
        {
            await _apiClient.MemoriesByMachine.PostAsync(new MemoryByMachineDto
            {
                MachineId = _editingId, Type = MemoryTypeIndex, Usage = MemoryUsageIndex,
                Size = MemorySize, Speed = MemorySpeed
            });
            MemorySize = null; MemorySpeed = null;
            await LoadMachineMemoriesAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding memory"); ErrorMessage = _localizer["FailedToSaveMachine"]; HasError = true; }
    }

    private async Task RemoveMemoryByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;
        int idx = MachineMemoryDisplays.IndexOf(display);
        if(idx >= 0 && idx < MachineMemories.Count && MachineMemories[idx].Id.HasValue)
        {
            try { await _apiClient.MemoriesByMachine[MachineMemories[idx].Id.Value].DeleteAsync(); await LoadMachineMemoriesAsync(_editingId.Value); }
            catch(Exception ex) { _logger.LogError(ex, "Error removing memory"); }
        }
    }

    // --- Storage (inline creation) ---
    private async Task LoadMachineStorageAsync(int machineId)
    {
        MachineStorage.Clear(); MachineStorageDisplays.Clear();
        try
        {
            List<StorageByMachineDto>? items = await _apiClient.Machines[machineId].Storage.GetAsync();
            if(items != null) foreach(StorageByMachineDto s in items)
            {
                MachineStorage.Add(s);
                string type = s.Type.HasValue && s.Type.Value >= 0 && s.Type.Value < StorageTypeItems.Count ? StorageTypeItems[s.Type.Value] : $"Type {s.Type}";
                string iface = s.Interface.HasValue && s.Interface.Value < StorageInterfaceItems.Count ? StorageInterfaceItems[s.Interface.Value] : $"If {s.Interface}";
                string cap = s.Capacity.HasValue ? $" {FormatBytes(s.Capacity.Value)}" : "";
                MachineStorageDisplays.Add($"{type} ({iface}){cap}");
            }
        }
        catch(Exception ex) { _logger.LogError(ex, "Error loading storage for machine"); }
    }

    private async Task AddStorageAsync()
    {
        if(_editingId == null) return;
        try
        {
            await _apiClient.StorageByMachine.PostAsync(new StorageByMachineDto
            {
                MachineId = _editingId, Type = StorageTypeIndex, Interface = StorageInterfaceIndex,
                Capacity = StorageCapacity
            });
            StorageCapacity = null;
            await LoadMachineStorageAsync(_editingId.Value);
        }
        catch(Exception ex) { _logger.LogError(ex, "Error adding storage"); ErrorMessage = _localizer["FailedToSaveMachine"]; HasError = true; }
    }

    private async Task RemoveStorageByDisplayAsync(string? display)
    {
        if(display == null || _editingId == null) return;
        int idx = MachineStorageDisplays.IndexOf(display);
        if(idx >= 0 && idx < MachineStorage.Count && MachineStorage[idx].Id.HasValue)
        {
            try { await _apiClient.StorageByMachine[MachineStorage[idx].Id.Value].DeleteAsync(); await LoadMachineStorageAsync(_editingId.Value); }
            catch(Exception ex) { _logger.LogError(ex, "Error removing storage"); }
        }
    }

    // ======================== HELPERS ========================

    private static void RefreshAvailable<TDto>(List<TDto>? all, IEnumerable<int?> assignedIds, ObservableCollection<TDto> available)
        where TDto : class
    {
        available.Clear();
        if(all == null) return;

        HashSet<int> ids = new(assignedIds.Where(i => i.HasValue).Select(i => i!.Value));

        foreach(TDto item in all)
        {
            int? id = item switch
            {
                GpuDto g       => g.Id,
                ProcessorDto p => p.Id,
                SoundSynthDto s => s.Id,
                ScreenDto sc   => sc.Id,
                _              => null
            };

            if(id.HasValue && !ids.Contains(id.Value))
                available.Add(item);
        }
    }

    private static string FormatBytes(long bytes)
    {
        if(bytes >= 1073741824) return $"{bytes / 1073741824.0:F1} GiB";
        if(bytes >= 1048576) return $"{bytes / 1048576.0:F1} MiB";
        if(bytes >= 1024) return $"{bytes / 1024.0:F1} KiB";
        return $"{bytes} B";
    }

    private void ClearForm()
    {
        MachineName = string.Empty; Model = string.Empty; MachineTypeIndex = 0; Introduced = null;
        SelectedCompany = null; CompanySearchText = string.Empty; SelectedFamily = null;
        MachineGpus.Clear(); MachineGpuDisplays.Clear(); AvailableGpus.Clear(); SelectedAvailableGpu = null;
        MachineProcessors.Clear(); MachineProcessorDisplays.Clear(); AvailableProcessors.Clear(); SelectedAvailableProcessor = null; ProcessorSpeed = null;
        MachineSounds.Clear(); MachineSoundDisplays.Clear(); AvailableSounds.Clear(); SelectedAvailableSound = null;
        MachineScreens.Clear(); MachineScreenDisplays.Clear(); AvailableScreens.Clear(); SelectedAvailableScreen = null;
        MachineMemories.Clear(); MachineMemoryDisplays.Clear(); MemoryTypeIndex = 0; MemoryUsageIndex = 0; MemorySize = null; MemorySpeed = null;
        MachineStorage.Clear(); MachineStorageDisplays.Clear(); StorageTypeIndex = 0; StorageInterfaceIndex = 0; StorageCapacity = null;
        HasError = false; ErrorMessage = string.Empty;
    }

    private void PopulateForm(MachineDto machine)
    {
        MachineName    = machine.Name  ?? string.Empty;
        Model          = machine.Model ?? string.Empty;
        MachineTypeIndex = machine.Type ?? 0;
        Introduced     = machine.Introduced;

        if(machine.CompanyId is > 0 && _allCompanies != null)
        {
            CompanyDto? c = _allCompanies.FirstOrDefault(x => x.Id == machine.CompanyId);
            if(c != null) { CompanySearchText = c.Name ?? string.Empty; UpdateCompanySuggestions(CompanySearchText); SelectedCompany = CompanySuggestions.FirstOrDefault(x => x.Id == c.Id); }
            else { CompanySearchText = machine.Company ?? string.Empty; SelectedCompany = null; }
        }
        else { CompanySearchText = string.Empty; SelectedCompany = null; }

        SelectedFamily = machine.FamilyId.HasValue ? Families.FirstOrDefault(f => f.Id == machine.FamilyId.Value) : null;
    }

    private void OpenPhotos(MachineDto? machine)
    {
        if(machine?.Id == null) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MachineId, machine.Id.Value },
            { NavParamKeys.MachineName, machine.Name ?? string.Empty }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminMachinePhotosPage), parameters);
    }
}
