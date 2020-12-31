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
using Marechai.Database;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Admin.Details
{
    public partial class SoftwareVariant
    {
        bool                                    _addingCompany;
        int?                                    _addingCompanyId;
        string                                  _addingCompanyRoleId;
        AuthenticationState                     _authState;
        List<CompanyViewModel>                  _companies;
        bool                                    _creating;
        CompanyBySoftwareVariantViewModel       _currentCompanyBySoftwareVariant;
        bool                                    _deleteInProgress;
        string                                  _deleteText;
        string                                  _deleteTitle;
        bool                                    _deletingCompanyBySoftwareVariant;
        bool                                    _editing;
        Modal                                   _frmDelete;
        bool                                    _loaded;
        SoftwareVariantViewModel                _model;
        List<DocumentRoleViewModel>             _roles;
        bool                                    _savingCompany;
        List<CompanyBySoftwareVariantViewModel> _softwareVariantCompanies;
        List<SoftwareVariantViewModel>          _softwareVariants;
        List<SoftwareVersionViewModel>          _softwareVersions;
        bool                                    _unknownCatalogueNumber;
        bool                                    _unknownIntroduced;
        bool                                    _unknownMinimumMemory;
        bool                                    _unknownName;
        bool                                    _unknownParent;
        bool                                    _unknownPartNumber;
        bool                                    _unknownProductCode;
        bool                                    _unknownRecommendedMemory;
        bool                                    _unknownRequiredStorage;
        bool                                    _unknownSerialNumber;
        bool                                    _unknownVersion;

        [Parameter]
        public ulong Id { get; set; }

        uint Distribution
        {
            get => (uint)_model.DistributionMode;
            set => _model.DistributionMode = (DistributionMode)value;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/software_variants/create",
                                                     StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _softwareVariants         = await Service.GetAsync();
            _softwareVersions         = await SoftwareVersionsService.GetAsync();
            _companies                = await CompaniesService.GetAsync();
            _roles                    = await DocumentRolesService.GetEnabledAsync();
            _model                    = _creating ? new SoftwareVariantViewModel() : await Service.GetAsync(Id);
            _authState                = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _addingCompanyRoleId      = _roles.First().Id;
            _softwareVariantCompanies = await CompaniesBySoftwareVariantService.GetBySoftwareVariant(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/software_variants/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownName              = string.IsNullOrWhiteSpace(_model.Name);
            _unknownVersion           = string.IsNullOrWhiteSpace(_model.Version);
            _unknownParent            = !_model.ParentId.HasValue;
            _unknownMinimumMemory     = !_model.MinimumMemory.HasValue;
            _unknownRecommendedMemory = !_model.RecommendedMemory.HasValue;
            _unknownRequiredStorage   = !_model.RequiredStorage.HasValue;
            _unknownPartNumber        = string.IsNullOrWhiteSpace(_model.PartNumber);
            _unknownSerialNumber      = string.IsNullOrWhiteSpace(_model.SerialNumber);
            _unknownProductCode       = string.IsNullOrWhiteSpace(_model.ProductCode);
            _unknownCatalogueNumber   = string.IsNullOrWhiteSpace(_model.CatalogueNumber);
            _unknownIntroduced        = !_model.Introduced.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/software_variants");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownName)
                _model.Name = null;
            else if(string.IsNullOrWhiteSpace(_model.Name))
                return;

            if(_unknownVersion)
                _model.Version = null;
            else if(string.IsNullOrWhiteSpace(_model.Version))
                return;

            if(_unknownPartNumber)
                _model.PartNumber = null;
            else if(string.IsNullOrWhiteSpace(_model.PartNumber))
                return;

            if(_unknownSerialNumber)
                _model.SerialNumber = null;
            else if(string.IsNullOrWhiteSpace(_model.SerialNumber))
                return;

            if(_unknownProductCode)
                _model.ProductCode = null;
            else if(string.IsNullOrWhiteSpace(_model.ProductCode))
                return;

            if(_unknownCatalogueNumber)
                _model.CatalogueNumber = null;
            else if(string.IsNullOrWhiteSpace(_model.CatalogueNumber))
                return;

            if(_unknownParent)
                _model.ParentId = null;
            else if(_model.ParentId < 1)
                return;

            if(_unknownIntroduced)
                _model.Introduced = null;
            else if(_model.Introduced?.Date >= DateTime.UtcNow.Date)
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

        void ValidateVersion(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Version must be smaller than 256 characters."], 256);

        void ValidatePartNumber(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Part number must be smaller than 256 characters."], 256);

        void ValidateSerialNumber(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Serial number must be smaller than 256 characters."], 256);

        void ValidateProductCode(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Product number must be smaller than 256 characters."], 256);

        void ValidateCatalogueNumber(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Catalogue number must be smaller than 256 characters."], 256);

        void ValidateIntroduced(ValidatorEventArgs e) => Validators.ValidateDate(e);

        void ValidateUnsignedLongBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateUnsignedLong(e);

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

            await CompaniesBySoftwareVariantService.CreateAsync(_addingCompanyId.Value, Id, _addingCompanyRoleId,
                                                                (await UserManager.GetUserAsync(_authState.User)).Id);

            _softwareVariantCompanies = await CompaniesBySoftwareVariantService.GetBySoftwareVariant(Id);

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
            _currentCompanyBySoftwareVariant  = _softwareVariantCompanies.FirstOrDefault(n => n.Id == itemId);
            _deletingCompanyBySoftwareVariant = true;
            _deleteTitle                      = L["Delete company from this software variant"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the company {0} with role {1} from this software variant?"],
                              _currentCompanyBySoftwareVariant?.Company, _currentCompanyBySoftwareVariant?.Role);

            _frmDelete.Show();
        }

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress                 = false;
            _deletingCompanyBySoftwareVariant = false;
            _currentCompanyBySoftwareVariant  = null;
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_deletingCompanyBySoftwareVariant)
                await ConfirmDeleteCompanyByMachine();
        }

        async Task ConfirmDeleteCompanyByMachine()
        {
            if(_currentCompanyBySoftwareVariant is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await CompaniesBySoftwareVariantService.DeleteAsync(_currentCompanyBySoftwareVariant.Id,
                                                                (await UserManager.GetUserAsync(_authState.User)).Id);

            _softwareVariantCompanies = await CompaniesBySoftwareVariantService.GetBySoftwareVariant(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }
    }
}