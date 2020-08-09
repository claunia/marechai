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
using Marechai.Database.Models;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Admin.Details
{
    public partial class Magazine
    {
        bool                             _addingCompany;
        int?                             _addingCompanyId;
        string                           _addingCompanyRoleId;
        AuthenticationState              _authState;
        List<DocumentCompanyViewModel>   _companies;
        List<Iso31661Numeric>            _countries;
        bool                             _creating;
        CompanyByMagazineViewModel       _currentCompanyByMagazine;
        bool                             _deleteInProgress;
        string                           _deleteText;
        string                           _deleteTitle;
        bool                             _deletingCompanyByMagazine;
        bool                             _editing;
        Modal                            _frmDelete;
        bool                             _loaded;
        List<CompanyByMagazineViewModel> _magazineCompanies;
        MagazineViewModel                _model;
        List<DocumentRoleViewModel>      _roles;
        bool                             _savingCompany;
        bool                             _unknownCountry;
        bool                             _unknownFirstPublication;
        bool                             _unknownIssn;
        bool                             _unknownNativeTitle;

        [Parameter]
        public long Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/magazines/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _countries           = await CountriesService.GetAsync();
            _companies           = await CompaniesService.GetAsync();
            _roles               = await DocumentRolesService.GetEnabledAsync();
            _model               = _creating ? new MagazineViewModel() : await Service.GetAsync(Id);
            _authState           = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _addingCompanyRoleId = _roles.First().Id;

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/magazines/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownCountry          = !_model.CountryId.HasValue;
            _unknownNativeTitle      = string.IsNullOrWhiteSpace(_model.NativeTitle);
            _unknownFirstPublication = !_model.FirstPublication.HasValue;
            _unknownIssn             = string.IsNullOrWhiteSpace(_model.Issn);
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
                NavigationManager.ToBaseRelativePath("admin/magazines");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownNativeTitle)
                _model.NativeTitle = null;
            else if(string.IsNullOrWhiteSpace(_model.NativeTitle))
                return;

            if(_unknownCountry)
                _model.CountryId = null;
            else if(_model.CountryId < 0)
                return;

            if(_unknownFirstPublication)
                _model.FirstPublication = null;
            else if(_model.FirstPublication?.Date >= DateTime.UtcNow.Date)
                return;

            if(_unknownIssn)
                _model.Issn = null;
            else if(string.IsNullOrWhiteSpace(_model.Issn))
                return;

            if(string.IsNullOrWhiteSpace(_model.Title))
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

        void ValidateTitle(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Title must be smaller than 256 characters."], 256);

        void ValidateFirstPublication(ValidatorEventArgs e) => Validators.ValidateDate(e);

        void ValidateNativeTitle(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Native title must be smaller than 256 characters."], 256);

        void ValidateIssn(ValidatorEventArgs e) => Validators.ValidateIssn(e);

        void OnAddCompanyClick()
        {
            _addingCompany   = true;
            _savingCompany   = false;
            _addingCompanyId = _companies.First().Id;
        }

        void CancelAddCpu()
        {
            _addingCompany   = false;
            _savingCompany   = false;
            _addingCompanyId = null;
        }

        async Task ConfirmAddCpu()
        {
            if(_addingCompanyId is null ||
               _addingCompanyId <= 0)
            {
                CancelAddCpu();

                return;
            }

            _savingCompany = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await CompaniesByMagazineService.CreateAsync(_addingCompanyId.Value, Id, _addingCompanyRoleId,
                                                         (await UserManager.GetUserAsync(_authState.User)).Id);

            _magazineCompanies = await CompaniesByMagazineService.GetByMagazine(Id);

            _addingCompany   = false;
            _savingCompany   = false;
            _addingCompanyId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ShowCpuDeleteModal(long itemId)
        {
            _currentCompanyByMagazine  = _magazineCompanies.FirstOrDefault(n => n.Id == itemId);
            _deletingCompanyByMagazine = true;
            _deleteTitle               = L["Delete company from this magazine"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the company {0} with role {1} from this magazine?"],
                              _currentCompanyByMagazine?.Company, _currentCompanyByMagazine?.Role);

            _frmDelete.Show();
        }

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress          = false;
            _deletingCompanyByMagazine = false;
            _currentCompanyByMagazine  = null;
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_deletingCompanyByMagazine)
                await ConfirmDeleteCpuByMachine();
        }

        async Task ConfirmDeleteCpuByMachine()
        {
            if(_currentCompanyByMagazine is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await CompaniesByMagazineService.DeleteAsync(_currentCompanyByMagazine.Id,
                                                         (await UserManager.GetUserAsync(_authState.User)).Id);

            _magazineCompanies = await CompaniesByMagazineService.GetByMagazine(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }
    }
}