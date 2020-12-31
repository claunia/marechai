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
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Machines
{
    public partial class View
    {
        bool[]           _gpuVisible;
        int              _id;
        bool             _loaded;
        MachineViewModel _machine;
        List<Guid>       _photos;
        bool[]           _processorVisible;
        bool[]           _soundVisible;

        [Parameter]
        public int Id
        {
            get => _id;
            set
            {
                if(_id == value)
                    return;

                _id     = value;
                _loaded = false;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _machine = await Service.GetMachine(Id);

            _processorVisible = new bool[_machine.Processors.Count];
            _gpuVisible       = new bool[_machine.Gpus.Count];
            _soundVisible     = new bool[_machine.SoundSynthesizers.Count];
            _photos           = await MachinePhotosService.GetGuidsByMachineAsync(Id);

            _loaded = true;
            StateHasChanged();
        }
    }
}