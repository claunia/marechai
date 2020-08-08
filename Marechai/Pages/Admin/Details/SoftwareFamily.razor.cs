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
    public partial class SoftwareFamily
    {
        AuthenticationState           _authState;
        bool                          _creating;
        bool                          _editing;
        bool                          _loaded;
        SoftwareFamilyViewModel       _model;
        List<SoftwareFamilyViewModel> _softwareFamilies;
        bool                          _unknownIntroduced;
        bool                          _unknownParent;

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

            _softwareFamilies = await Service.GetAsync();
            _model            = _creating ? new SoftwareFamilyViewModel() : await Service.GetAsync(Id);
            _authState        = await AuthenticationStateProvider.GetAuthenticationStateAsync();

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
    }
}