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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Companies
{
    public partial class View
    {
        string            _carrouselActive;
        CompanyViewModel  _company;
        List<Machine>     _computers;
        List<Machine>     _consoles;
        string            _description;
        bool              _loaded;
        List<CompanyLogo> _logos;
        Company           _soldTo;
        [Parameter]
        public int Id { get; set; }

        public bool ComputersCollapsed { get; set; } = true;
        public bool ConsolesCollapsed  { get; set; } = true;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            if(Id <= 0)
            {
                _loaded = true;

                return;
            }

            _company = await Service.GetAsync(Id);
            List<Machine> machines = await Service.GetMachinesAsync(Id);

            _computers = machines.Where(m => m.Type == MachineType.Computer).ToList();
            _consoles  = machines.Where(m => m.Type == MachineType.Console).ToList();

            _description = await Service.GetDescriptionTextAsync(Id);
            _soldTo      = await Service.GetSoldToAsync(_company.SoldToId);
            _logos       = await CompanyLogosService.GetByCompany(Id);

            _loaded = true;
            StateHasChanged();
        }

        void CollapseComputers()
        {
            if(_computers.Count == 0)
                return;

            ComputersCollapsed = !ComputersCollapsed;
        }

        void CollapseConsoles()
        {
            if(_consoles.Count == 0)
                return;

            ConsolesCollapsed = !ConsolesCollapsed;
        }
    }
}