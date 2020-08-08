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
        AuthenticationState     _authState;
        bool                    _creating;
        bool                    _editing;
        bool                    _loaded;
        List<MagazineViewModel> _magazines;
        MagazineIssueViewModel  _model;
        bool                    _unknownIssueNumber;
        bool                    _unknownNativeCaption;
        bool                    _unknownPages;
        bool                    _unknownProductCode;
        bool                    _unknownPublished;

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

            _magazines = await MagazinesService.GetTitlesAsync();
            _model     = _creating ? new MagazineIssueViewModel() : await Service.GetAsync(Id);
            _authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

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
    }
}