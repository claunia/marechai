using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Processor
    {
        bool                   _addingExtension;
        int?                   _addingExtensionId;
        List<CompanyViewModel> _companies;
        bool                   _creating;

        InstructionSetExtensionByProcessorViewModel   _currentInstructionByMachine;
        bool                                          _deleteInProgress;
        string                                        _deleteText;
        string                                        _deleteTitle;
        bool                                          _editing;
        Modal                                         _frmDelete;
        List<Database.Models.InstructionSetExtension> _instructionSetExtensions;
        List<Database.Models.InstructionSet>          _instructionSets;
        bool                                          _loaded;
        ProcessorViewModel                            _model;

        List<InstructionSetExtensionByProcessorViewModel> _processorExtensions;
        bool                                              _prototype;
        bool                                              _savingExtension;
        bool                                              _unknownAddressBus;
        bool                                              _unknownCompany;
        bool                                              _unknownCores;
        bool                                              _unknownDataBus;
        bool                                              _unknownDieSize;
        bool                                              _unknownFprs;
        bool                                              _unknownFprSize;
        bool                                              _unknownGprs;
        bool                                              _unknownGprSize;
        bool                                              _unknownInstructionSet;
        bool                                              _unknownIntroduced;
        bool                                              _unknownL1Data;
        bool                                              _unknownL1Instruction;
        bool                                              _unknownL2;
        bool                                              _unknownL3;
        bool                                              _unknownModelCode;
        bool                                              _unknownPackage;
        bool                                              _unknownProcess;
        bool                                              _unknownProcessNm;
        bool                                              _unknownSimdRegisters;
        bool                                              _unknownSimdSize;
        bool                                              _unknownSpeed;
        bool                                              _unknownThreadsPerCore;
        bool                                              _unknownTransistors;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/processors/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _companies                = await CompaniesService.GetAsync();
            _instructionSets          = await InstructionSetsService.GetAsync();
            _model                    = _creating ? new ProcessorViewModel() : await Service.GetAsync(Id);
            _instructionSetExtensions = await InstructionSetExtensionsService.GetAsync();
            _processorExtensions      = await InstructionSetExtensionsByProcessorService.GetByProcessor(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/processors/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownAddressBus     = !_model.AddrBus.HasValue;
            _unknownCompany        = !_model.CompanyId.HasValue;
            _unknownCores          = !_model.Cores.HasValue;
            _unknownDataBus        = !_model.DataBus.HasValue;
            _unknownDieSize        = !_model.DieSize.HasValue;
            _unknownFprs           = !_model.Fprs.HasValue;
            _unknownFprSize        = !_model.FprSize.HasValue;
            _unknownGprs           = !_model.Gprs.HasValue;
            _unknownGprSize        = !_model.GprSize.HasValue;
            _unknownInstructionSet = !_model.InstructionSetId.HasValue;
            _unknownIntroduced     = !_model.Introduced.HasValue;
            _unknownL1Data         = !_model.L1Data.HasValue;
            _unknownL1Instruction  = !_model.L1Instruction.HasValue;
            _unknownL2             = !_model.L2.HasValue;
            _unknownL3             = !_model.L3.HasValue;
            _unknownModelCode      = string.IsNullOrWhiteSpace(_model.ModelCode);
            _unknownPackage        = string.IsNullOrWhiteSpace(_model.Package);
            _unknownProcess        = string.IsNullOrWhiteSpace(_model.Process);
            _unknownProcessNm      = !_model.ProcessNm.HasValue;
            _unknownSimdRegisters  = !_model.SimdRegisters.HasValue;
            _unknownSimdSize       = !_model.SimdSize.HasValue;
            _unknownSpeed          = !_model.Speed.HasValue;
            _unknownThreadsPerCore = !_model.ThreadsPerCore.HasValue;
            _unknownTransistors    = !_model.Transistors.HasValue;
            _prototype             = _model.Introduced?.Year == 1000;
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
                NavigationManager.ToBaseRelativePath("admin/processors");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownCompany)
                _model.CompanyId = null;
            else if(_model.CompanyId < 0)
                return;

            if(_unknownModelCode)
                _model.ModelCode = null;
            else if(string.IsNullOrWhiteSpace(_model.ModelCode))
                return;

            if(_unknownIntroduced)
                _model.Introduced = null;
            else if(_prototype)
                _model.Introduced = new DateTime(1000, 1, 1);
            else if(_model.Introduced >= DateTime.UtcNow.Date)
                return;

            if(_unknownInstructionSet)
                _model.InstructionSetId = null;
            else if(_model.InstructionSetId < 0)
                return;

            if(_unknownSpeed)
                _model.Speed = null;
            else if(_model.Speed < 0)
                return;

            if(_unknownPackage)
                _model.Package = null;
            else if(string.IsNullOrWhiteSpace(_model.Package))
                return;

            if(_unknownGprs)
                _model.Gprs = null;
            else if(_model.Gprs < 0)
                return;

            if(_unknownGprSize)
                _model.GprSize = null;
            else if(_model.GprSize < 0)
                return;

            if(_unknownFprs)
                _model.Fprs = null;
            else if(_model.Fprs < 0)
                return;

            if(_unknownFprSize)
                _model.FprSize = null;
            else if(_model.FprSize < 0)
                return;

            if(_unknownSimdRegisters)
                _model.SimdRegisters = null;
            else if(_model.SimdRegisters < 0)
                return;

            if(_unknownSimdSize)
                _model.SimdSize = null;
            else if(_model.SimdSize < 0)
                return;

            if(_unknownCores)
                _model.Cores = null;
            else if(_model.Cores < 1)
                return;

            if(_unknownThreadsPerCore)
                _model.ThreadsPerCore = null;
            else if(_model.ThreadsPerCore < 1)
                return;

            if(_unknownProcess)
                _model.Process = null;
            else if(string.IsNullOrWhiteSpace(_model.Process))
                return;

            if(_unknownProcessNm)
                _model.ProcessNm = null;
            else if(_model.ProcessNm < 1)
                return;

            if(_unknownDieSize)
                _model.DieSize = null;
            else if(_model.DieSize < 1)
                return;

            if(_unknownTransistors)
                _model.Transistors = null;
            else if(_model.Transistors < 0)
                return;

            if(_unknownDataBus)
                _model.DataBus = null;
            else if(_model.DataBus < 1)
                return;

            if(_unknownAddressBus)
                _model.AddrBus = null;
            else if(_model.AddrBus < 1)
                return;

            if(_unknownL1Instruction)
                _model.L1Instruction = null;
            else if(_model.L1Instruction < 0)
                return;

            if(_unknownL1Data)
                _model.L1Data = null;
            else if(_model.L1Data < 0)
                return;

            if(_unknownL2)
                _model.L2 = null;
            else if(_model.L2 < 0)
                return;

            if(_unknownL3)
                _model.FprSize = null;
            else if(_model.L3 < 0)
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
            Validators.ValidateString(e, L["Name must be 50 characters or less."], 50);

        void ValidateModelCode(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Model code must be 45 characters or less."], 45);

        void ValidateIntroduced(ValidatorEventArgs e) => Validators.ValidateIntroducedDate(e);

        void ValidateDoubleBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateDouble(e);

        void ValidateIntegerBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateInteger(e);

        void ValidatePackage(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Package must be 45 characters or less."], 45);

        void ValidateFloatBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateFloat(e);

        void ValidateIntegerBiggerThanOne(ValidatorEventArgs e) => Validators.ValidateInteger(e, 1);

        void ValidateLongBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateLong(e);

        void ValidateFloatBiggerThanOne(ValidatorEventArgs e) => Validators.ValidateFloat(e, 1);

        void ValidateProcess(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Process must be 45 characters or less."], 45);

        void ShowExtensionDeleteModal(long itemId)
        {
            _currentInstructionByMachine = _processorExtensions.FirstOrDefault(n => n.Id == itemId);
            _deleteTitle                 = L["Delete instruction set extension from this processor"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the instruction set extension {0} from this processor?"],
                              _currentInstructionByMachine?.Extension);

            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_currentInstructionByMachine is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await InstructionSetExtensionsByProcessorService.DeleteAsync(_currentInstructionByMachine.Id);
            _processorExtensions = await InstructionSetExtensionsByProcessorService.GetByProcessor(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress            = false;
            _currentInstructionByMachine = null;
        }

        void OnAddExtensionClick()
        {
            _addingExtension   = true;
            _savingExtension   = false;
            _addingExtensionId = _instructionSetExtensions.First().Id;
        }

        void CancelAddExtension()
        {
            _addingExtension   = false;
            _savingExtension   = false;
            _addingExtensionId = null;
        }

        async Task ConfirmAddExtension()
        {
            if(_addingExtensionId is null ||
               _addingExtensionId <= 0)
            {
                CancelAddExtension();

                return;
            }

            _savingExtension = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await InstructionSetExtensionsByProcessorService.CreateAsync(Id, _addingExtensionId.Value);
            _processorExtensions = await InstructionSetExtensionsByProcessorService.GetByProcessor(Id);

            _addingExtension   = false;
            _savingExtension   = false;
            _addingExtensionId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }
    }
}