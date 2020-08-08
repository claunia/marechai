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
    public partial class Dump
    {
        AuthenticationState   _authState;
        bool                  _creating;
        bool                  _editing;
        bool                  _loaded;
        List<MediaViewModel>  _medias;
        DumpViewModel         _model;
        bool                  _unknownDumpDate;
        bool                  _unknownDumpingGroup;
        bool                  _unknownUser;
        List<ApplicationUser> _users;

        [Parameter]
        public ulong Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/dumps/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _medias    = await MediaService.GetTitlesAsync();
            _model     = _creating ? new DumpViewModel() : await Service.GetAsync(Id);
            _authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            _users = UserManager.Users.Select(u => new ApplicationUser
            {
                Id       = u.Id,
                UserName = u.UserName
            }).ToList();

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/dumps/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownUser         = string.IsNullOrWhiteSpace(_model.UserId);
            _unknownDumpingGroup = string.IsNullOrWhiteSpace(_model.DumpingGroup);
            _unknownDumpDate     = !_model.DumpDate.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/dumps");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownUser)
                _model.UserId = null;
            else if(string.IsNullOrWhiteSpace(_model.UserId))
                return;

            if(_unknownDumpingGroup)
                _model.DumpingGroup = null;
            else if(string.IsNullOrWhiteSpace(_model.DumpingGroup))
                return;

            if(_unknownDumpDate)
                _model.DumpDate = null;
            else if(_model.DumpDate?.Date >= DateTime.UtcNow.Date)
                return;

            if(string.IsNullOrWhiteSpace(_model.Dumper))
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

        void ValidateDumper(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Dumper must be smaller than 256 characters."], 256);

        void ValidateDumpingGroup(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Dumping group must be smaller than 256 characters."], 256);

        void ValidateDumpDate(ValidatorEventArgs e) => Validators.ValidateDate(e);
    }
}