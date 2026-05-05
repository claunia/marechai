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
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Machines;

public partial class View
{
    int              _lastId;
    string           _description;
    bool             _isCollected;
    PhotoLightbox    _lightbox;
    bool             _loaded;
    MachineDto _machine;
    List<Guid>       _photos;
    List<SoftwareDto> _software;
    bool             _togglingCollection;

    [CascadingParameter]
    Task<AuthenticationState> AuthState { get; set; }

    [Parameter]
    public int Id { get; set; }

    protected override void OnParametersSet()
    {
        if(Id == _lastId) return;

        _lastId = Id;
        _loaded = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        try
        {
        _machine = await Service.GetMachine(Id);

        _photos           = await MachinePhotosService.GetGuidsByMachineAsync(Id);
        _software         = await Service.GetSoftwareByMachineAsync(Id);
        _description      = await Service.GetDescriptionTextAsync(Id);

        AuthenticationState authState = await AuthState;

        if(authState.User.Identity?.IsAuthenticated == true)
            _isCollected = await CollectionSvc.IsMachineCollectedAsync(Id);

        _loaded = true;
        StateHasChanged();
        }
        catch(ObjectDisposedException)
        {
            // Component was disposed during async loading — ignore
        }
    }

    async Task ToggleCollectionAsync()
    {
        _togglingCollection = true;

        if(_isCollected)
        {
            (bool success, _) = await CollectionSvc.RemoveMachineFromCollectionAsync(Id);

            if(success) _isCollected = false;
        }
        else
        {
            (bool success, _) = await CollectionSvc.AddMachineToCollectionAsync(Id);

            if(success) _isCollected = true;
        }

        _togglingCollection = false;
    }
}