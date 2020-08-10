/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Admin.Details
{
    public partial class MagazineIssue
    {
        bool                                   _addingMachine;
        bool                                   _addingMachineFamily;
        int?                                   _addingMachineFamilyId;
        int?                                   _addingMachineId;
        AuthenticationState                    _authState;
        bool                                   _creating;
        MagazineByMachineViewModel             _currentMagazineByMachine;
        MagazineByMachineFamilyViewModel       _currentMagazineByMachineFamily;
        bool                                   _deleteInProgress;
        string                                 _deleteText;
        string                                 _deleteTitle;
        bool                                   _deletingMagazineByMachine;
        bool                                   _deletingMagazineByMachineFamily;
        bool                                   _editing;
        Modal                                  _frmDelete;
        bool                                   _loaded;
        List<MachineFamilyViewModel>           _machineFamilies;
        List<MachineViewModel>                 _machines;
        List<MagazineByMachineFamilyViewModel> _magazineMachineFamilies;
        List<MagazineByMachineViewModel>       _magazineMachines;
        List<MagazineViewModel>                _magazines;
        MagazineIssueViewModel                 _model;
        bool                                   _savingMachine;
        bool                                   _savingMachineFamily;
        bool                                   _unknownIssueNumber;
        bool                                   _unknownNativeCaption;
        bool                                   _unknownPages;
        bool                                   _unknownProductCode;
        bool                                   _unknownPublished;

        [Parameter]
        public long Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/magazine_issues/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _magazines               = await MagazinesService.GetTitlesAsync();
            _machineFamilies         = await MachineFamiliesService.GetAsync();
            _machines                = await MachinesService.GetAsync();
            _model                   = _creating ? new MagazineIssueViewModel() : await Service.GetAsync(Id);
            _authState               = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _addingMachineFamilyId   = _machineFamilies.First().Id;
            _magazineMachineFamilies = await MagazinesByMachineFamilyService.GetByMagazine(Id);
            _addingMachineId         = _machines.First().Id;
            _magazineMachines        = await MagazinesByMachineService.GetByMagazine(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/magazine_issues/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownProductCode   = string.IsNullOrWhiteSpace(_model.ProductCode);
            _unknownIssueNumber   = !_model.IssueNumber.HasValue;
            _unknownNativeCaption = string.IsNullOrWhiteSpace(_model.NativeCaption);
            _unknownPublished     = !_model.Published.HasValue;
            _unknownPages         = !_model.Pages.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/magazine_issues");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownNativeCaption)
                _model.NativeCaption = null;
            else if(string.IsNullOrWhiteSpace(_model.NativeCaption))
                return;

            if(_unknownPages)
                _model.Pages = null;
            else if(_model.Pages < 1)
                return;

            if(_unknownIssueNumber)
                _model.IssueNumber = null;
            else if(_model.IssueNumber < 1)
                return;

            if(_unknownPublished)
                _model.Published = null;
            else if(_model.Published?.Date >= DateTime.UtcNow.Date)
                return;

            // TODO: Recognize JAN-13, EAN-13, UPC, etc
            if(_unknownProductCode)
                _model.ProductCode = null;
            else if(string.IsNullOrWhiteSpace(_model.ProductCode))
                return;

            if(string.IsNullOrWhiteSpace(_model.Caption))
                return;

            if(_creating)
                Id = await Service.CreateAsync(_model, (await UserManager.GetUserAsync(_authState.User)).Id);
            else
                await Service.UpdateAsync(_model, (await UserManager.GetUserAsync(_authState.User)).Id);

            _editing  = false;
            _creating = false;
            _model    = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        void ValidateCaption(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Caption must be smaller than 256 characters."], 256);

        void ValidatePublished(ValidatorEventArgs e) => Validators.ValidateDate(e);

        void ValidateNativeCaption(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Native caption must be smaller than 256 characters."], 256);

        void ValidatePages(ValidatorEventArgs e) => Validators.ValidateShort(e, 1);

        void ValidateIssueNumber(ValidatorEventArgs e) => Validators.ValidateInteger(e, 1);

        void ValidateProductCode(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Product code must be smaller than 18 characters."], 18);

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress                = false;
            _deletingMagazineByMachineFamily = false;
            _currentMagazineByMachineFamily  = null;
            _deletingMagazineByMachine       = false;
            _currentMagazineByMachine        = null;
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_deletingMagazineByMachineFamily)
                await ConfirmDeleteMagazineByMachineFamily();
            else if(_deletingMagazineByMachine)
                await ConfirmDeleteMagazineByMachine();
        }

        void OnAddFamilyClick()
        {
            _addingMachineFamily   = true;
            _savingMachineFamily   = false;
            _addingMachineFamilyId = _machineFamilies.First().Id;
        }

        void CancelAddFamily()
        {
            _addingMachineFamily   = false;
            _savingMachineFamily   = false;
            _addingMachineFamilyId = null;
        }

        async Task ConfirmAddFamily()
        {
            if(_addingMachineFamilyId is null ||
               _addingMachineFamilyId <= 0)
            {
                CancelAddFamily();

                return;
            }

            _savingMachineFamily = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await MagazinesByMachineFamilyService.CreateAsync(_addingMachineFamilyId.Value, Id,
                                                              (await UserManager.GetUserAsync(_authState.User)).Id);

            _magazineMachineFamilies = await MagazinesByMachineFamilyService.GetByMagazine(Id);

            _addingMachineFamily   = false;
            _savingMachineFamily   = false;
            _addingMachineFamilyId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ShowMachineFamilyDeleteModal(long itemId)
        {
            _currentMagazineByMachineFamily  = _magazineMachineFamilies.FirstOrDefault(n => n.Id == itemId);
            _deletingMagazineByMachineFamily = true;
            _deleteTitle                     = L["Delete machine family from this magazine"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the machine family {0} from this magazine issue?"],
                              _currentMagazineByMachineFamily?.MachineFamily);

            _frmDelete.Show();
        }

        async Task ConfirmDeleteMagazineByMachineFamily()
        {
            if(_currentMagazineByMachineFamily is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await MagazinesByMachineFamilyService.DeleteAsync(_currentMagazineByMachineFamily.Id,
                                                              (await UserManager.GetUserAsync(_authState.User)).Id);

            _magazineMachineFamilies = await MagazinesByMachineFamilyService.GetByMagazine(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void OnAddMachineClick()
        {
            _addingMachine   = true;
            _savingMachine   = false;
            _addingMachineId = _machines.First().Id;
        }

        void CancelAddMachine()
        {
            _addingMachine   = false;
            _savingMachine   = false;
            _addingMachineId = null;
        }

        async Task ConfirmAddMachine()
        {
            if(_addingMachineId is null ||
               _addingMachineId <= 0)
            {
                CancelAddMachine();

                return;
            }

            _savingMachine = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await MagazinesByMachineService.CreateAsync(_addingMachineId.Value, Id,
                                                        (await UserManager.GetUserAsync(_authState.User)).Id);

            _magazineMachines = await MagazinesByMachineService.GetByMagazine(Id);

            _addingMachine   = false;
            _savingMachine   = false;
            _addingMachineId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ShowMachineDeleteModal(long itemId)
        {
            _currentMagazineByMachine  = _magazineMachines.FirstOrDefault(n => n.Id == itemId);
            _deletingMagazineByMachine = true;
            _deleteTitle               = L["Delete machine from this magazine"];

            _deleteText = string.Format(L["Are you sure you want to delete the machine {0} from this magazine issue?"],
                                        _currentMagazineByMachine?.Machine);

            _frmDelete.Show();
        }

        async Task ConfirmDeleteMagazineByMachine()
        {
            if(_currentMagazineByMachine is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await MagazinesByMachineService.DeleteAsync(_currentMagazineByMachine.Id,
                                                        (await UserManager.GetUserAsync(_authState.User)).Id);

            _magazineMachines = await MagazinesByMachineService.GetByMagazine(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }
    }
}