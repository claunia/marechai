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
    public partial class Document
    {
        bool                             _addingCompany;
        int?                             _addingCompanyId;
        string                           _addingCompanyRoleId;
        AuthenticationState              _authState;
        List<DocumentCompanyViewModel>   _companies;
        List<Iso31661Numeric>            _countries;
        bool                             _creating;
        CompanyByDocumentViewModel       _currentCompanyByDocument;
        bool                             _deleteInProgress;
        string                           _deleteText;
        string                           _deleteTitle;
        bool                             _deletingCompanyByDocument;
        List<CompanyByDocumentViewModel> _documentCompanies;
        bool                             _editing;
        Modal                            _frmDelete;
        bool                             _loaded;
        DocumentViewModel                _model;
        List<DocumentRoleViewModel>      _roles;
        bool                             _savingCompany;
        bool                             _unknownCountry;
        bool                             _unknownNativeTitle;
        bool                             _unknownPublished;

        [Parameter]
        public long Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/documents/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _countries           = await CountriesService.GetAsync();
            _companies           = await CompaniesService.GetAsync();
            _roles               = await DocumentRolesService.GetEnabledAsync();
            _model               = _creating ? new DocumentViewModel() : await Service.GetAsync(Id);
            _authState           = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _addingCompanyRoleId = _roles.First().Id;
            _documentCompanies   = await CompaniesByDocumentService.GetByDocument(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/documents/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownCountry     = !_model.CountryId.HasValue;
            _unknownNativeTitle = string.IsNullOrWhiteSpace(_model.NativeTitle);
            _unknownPublished   = !_model.Published.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/documents");

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

            if(_unknownPublished)
                _model.Published = null;
            else if(_model.Published?.Date >= DateTime.UtcNow.Date)
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

        void ValidatePublished(ValidatorEventArgs e) => Validators.ValidateDate(e);

        void ValidateNativeTitle(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Native title must be smaller than 256 characters."], 256);

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

            await CompaniesByDocumentService.CreateAsync(_addingCompanyId.Value, Id, _addingCompanyRoleId,
                                                         (await UserManager.GetUserAsync(_authState.User)).Id);

            _documentCompanies = await CompaniesByDocumentService.GetByDocument(Id);

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
            _currentCompanyByDocument  = _documentCompanies.FirstOrDefault(n => n.Id == itemId);
            _deletingCompanyByDocument = true;
            _deleteTitle               = L["Delete company from this document"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the company {0} with role {1} from this document?"],
                              _currentCompanyByDocument?.Company, _currentCompanyByDocument?.Role);

            _frmDelete.Show();
        }

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress          = false;
            _deletingCompanyByDocument = false;
            _currentCompanyByDocument  = null;
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_deletingCompanyByDocument)
                await ConfirmDeleteCpuByMachine();
        }

        async Task ConfirmDeleteCpuByMachine()
        {
            if(_currentCompanyByDocument is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await CompaniesByDocumentService.DeleteAsync(_currentCompanyByDocument.Id,
                                                         (await UserManager.GetUserAsync(_authState.User)).Id);

            _documentCompanies = await CompaniesByDocumentService.GetByDocument(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }
    }
}