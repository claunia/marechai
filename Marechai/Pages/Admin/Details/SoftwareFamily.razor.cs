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
    public partial class SoftwareFamily
    {
        bool                                   _addingCompany;
        int?                                   _addingCompanyId;
        string                                 _addingCompanyRoleId;
        AuthenticationState                    _authState;
        List<CompanyViewModel>                 _companies;
        bool                                   _creating;
        CompanyBySoftwareFamilyViewModel       _currentCompanyBySoftwareFamily;
        bool                                   _deleteInProgress;
        string                                 _deleteText;
        string                                 _deleteTitle;
        bool                                   _deletingCompanyBySoftwareFamily;
        bool                                   _editing;
        Modal                                  _frmDelete;
        bool                                   _loaded;
        SoftwareFamilyViewModel                _model;
        List<DocumentRoleViewModel>            _roles;
        bool                                   _savingCompany;
        List<SoftwareFamilyViewModel>          _softwareFamilies;
        List<CompanyBySoftwareFamilyViewModel> _softwareFamilyCompanies;
        bool                                   _unknownIntroduced;
        bool                                   _unknownParent;

        [Parameter]
        public ulong Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/software_families/create",
                                                     StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _softwareFamilies        = await Service.GetAsync();
            _companies               = await CompaniesService.GetAsync();
            _roles                   = await DocumentRolesService.GetEnabledAsync();
            _model                   = _creating ? new SoftwareFamilyViewModel() : await Service.GetAsync(Id);
            _authState               = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _addingCompanyRoleId     = _roles.First().Id;
            _softwareFamilyCompanies = await CompaniesBySoftwareFamilyService.GetBySoftwareFamily(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/software_families/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownParent     = !_model.ParentId.HasValue;
            _unknownIntroduced = !_model.Introduced.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/software_families");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownParent)
                _model.ParentId = null;
            else if(_model.ParentId < 1)
                return;

            if(_unknownIntroduced)
                _model.Introduced = null;
            else if(_model.Introduced?.Date >= DateTime.UtcNow.Date)
                return;

            if(string.IsNullOrWhiteSpace(_model.Name))
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
            Validators.ValidateString(e, L["Name must be smaller than 256 characters."], 256);

        void ValidateIntroduced(ValidatorEventArgs e) => Validators.ValidateDate(e);

        void OnAddCompanyClick()
        {
            _addingCompany   = true;
            _savingCompany   = false;
            _addingCompanyId = _companies.First().Id;
        }

        void CancelAddCompany()
        {
            _addingCompany   = false;
            _savingCompany   = false;
            _addingCompanyId = null;
        }

        async Task ConfirmAddCompany()
        {
            if(_addingCompanyId is null ||
               _addingCompanyId <= 0)
            {
                CancelAddCompany();

                return;
            }

            _savingCompany = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await CompaniesBySoftwareFamilyService.CreateAsync(_addingCompanyId.Value, Id, _addingCompanyRoleId,
                                                               (await UserManager.GetUserAsync(_authState.User)).Id);

            _softwareFamilyCompanies = await CompaniesBySoftwareFamilyService.GetBySoftwareFamily(Id);

            _addingCompany   = false;
            _savingCompany   = false;
            _addingCompanyId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ShowCompanyDeleteModal(ulong itemId)
        {
            _currentCompanyBySoftwareFamily  = _softwareFamilyCompanies.FirstOrDefault(n => n.Id == itemId);
            _deletingCompanyBySoftwareFamily = true;
            _deleteTitle                     = L["Delete company from this software family"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the company {0} with role {1} from this software family?"],
                              _currentCompanyBySoftwareFamily?.Company, _currentCompanyBySoftwareFamily?.Role);

            _frmDelete.Show();
        }

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress                = false;
            _deletingCompanyBySoftwareFamily = false;
            _currentCompanyBySoftwareFamily  = null;
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_deletingCompanyBySoftwareFamily)
                await ConfirmDeleteCompanyByMachine();
        }

        async Task ConfirmDeleteCompanyByMachine()
        {
            if(_currentCompanyBySoftwareFamily is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await CompaniesBySoftwareFamilyService.DeleteAsync(_currentCompanyBySoftwareFamily.Id,
                                                               (await UserManager.GetUserAsync(_authState.User)).Id);

            _softwareFamilyCompanies = await CompaniesBySoftwareFamilyService.GetBySoftwareFamily(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }
    }
}