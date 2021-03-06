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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Admin
{
    public partial class Dumps
    {
        DumpViewModel       _currentDump;
        bool                _deleteInProgress;
        List<DumpViewModel> _dumps;
        Modal               _frmDelete;
        bool                _loaded;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _dumps  = await Service.GetAsync();
            _loaded = true;
            StateHasChanged();
        }

        void ShowModal(ulong itemId)
        {
            _currentDump = _dumps.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_currentDump is null)
                return;

            _deleteInProgress = true;
            _dumps            = null;
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            // Yield thread to let UI to update
            await Task.Yield();

            await Service.DeleteAsync(_currentDump.Id, (await UserManager.GetUserAsync(authState.User)).Id);
            _dumps = await Service.GetAsync();

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj) => _currentDump = null;
    }
}