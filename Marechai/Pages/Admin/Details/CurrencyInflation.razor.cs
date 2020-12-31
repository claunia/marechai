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
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Admin.Details
{
    public partial class CurrencyInflation
    {
        AuthenticationState        _authState;
        bool                       _creating;
        List<Iso4217>              _currencies;
        bool                       _editing;
        bool                       _loaded;
        CurrencyInflationViewModel _model;

        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/currency_inflation/create",
                                                     StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _currencies = await CurrenciesService.GetAsync();
            _model      = _creating ? new CurrencyInflationViewModel() : await Service.GetAsync(Id);
            _authState  = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/currency_inflation/edit/",
                                                                 StringComparison.InvariantCulture);

            StateHasChanged();
        }

        void OnEditClicked()
        {
            _editing = true;
            StateHasChanged();
        }

        async void OnCancelClicked()
        {
            _editing = false;

            if(_creating)
            {
                NavigationManager.ToBaseRelativePath("admin/currency_inflation");

                return;
            }

            _model = await Service.GetAsync(Id);
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(string.IsNullOrWhiteSpace(_model.CurrencyCode))
                return;

            if(_model.Year > DateTime.UtcNow.Year)
                return;

            if(_model.Inflation == 0)
                return;

            if(_creating)
                Id = await Service.CreateAsync(_model, (await UserManager.GetUserAsync(_authState.User)).Id);
            else
                await Service.UpdateAsync(_model, (await UserManager.GetUserAsync(_authState.User)).Id);

            _editing  = false;
            _creating = false;
            _model    = await Service.GetAsync(Id);
            StateHasChanged();
        }

        void ValidateYear(ValidatorEventArgs e)
        {
            e.Status = ValidationStatus.Success;

            if((uint)e.Value <= DateTime.UtcNow.Year)
                return;

            e.Status    = ValidationStatus.Error;
            e.ErrorText = L["Year must be before current one."];
        }

        void ValidateInflation(ValidatorEventArgs e)
        {
            e.Status = ValidationStatus.Success;

            if((float)e.Value != 0)
                return;

            e.Status    = ValidationStatus.Error;
            e.ErrorText = L["Inflation must be bigger than 0."];
        }
    }
}