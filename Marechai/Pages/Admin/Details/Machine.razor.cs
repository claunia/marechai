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
        bool                               _addingGpu;
        int?                               _addingGpuId;
        bool                               _addingSound;
        int?                               _addingSoundId;
        List<CompanyViewModel>             _companies;
        bool                               _creating;
        GpuByMachineViewModel              _currentGpuByMachine;
        SoundSynthByMachineViewModel       _currentSoundByMachine;
        bool                               _deleteInProgress;
        string                             _deleteText;
        string                             _deleteTitle;
        bool                               _deletingGpuByMachine;
        bool                               _deletingSoundByMachine;
        bool                               _editing;
        List<MachineFamilyViewModel>       _families;
        Modal                              _frmDelete;
        List<GpuViewModel>                 _gpus;
        bool                               _loaded;
        List<GpuByMachineViewModel>        _machineGpus;
        List<SoundSynthByMachineViewModel> _machineSound;
        MachineViewModel                   _model;
        bool                               _noFamily;
        bool                               _prototype;
        bool                               _savingGpu;
        bool                               _savingSound;
        List<SoundSynthViewModel>          _soundSynths;
        bool                               _unknownIntroduced;
        bool                               _unknownModel;
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

            _companies   = await CompaniesService.GetAsync();
            _families    = await MachineFamiliesService.GetAsync();
            _model       = _creating ? new MachineViewModel() : await Service.GetAsync(Id);
            _machineGpus = await GpuByMachineService.GetByMachine(Id);
            _gpus        = await GpusService.GetAsync();
            _soundSynths = await SoundSynthsService.GetAsync();

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
        }

        async Task ConfirmDeleteGpuByMachine()
        {
            if(_currentGpuByMachine is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await GpuByMachineService.DeleteAsync(_currentGpuByMachine.Id);
            _machineGpus = await GpuByMachineService.GetByMachine(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress       = false;
            _deletingGpuByMachine   = false;
            _currentGpuByMachine    = null;
            _deletingSoundByMachine = false;
        }

        async Task OnAddGpuClick()
        {
            _addingGpu   = true;
            _savingGpu   = false;
            _addingGpuId = null;
        }

        void CancelAddGpu()
        {
            _addingGpu   = false;
            _savingGpu   = false;
            _addingGpuId = null;
        }

        async Task ConfirmAddGpu()
        {
            if(_addingGpuId is null)
            {
                CancelAddGpu();

                return;
            }

            _savingGpu = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await GpuByMachineService.CreateAsync(_addingGpuId.Value, Id);
            _machineGpus = await GpuByMachineService.GetByMachine(Id);

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

        async Task OnAddSoundClick()
        {
            _addingSound   = true;
            _savingSound   = false;
            _addingSoundId = null;
        }

        void CancelAddSound()
        {
            _addingSound   = false;
            _savingSound   = false;
            _addingSoundId = null;
        }

        async Task ConfirmAddSound()
        {
            if(_addingSoundId is null)
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
    }
}