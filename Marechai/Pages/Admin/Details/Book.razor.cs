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
    public partial class Book
    {
        bool                           _addingCompany;
        int?                           _addingCompanyId;
        string                         _addingCompanyRoleId;
        AuthenticationState            _authState;
        List<CompanyByBookViewModel>   _bookCompanies;
        List<DocumentCompanyViewModel> _companies;
        List<Iso31661Numeric>          _countries;
        bool                           _creating;
        CompanyByBookViewModel         _currentCompanyByBook;
        bool                           _deleteInProgress;
        string                         _deleteText;
        string                         _deleteTitle;
        bool                           _deletingCompanyByBook;
        bool                           _editing;
        Modal                          _frmDelete;
        bool                           _loaded;
        BookViewModel                  _model;
        List<DocumentRoleViewModel>    _roles;
        bool                           _savingCompany;
        bool                           _unknownCountry;
        bool                           _unknownEdition;
        bool                           _unknownIsbn;
        bool                           _unknownNativeTitle;
        bool                           _unknownPages;
        bool                           _unknownPublished;

        [Parameter]
        public long Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/books/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _countries           = await CountriesService.GetAsync();
            _companies           = await CompaniesService.GetAsync();
            _roles               = await DocumentRolesService.GetEnabledAsync();
            _model               = _creating ? new BookViewModel() : await Service.GetAsync(Id);
            _authState           = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _addingCompanyRoleId = _roles.First().Id;

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/books/edit/",
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
            _unknownIsbn        = string.IsNullOrWhiteSpace(_model.Isbn);
            _unknownPages       = !_model.Pages.HasValue;
            _unknownEdition     = !_model.Edition.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/books");

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

            if(_unknownPages)
                _model.Pages = null;
            else if(_model.Pages < 1)
                return;

            if(_unknownEdition)
                _model.Edition = null;
            else if(_model.Edition < 1)
                return;

            if(_unknownPublished)
                _model.Published = null;
            else if(_model.Published?.Date >= DateTime.UtcNow.Date)
                return;

            if(_unknownIsbn)
                _model.Isbn = null;
            else if(string.IsNullOrWhiteSpace(_model.Isbn))
                return;

            // Convert ISBN-10 to ISBN-13
            if(_model.Isbn?.Length == 10)
            {
                char[] newIsbn = new char[13];
                Array.Copy(_model.Isbn.ToCharArray(), 0, newIsbn, 3, 9);
                newIsbn[0] = '9';
                newIsbn[1] = '7';
                newIsbn[2] = '8';

                int sum = (newIsbn[0] - 0x30) + ((newIsbn[1] - 0x30) * 3) + (newIsbn[2] - 0x30) +
                          ((newIsbn[3] - 0x30) * 3) + (newIsbn[4] - 0x30) + ((newIsbn[5] - 0x30) * 3) +
                          (newIsbn[6] - 0x30) + ((newIsbn[7] - 0x30) * 3) + (newIsbn[8] - 0x30) +
                          ((newIsbn[9] - 0x30) * 3) + (newIsbn[10] - 0x30) + ((newIsbn[11] - 0x30) * 3);

                int modulo = sum % 10;

                if(modulo != 0)
                    modulo = 10 - modulo;

                newIsbn[12] = (char)(modulo + 0x30);

                _model.Isbn = new string(newIsbn);
            }

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

        void ValidatePages(ValidatorEventArgs e) => Validators.ValidateShort(e, 1);

        void ValidateEdition(ValidatorEventArgs e) => Validators.ValidateInteger(e, 1);

        void ValidateIsbn(ValidatorEventArgs e) => Validators.ValidateIsbn(e);

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

            await CompaniesByBookService.CreateAsync(_addingCompanyId.Value, Id, _addingCompanyRoleId,
                                                     (await UserManager.GetUserAsync(_authState.User)).Id);

            _bookCompanies = await CompaniesByBookService.GetByBook(Id);

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
            _currentCompanyByBook  = _bookCompanies.FirstOrDefault(n => n.Id == itemId);
            _deletingCompanyByBook = true;
            _deleteTitle           = L["Delete company from this book"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the company {0} with role {1} from this book?"],
                              _currentCompanyByBook?.Company, _currentCompanyByBook?.Role);

            _frmDelete.Show();
        }

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress      = false;
            _deletingCompanyByBook = false;
            _currentCompanyByBook  = null;
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_deletingCompanyByBook)
                await ConfirmDeleteCpuByMachine();
        }

        async Task ConfirmDeleteCpuByMachine()
        {
            if(_currentCompanyByBook is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await CompaniesByBookService.DeleteAsync(_currentCompanyByBook.Id,
                                                     (await UserManager.GetUserAsync(_authState.User)).Id);

            _bookCompanies = await CompaniesByBookService.GetByBook(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }
    }
}