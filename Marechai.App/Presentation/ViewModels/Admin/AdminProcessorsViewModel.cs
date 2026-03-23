#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminProcessorsViewModel : ObservableObject, IRegionAware
{
    private readonly ApiClient                          _apiClient;
    private readonly IJwtService                        _jwtService;
    private readonly IStringLocalizer                   _localizer;
    private readonly ILogger<AdminProcessorsViewModel>  _logger;
    private readonly ITokenService                      _tokenService;

    // --- List state ---
    [ObservableProperty]
    private ObservableCollection<ProcessorDto> _processors = [];

    [ObservableProperty]
    private ObservableCollection<ProcessorDto> _filteredProcessors = [];

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private ProcessorDto? _selectedProcessor;

    private List<ProcessorDto>? _allProcessors;

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

    private int? _editingProcessorId;

    // --- Form fields ---
    [ObservableProperty]
    private string _processorName = string.Empty;

    [ObservableProperty]
    private string _modelCode = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? _introduced;

    [ObservableProperty]
    private double? _speed;

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

    [ObservableProperty]
    private int? _cores;

    [ObservableProperty]
    private int? _threadsPerCore;

    [ObservableProperty]
    private int? _dataBus;

    [ObservableProperty]
    private int? _addressBus;

    [ObservableProperty]
    private int? _gprs;

    [ObservableProperty]
    private int? _gprSize;

    [ObservableProperty]
    private int? _fprs;

    [ObservableProperty]
    private int? _fprSize;

    [ObservableProperty]
    private int? _simdRegisters;

    [ObservableProperty]
    private int? _simdSize;

    [ObservableProperty]
    private double? _l1Instruction;

    [ObservableProperty]
    private double? _l1Data;

    [ObservableProperty]
    private double? _l2;

    [ObservableProperty]
    private double? _l3;

    // --- Company picker ---
    [ObservableProperty]
    private CompanyDto? _selectedCompany;

    [ObservableProperty]
    private string _companySearchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyDto> _companySuggestions = [];

    private List<CompanyDto>? _allCompanies;

    // --- Instruction set picker ---
    [ObservableProperty]
    private InstructionSetDto? _selectedInstructionSet;

    [ObservableProperty]
    private ObservableCollection<InstructionSetDto> _instructionSets = [];

    // --- ISA extensions by processor ---
    [ObservableProperty]
    private ObservableCollection<InstructionSetExtensionByProcessorDto> _processorExtensions = [];

    [ObservableProperty]
    private ObservableCollection<InstructionSetExtensionDto> _availableExtensions = [];

    [ObservableProperty]
    private InstructionSetExtensionDto? _selectedAvailableExtension;

    private List<InstructionSetExtensionDto>? _allAvailableExtensions;

    public AdminProcessorsViewModel(ApiClient                          apiClient,
                                    IJwtService                        jwtService,
                                    ITokenService                      tokenService,
                                    ILogger<AdminProcessorsViewModel>  logger,
                                    IStringLocalizer                   localizer)
    {
        _apiClient    = apiClient;
        _jwtService   = jwtService;
        _tokenService = tokenService;
        _logger       = logger;
        _localizer    = localizer;

        LoadProcessorsCommand    = new AsyncRelayCommand(LoadProcessorsAsync);
        OpenAddProcessorCommand  = new RelayCommand(OpenAddProcessor);
        OpenEditProcessorCommand = new RelayCommand<ProcessorDto>(OpenEditProcessor);
        DeleteProcessorCommand   = new AsyncRelayCommand<ProcessorDto>(DeleteProcessorAsync);
        SaveProcessorCommand     = new AsyncRelayCommand(SaveProcessorAsync);
        CancelEditCommand        = new RelayCommand(CancelEdit);
        AddExtensionCommand      = new AsyncRelayCommand(AddExtensionAsync);
        RemoveExtensionCommand   = new AsyncRelayCommand<InstructionSetExtensionByProcessorDto>(RemoveExtensionAsync);

        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand               LoadProcessorsCommand    { get; }
    public IRelayCommand                    OpenAddProcessorCommand  { get; }
    public IRelayCommand<ProcessorDto>      OpenEditProcessorCommand { get; }
    public IAsyncRelayCommand<ProcessorDto> DeleteProcessorCommand   { get; }
    public IAsyncRelayCommand               SaveProcessorCommand     { get; }
    public IRelayCommand                    CancelEditCommand        { get; }
    public IAsyncRelayCommand               AddExtensionCommand      { get; }
    public IAsyncRelayCommand<InstructionSetExtensionByProcessorDto> RemoveExtensionCommand { get; }

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
        {
            _ = LoadProcessorsCommand.ExecuteAsync(null);
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

            IsAdmin = roles.Contains("Uberadmin",      StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    // --- Load processors ---
    private async Task LoadProcessorsAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;
            Processors.Clear();

            List<ProcessorDto>? response = await _apiClient.Processors.GetAsync();
            _allProcessors = response;

            if(response != null)
                foreach(ProcessorDto proc in response)
                    Processors.Add(proc);

            ApplyFilter();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading processors");
            ErrorMessage = _localizer["FailedToLoadProcessors"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // --- Add ---
    private void OpenAddProcessor()
    {
        _editingProcessorId = null;
        EditPanelTitle      = _localizer["AddProcessorDialog_Title"];
        ClearForm();
        IsEditing = true;
    }

    // --- Edit ---
    private async void OpenEditProcessor(ProcessorDto? proc)
    {
        if(proc?.Id == null) return;

        try
        {
            // Fetch full details (list may return partial data)
            ProcessorDto? full = await _apiClient.Processors[proc.Id.Value].GetAsync();

            if(full == null) return;

            _editingProcessorId = proc.Id;
            EditPanelTitle      = _localizer["EditProcessorDialog_Title"];
            PopulateForm(full);

            if(proc.Id.HasValue)
                await LoadProcessorExtensionsAsync(proc.Id.Value);

            IsEditing = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading processor details for {Id}", proc.Id);
            ErrorMessage = _localizer["FailedToLoadProcessors"];
            HasError     = true;
        }
    }

    // --- Delete ---
    private async Task DeleteProcessorAsync(ProcessorDto? proc)
    {
        if(proc?.Id == null) return;

        try
        {
            await _apiClient.Processors[proc.Id.Value].DeleteAsync();
            await LoadProcessorsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting processor {Id}", proc.Id);
            ErrorMessage = _localizer["FailedToDeleteProcessor"];
            HasError     = true;
        }
    }

    // --- Save ---
    private async Task SaveProcessorAsync()
    {
        try
        {
            if(string.IsNullOrWhiteSpace(ProcessorName))
            {
                ErrorMessage = _localizer["NameIsRequired"];
                HasError     = true;

                return;
            }

            var dto = new ProcessorDto
            {
                Name             = ProcessorName,
                CompanyId        = SelectedCompany?.Id,
                ModelCode        = string.IsNullOrWhiteSpace(ModelCode) ? null : ModelCode,
                InstructionSetId = SelectedInstructionSet?.Id,
                Introduced       = Introduced,
                Speed            = Speed,
                Package          = string.IsNullOrWhiteSpace(Package) ? null : Package,
                Process          = string.IsNullOrWhiteSpace(Process) ? null : Process,
                ProcessNm        = ProcessNm.HasValue ? (float)ProcessNm.Value : null,
                DieSize          = DieSize.HasValue   ? (float)DieSize.Value   : null,
                Transistors      = Transistors,
                Cores            = Cores,
                ThreadsPerCore   = ThreadsPerCore,
                DataBus          = DataBus,
                AddressBus       = AddressBus,
                Gprs             = Gprs,
                GprSize          = GprSize,
                Fprs             = Fprs,
                FprSize          = FprSize,
                SimdRegisters    = SimdRegisters,
                SimdSize         = SimdSize,
                L1Instruction    = L1Instruction.HasValue ? (float)L1Instruction.Value : null,
                L1Data           = L1Data.HasValue        ? (float)L1Data.Value        : null,
                L2               = L2.HasValue            ? (float)L2.Value            : null,
                L3               = L3.HasValue            ? (float)L3.Value            : null
            };

            if(_editingProcessorId == null)
                await _apiClient.Processors.PostAsync(dto);
            else
            {
                dto.Id = _editingProcessorId;
                await _apiClient.Processors[_editingProcessorId.Value].PutAsync(dto);
            }

            IsEditing = false;
            ClearForm();
            await LoadProcessorsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving processor");
            ErrorMessage = _localizer["FailedToSaveProcessor"];
            HasError     = true;
        }
    }

    // --- Cancel ---
    private void CancelEdit()
    {
        IsEditing           = false;
        _editingProcessorId = null;
        ClearForm();
        HasError     = false;
        ErrorMessage = string.Empty;
    }

    // --- Filtering ---
    public void ApplyFilter()
    {
        FilteredProcessors.Clear();

        IEnumerable<ProcessorDto> source = (IEnumerable<ProcessorDto>?)_allProcessors ?? Processors;

        if(!string.IsNullOrWhiteSpace(FilterText))
            source = source.Where(p => (p.Name != null &&
                                        p.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (p.Company != null &&
                                        p.Company.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                                       (p.ModelCode != null &&
                                        p.ModelCode.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));

        foreach(ProcessorDto proc in source)
            FilteredProcessors.Add(proc);
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
            List<InstructionSetDto>? sets = await _apiClient.InstructionSets.GetAsync();
            InstructionSets.Clear();

            if(sets != null)
                foreach(InstructionSetDto s in sets)
                    InstructionSets.Add(s);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading instruction sets");
        }

        try
        {
            _allAvailableExtensions = await _apiClient.InstructionSetExtensions.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading instruction set extensions");
        }
    }

    // --- ISA extension management ---
    private async Task LoadProcessorExtensionsAsync(int processorId)
    {
        ProcessorExtensions.Clear();

        try
        {
            List<InstructionSetExtensionByProcessorDto>? exts =
                await _apiClient.Processor[processorId].InstructionSetExtensions.GetAsync();

            if(exts != null)
                foreach(InstructionSetExtensionByProcessorDto ext in exts)
                    ProcessorExtensions.Add(ext);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading extensions for processor {Id}", processorId);
        }

        RefreshAvailableExtensions();
    }

    private void RefreshAvailableExtensions()
    {
        AvailableExtensions.Clear();

        if(_allAvailableExtensions == null) return;

        HashSet<int> assignedIds = new(ProcessorExtensions
                                     .Where(e => e.ExtensionId.HasValue)
                                     .Select(e => e.ExtensionId!.Value));

        foreach(InstructionSetExtensionDto ext in _allAvailableExtensions)
            if(ext.Id.HasValue && !assignedIds.Contains(ext.Id.Value))
                AvailableExtensions.Add(ext);
    }

    private async Task AddExtensionAsync()
    {
        if(_editingProcessorId == null || SelectedAvailableExtension?.Id == null) return;

        try
        {
            var dto = new InstructionSetExtensionByProcessorDto
            {
                ProcessorId = _editingProcessorId.Value,
                ExtensionId = SelectedAvailableExtension.Id.Value
            };

            await _apiClient.InstructionSetExtensionsByProcessor.PostAsync(dto);
            SelectedAvailableExtension = null;
            await LoadProcessorExtensionsAsync(_editingProcessorId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding extension to processor");
            ErrorMessage = _localizer["FailedToSaveProcessor"];
            HasError     = true;
        }
    }

    private async Task RemoveExtensionAsync(InstructionSetExtensionByProcessorDto? ext)
    {
        if(ext?.Id == null || _editingProcessorId == null) return;

        try
        {
            await _apiClient.InstructionSetExtensionsByProcessor[ext.Id.Value].DeleteAsync();
            await LoadProcessorExtensionsAsync(_editingProcessorId.Value);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing extension from processor");
            ErrorMessage = _localizer["FailedToSaveProcessor"];
            HasError     = true;
        }
    }

    // --- Helpers ---
    private void ClearForm()
    {
        ProcessorName        = string.Empty;
        ModelCode             = string.Empty;
        Introduced            = null;
        Speed                 = null;
        Package               = string.Empty;
        Process               = string.Empty;
        ProcessNm             = null;
        DieSize               = null;
        Transistors           = null;
        Cores                 = null;
        ThreadsPerCore        = null;
        DataBus               = null;
        AddressBus            = null;
        Gprs                  = null;
        GprSize               = null;
        Fprs                  = null;
        FprSize               = null;
        SimdRegisters         = null;
        SimdSize              = null;
        L1Instruction         = null;
        L1Data                = null;
        L2                    = null;
        L3                    = null;
        SelectedCompany       = null;
        CompanySearchText     = string.Empty;
        SelectedInstructionSet = null;
        ProcessorExtensions.Clear();
        AvailableExtensions.Clear();
        SelectedAvailableExtension = null;
        HasError              = false;
        ErrorMessage          = string.Empty;
    }

    private void PopulateForm(ProcessorDto proc)
    {
        ProcessorName  = proc.Name      ?? string.Empty;
        ModelCode      = proc.ModelCode  ?? string.Empty;
        Introduced     = proc.Introduced;
        Speed          = proc.Speed;
        Package        = proc.Package   ?? string.Empty;
        Process        = proc.Process   ?? string.Empty;
        ProcessNm      = proc.ProcessNm;
        DieSize        = proc.DieSize;
        Transistors    = proc.Transistors;
        Cores          = proc.Cores;
        ThreadsPerCore = proc.ThreadsPerCore;
        DataBus        = proc.DataBus;
        AddressBus     = proc.AddressBus;
        Gprs           = proc.Gprs;
        GprSize        = proc.GprSize;
        Fprs           = proc.Fprs;
        FprSize        = proc.FprSize;
        SimdRegisters  = proc.SimdRegisters;
        SimdSize       = proc.SimdSize;
        L1Instruction  = proc.L1Instruction;
        L1Data         = proc.L1Data;
        L2             = proc.L2;
        L3             = proc.L3;

        // Company picker
        if(proc.CompanyId.HasValue && _allCompanies != null)
        {
            CompanyDto? company = _allCompanies.FirstOrDefault(c => c.Id == proc.CompanyId.Value);

            if(company != null)
            {
                CompanySearchText = company.Name ?? string.Empty;
                UpdateCompanySuggestions(CompanySearchText);
                SelectedCompany = CompanySuggestions.FirstOrDefault(c => c.Id == company.Id);
            }
            else
            {
                CompanySearchText = proc.Company ?? string.Empty;
                SelectedCompany   = null;
            }
        }
        else
        {
            CompanySearchText = string.Empty;
            SelectedCompany   = null;
        }

        // Instruction set picker
        SelectedInstructionSet = proc.InstructionSetId.HasValue
                                     ? InstructionSets.FirstOrDefault(s => s.Id == proc.InstructionSetId.Value)
                                     : null;
    }
}
