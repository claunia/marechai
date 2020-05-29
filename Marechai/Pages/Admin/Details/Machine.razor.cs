using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Machine
    {
        bool                               _addingCpu;
        int?                               _addingCpuId;
        bool                               _addingGpu;
        int?                               _addingGpuId;
        bool                               _addingMemory;
        long?                              _addingMemorySize;
        double?                            _addingMemorySpeed;
        int                                _addingMemoryType;
        int                                _addingMemoryUsage;
        float?                             _addingProcessorSpeed;
        bool                               _addingScreen;
        int?                               _addingScreenId;
        bool                               _addingSound;
        int?                               _addingSoundId;
        bool                               _addingStorage;
        int                                _addingStorageInterface;
        long?                              _addingStorageSize;
        int                                _addingStorageType;
        List<CompanyViewModel>             _companies;
        List<ProcessorViewModel>           _cpus;
        bool                               _creating;
        ProcessorByMachineViewModel        _currentCpuByMachine;
        GpuByMachineViewModel              _currentGpuByMachine;
        MemoryByMachineViewModel           _currentMemoryByMachine;
        ScreenByMachineViewModel           _currentScreenByMachine;
        SoundSynthByMachineViewModel       _currentSoundByMachine;
        StorageByMachineViewModel          _currentStorageByMachine;
        bool                               _deleteInProgress;
        string                             _deleteText;
        string                             _deleteTitle;
        bool                               _deletingCpuByMachine;
        bool                               _deletingGpuByMachine;
        bool                               _deletingMemoryByMachine;
        bool                               _deletingScreenByMachine;
        bool                               _deletingSoundByMachine;
        bool                               _deletingStorageByMachine;
        bool                               _editing;
        List<MachineFamilyViewModel>       _families;
        Modal                              _frmDelete;
        List<GpuViewModel>                 _gpus;
        bool                               _loaded;
        List<ProcessorByMachineViewModel>  _machineCpus;
        List<GpuByMachineViewModel>        _machineGpus;
        List<MemoryByMachineViewModel>     _machineMemories;
        List<ScreenByMachineViewModel>     _machineScreens;
        List<SoundSynthByMachineViewModel> _machineSound;
        List<StorageByMachineViewModel>    _machineStorage;
        MachineViewModel                   _model;
        bool                               _noFamily;
        bool                               _prototype;
        bool                               _savingCpu;
        bool                               _savingGpu;
        bool                               _savingMemory;
        bool                               _savingScreen;
        bool                               _savingSound;
        bool                               _savingStorage;
        List<ScreenViewModel>              _screens;
        List<SoundSynthViewModel>          _soundSynths;
        bool                               _unknownIntroduced;
        bool                               _unknownMemorySize;
        bool                               _unknownMemorySpeed;
        bool                               _unknownModel;
        bool                               _unknownProcessorSpeed;
        bool                               _unknownStorageSize;
        [Parameter]
        public int Id { get; set; }

        int Type
        {
            get => (int)_model.Type;
            set => _model.Type = (MachineType)value;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/machines/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _companies       = await CompaniesService.GetAsync();
            _families        = await MachineFamiliesService.GetAsync();
            _model           = _creating ? new MachineViewModel() : await Service.GetAsync(Id);
            _machineGpus     = await GpusByMachineService.GetByMachine(Id);
            _machineCpus     = await ProcessorsByMachineService.GetByMachine(Id);
            _gpus            = await GpusService.GetAsync();
            _screens         = await ScreensService.GetAsync();
            _cpus            = await ProcessorsService.GetAsync();
            _soundSynths     = await SoundSynthsService.GetAsync();
            _machineMemories = await MemoriesByMachineService.GetByMachine(Id);
            _machineStorage  = await StorageByMachineService.GetByMachine(Id);
            _machineScreens  = await ScreensByMachineService.GetByMachine(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/machines/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _noFamily          = !_model.FamilyId.HasValue;
            _prototype         = _model.Introduced?.Year == 1000;
            _unknownIntroduced = !_model.Introduced.HasValue;
            _unknownModel      = string.IsNullOrWhiteSpace(_model.Model);
        }

        void OnEditClicked()
        {
            _editing = true;
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnCancelClicked()
        {
            _editing = false;

            if(_creating)
            {
                NavigationManager.ToBaseRelativePath("admin/machines");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_noFamily)
                _model.FamilyId = null;
            else if(_model.FamilyId < 0)
                return;

            if(_unknownIntroduced)
                _model.Introduced = null;
            else if(_prototype)
                _model.Introduced = new DateTime(1000, 1, 1);
            else if(_model.Introduced?.Date >= DateTime.UtcNow.Date)
                return;

            if(_unknownModel)
                _model.Model = null;
            else if(string.IsNullOrWhiteSpace(_model.Model))
                return;

            if(_creating)
                Id = await Service.CreateAsync(_model);
            else
                await Service.UpdateAsync(_model);

            _editing  = false;
            _creating = false;
            _model    = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        void ValidateName(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Name must contain less than 255 characters."], 255);

        void ValidateModel(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Model must contain less than 50 characters."], 50);

        void ValidateIntroduced(ValidatorEventArgs e) => Validators.ValidateIntroducedDate(e);

        void ShowGpuDeleteModal(long itemId)
        {
            _currentGpuByMachine  = _machineGpus.FirstOrDefault(n => n.Id == itemId);
            _deletingGpuByMachine = true;
            _deleteTitle          = L["Delete graphical processing unit from this machine"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the graphical processing unit {0} manufactured by {1} from this machine?"],
                              _currentGpuByMachine?.Name, _currentGpuByMachine?.CompanyName);

            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_deletingGpuByMachine)
                await ConfirmDeleteGpuByMachine();
            else if(_deletingSoundByMachine)
                await ConfirmDeleteSoundByMachine();
            else if(_deletingCpuByMachine)
                await ConfirmDeleteCpuByMachine();
            else if(_deletingMemoryByMachine)
                await ConfirmDeleteMemoryByMachine();
            else if(_deletingStorageByMachine)
                await ConfirmDeleteStorageByMachine();
            else if(_deletingScreenByMachine)
                await ConfirmDeleteScreenByMachine();
        }

        async Task ConfirmDeleteGpuByMachine()
        {
            if(_currentGpuByMachine is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await GpusByMachineService.DeleteAsync(_currentGpuByMachine.Id);
            _machineGpus = await GpusByMachineService.GetByMachine(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress         = false;
            _deletingGpuByMachine     = false;
            _currentGpuByMachine      = null;
            _deletingSoundByMachine   = false;
            _currentSoundByMachine    = null;
            _deletingCpuByMachine     = false;
            _currentCpuByMachine      = null;
            _deletingScreenByMachine  = false;
            _currentScreenByMachine   = null;
            _deletingMemoryByMachine  = false;
            _deletingStorageByMachine = false;
        }

        void OnAddGpuClick()
        {
            _addingGpu   = true;
            _savingGpu   = false;
            _addingGpuId = _gpus.First().Id;
        }

        void CancelAddGpu()
        {
            _addingGpu   = false;
            _savingGpu   = false;
            _addingGpuId = null;
        }

        async Task ConfirmAddGpu()
        {
            if(_addingGpuId is null ||
               _addingGpuId <= 0)
            {
                CancelAddGpu();

                return;
            }

            _savingGpu = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await GpusByMachineService.CreateAsync(_addingGpuId.Value, Id);
            _machineGpus = await GpusByMachineService.GetByMachine(Id);

            _addingGpu   = false;
            _savingGpu   = false;
            _addingGpuId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ShowSoundDeleteModal(long itemId)
        {
            _currentSoundByMachine  = _machineSound.FirstOrDefault(n => n.Id == itemId);
            _deletingSoundByMachine = true;
            _deleteTitle            = L["Delete sound synthesizer from this machine"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the sound synthesizer {0} manufactured by {1} from this machine?"],
                              _currentSoundByMachine?.Name, _currentSoundByMachine?.CompanyName);

            _frmDelete.Show();
        }

        async Task ConfirmDeleteSoundByMachine()
        {
            if(_currentSoundByMachine is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await SoundSynthsByMachineService.DeleteAsync(_currentSoundByMachine.Id);
            _machineSound = await SoundSynthsByMachineService.GetByMachine(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void OnAddSoundClick()
        {
            _addingSound   = true;
            _savingSound   = false;
            _addingSoundId = _soundSynths.First().Id;
        }

        void CancelAddSound()
        {
            _addingSound   = false;
            _savingSound   = false;
            _addingSoundId = null;
        }

        async Task ConfirmAddSound()
        {
            if(_addingSoundId is null ||
               _addingSoundId <= 0)
            {
                CancelAddSound();

                return;
            }

            _savingSound = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await SoundSynthsByMachineService.CreateAsync(_addingSoundId.Value, Id);
            _machineSound = await SoundSynthsByMachineService.GetByMachine(Id);

            _addingSound   = false;
            _savingSound   = false;
            _addingSoundId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ShowCpuDeleteModal(long itemId)
        {
            _currentCpuByMachine  = _machineCpus.FirstOrDefault(n => n.Id == itemId);
            _deletingCpuByMachine = true;
            _deleteTitle          = L["Delete processor from this machine"];

            string speed;

            speed = _currentCpuByMachine?.Speed == null ? L["Unknown (processor by machine speed)"]
                        : string.Format(L["{0:F3} MHz"], _currentCpuByMachine?.Speed);

            _deleteText =
                string.Format(L["Are you sure you want to delete the graphical processing unit {0} with speed {2} manufactured by {1} from this machine?"],
                              _currentCpuByMachine?.Name, _currentCpuByMachine?.CompanyName, speed);

            _frmDelete.Show();
        }

        async Task ConfirmDeleteCpuByMachine()
        {
            if(_currentCpuByMachine is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await ProcessorsByMachineService.DeleteAsync(_currentCpuByMachine.Id);
            _machineCpus = await ProcessorsByMachineService.GetByMachine(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void OnAddCpuClick()
        {
            _addingCpu             = true;
            _savingCpu             = false;
            _addingCpuId           = _cpus.First().Id;
            _addingProcessorSpeed  = 0;
            _unknownProcessorSpeed = true;
        }

        void CancelAddCpu()
        {
            _addingCpu   = false;
            _savingCpu   = false;
            _addingCpuId = null;
        }

        async Task ConfirmAddCpu()
        {
            if(_addingCpuId is null ||
               _addingCpuId <= 0)
            {
                CancelAddCpu();

                return;
            }

            _savingGpu = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await ProcessorsByMachineService.CreateAsync(_addingCpuId.Value, Id,
                                                         _unknownProcessorSpeed ? null : _addingProcessorSpeed);

            _machineCpus = await ProcessorsByMachineService.GetByMachine(Id);

            _addingCpu   = false;
            _savingCpu   = false;
            _addingCpuId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ValidateProcessorSpeed(ValidatorEventArgs e)
        {
            if(!(e.Value is float item))
            {
                e.Status = ValidationStatus.Error;

                return;
            }

            e.Status = item > 0 ? ValidationStatus.Success : ValidationStatus.Error;
        }

        void ShowMemoryDeleteModal(long itemId)
        {
            _currentMemoryByMachine  = _machineMemories.FirstOrDefault(n => n.Id == itemId);
            _deletingMemoryByMachine = true;
            _deleteTitle             = L["Delete memory from this machine"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the memory type {0} with usage {1} from this machine?"],
                              _currentMemoryByMachine?.Type, _currentMemoryByMachine?.Usage);

            _frmDelete.Show();
        }

        async Task ConfirmDeleteMemoryByMachine()
        {
            if(_currentMemoryByMachine is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await MemoriesByMachineService.DeleteAsync(_currentMemoryByMachine.Id);
            _machineMemories = await MemoriesByMachineService.GetByMachine(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void OnAddMemoryClick()
        {
            _addingMemory       = true;
            _savingMemory       = false;
            _addingMemorySpeed  = 0;
            _unknownMemorySpeed = true;
            _addingMemorySize   = 0;
            _unknownMemorySize  = true;
        }

        void CancelAddMemory()
        {
            _addingMemory = false;
            _savingMemory = false;
        }

        async Task ConfirmAddMemory()
        {
            // TODO: Validation

            _savingMemory = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await MemoriesByMachineService.CreateAsync(Id, (MemoryType)_addingMemoryType,
                                                       (MemoryUsage)_addingMemoryUsage,
                                                       _unknownMemorySize ? null : _addingMemorySize,
                                                       _unknownMemorySpeed ? null : _addingMemorySpeed);

            _machineMemories = await MemoriesByMachineService.GetByMachine(Id);

            _addingMemory = false;
            _savingMemory = false;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ValidateMemorySpeed(ValidatorEventArgs e)
        {
            if(!(e.Value is double item))
            {
                e.Status = ValidationStatus.Error;

                return;
            }

            e.Status = item > 0 ? ValidationStatus.Success : ValidationStatus.Error;
        }

        void ValidateMemorySize(ValidatorEventArgs e)
        {
            if(!(e.Value is long item))
            {
                e.Status = ValidationStatus.Error;

                return;
            }

            e.Status = item > 0 ? ValidationStatus.Success : ValidationStatus.Error;
        }

        void ShowStorageDeleteModal(long itemId)
        {
            _currentStorageByMachine  = _machineStorage.FirstOrDefault(n => n.Id == itemId);
            _deletingStorageByMachine = true;
            _deleteTitle              = L["Delete storage from this machine"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the storage type {0} with interface {1} from this machine?"],
                              _currentStorageByMachine?.Type, _currentStorageByMachine?.Interface);

            _frmDelete.Show();
        }

        async Task ConfirmDeleteStorageByMachine()
        {
            if(_currentStorageByMachine is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await StorageByMachineService.DeleteAsync(_currentStorageByMachine.Id);
            _machineStorage = await StorageByMachineService.GetByMachine(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void OnAddStorageClick()
        {
            _addingStorage      = true;
            _savingStorage      = false;
            _addingStorageSize  = 0;
            _unknownStorageSize = true;
        }

        void CancelAddStorage()
        {
            _addingStorage = false;
            _savingStorage = false;
        }

        async Task ConfirmAddStorage()
        {
            // TODO: Validation

            _savingStorage = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await StorageByMachineService.CreateAsync(Id, (StorageType)_addingStorageType,
                                                      (StorageInterface)_addingStorageInterface,
                                                      _unknownStorageSize ? null : _addingStorageSize);

            _machineStorage = await StorageByMachineService.GetByMachine(Id);

            _addingStorage = false;
            _savingStorage = false;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ValidateStorageSize(ValidatorEventArgs e)
        {
            if(!(e.Value is long item))
            {
                e.Status = ValidationStatus.Error;

                return;
            }

            e.Status = item > 0 ? ValidationStatus.Success : ValidationStatus.Error;
        }

        void ShowScreenDeleteModal(long itemId)
        {
            _currentScreenByMachine  = _machineScreens.FirstOrDefault(n => n.Id == itemId);
            _deletingScreenByMachine = true;
            _deleteTitle             = L["Delete screen from this machine"];
            _deleteText              = L["Are you sure you want to delete the selected screen from this machine?"];

            _frmDelete.Show();
        }

        async Task ConfirmDeleteScreenByMachine()
        {
            if(_currentScreenByMachine is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await ScreensByMachineService.DeleteAsync(_currentScreenByMachine.Id);
            _machineScreens = await ScreensByMachineService.GetByMachine(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void OnAddScreenClick()
        {
            _addingScreen   = true;
            _savingScreen   = false;
            _addingScreenId = _screens.First(s => _machineScreens.All(m => m.ScreenId == s.Id)).Id;
        }

        void CancelAddScreen()
        {
            _addingScreen   = false;
            _savingScreen   = false;
            _addingScreenId = null;
        }

        async Task ConfirmAddScreen()
        {
            if(_addingScreenId is null ||
               _addingScreenId <= 0)
            {
                CancelAddScreen();

                return;
            }

            _savingScreen = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await ScreensByMachineService.CreateAsync(_addingScreenId.Value, Id);
            _machineScreens = await ScreensByMachineService.GetByMachine(Id);

            _addingScreen   = false;
            _savingScreen   = false;
            _addingScreenId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }
    }
}