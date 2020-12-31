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
// Copyright © 2003-2021 Natalia Portillo
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
    public partial class Gpu
    {
        bool                           _addingResolution;
        int?                           _addingResolutionId;
        AuthenticationState            _authState;
        List<CompanyViewModel>         _companies;
        bool                           _creating;
        ResolutionByGpuViewModel       _currentResolution;
        bool                           _deleteInProgress;
        string                         _deleteText;
        string                         _deleteTitle;
        bool                           _editing;
        Modal                          _frmDelete;
        List<ResolutionByGpuViewModel> _gpuResolutions;
        bool                           _loaded;
        GpuViewModel                   _model;
        bool                           _prototype;
        List<ResolutionViewModel>      _resolutions;
        bool                           _savingResolution;
        bool                           _unknownCompany;
        bool                           _unknownDieSize;
        bool                           _unknownIntroduced;
        bool                           _unknownModelCode;
        bool                           _unknownPackage;
        bool                           _unknownProcess;
        bool                           _unknownProcessNm;
        bool                           _unknownTransistors;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/gpus/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _companies      = await CompaniesService.GetAsync();
            _model          = _creating ? new GpuViewModel() : await Service.GetAsync(Id);
            _resolutions    = await ResolutionsService.GetAsync();
            _gpuResolutions = await ResolutionsByGpuService.GetByGpu(Id);
            _authState      = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/gpus/edit/", StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownCompany     = !_model.CompanyId.HasValue;
            _unknownDieSize     = !_model.DieSize.HasValue;
            _unknownIntroduced  = !_model.Introduced.HasValue;
            _unknownModelCode   = string.IsNullOrWhiteSpace(_model.ModelCode);
            _unknownPackage     = string.IsNullOrWhiteSpace(_model.Package);
            _unknownProcess     = string.IsNullOrWhiteSpace(_model.Process);
            _unknownProcessNm   = !_model.ProcessNm.HasValue;
            _unknownTransistors = !_model.Transistors.HasValue;
            _prototype          = _model.Introduced?.Year == 1000;
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
                NavigationManager.ToBaseRelativePath("admin/document_people");

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

            if(_unknownPackage)
                _model.Package = null;
            else if(string.IsNullOrWhiteSpace(_model.Package))
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

        void ValidateName(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Name must be 50 characters or less."], 50);

        void ValidateModelCode(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Model code must be 45 characters or less."], 45);

        void ValidateIntroduced(ValidatorEventArgs e) => Validators.ValidateIntroducedDate(e);

        void ValidatePackage(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Package must be 45 characters or less."], 45);

        void ValidateLongBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateLong(e);

        void ValidateFloatBiggerThanOne(ValidatorEventArgs e) => Validators.ValidateFloat(e, 1);

        void ValidateProcess(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Process must be 45 characters or less."], 45);

        void ShowResolutionDeleteModal(long itemId)
        {
            _currentResolution = _gpuResolutions.FirstOrDefault(n => n.Id == itemId);
            _deleteTitle       = L["Delete resolution from this graphical processing unit"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the resolution {0} from this graphical processing unit?"],
                              _currentResolution);

            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_currentResolution is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await ResolutionsByGpuService.DeleteAsync(_currentResolution.Id,
                                                      (await UserManager.GetUserAsync(_authState.User)).Id);

            _gpuResolutions = await ResolutionsByGpuService.GetByGpu(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress  = false;
            _currentResolution = null;
        }

        void OnAddResolutionClick()
        {
            _addingResolution   = true;
            _savingResolution   = false;
            _addingResolutionId = _resolutions.First().Id;
        }

        void CancelAddResolution()
        {
            _addingResolution   = false;
            _savingResolution   = false;
            _addingResolutionId = null;
        }

        async Task ConfirmAddResolution()
        {
            if(_addingResolutionId is null ||
               _addingResolutionId <= 0)
            {
                CancelAddResolution();

                return;
            }

            _savingResolution = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await ResolutionsByGpuService.CreateAsync(_addingResolutionId.Value, Id,
                                                      (await UserManager.GetUserAsync(_authState.User)).Id);

            _gpuResolutions = await ResolutionsByGpuService.GetByGpu(Id);

            _addingResolution   = false;
            _savingResolution   = false;
            _addingResolutionId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }
    }
}