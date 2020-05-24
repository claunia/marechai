using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.Database;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Companies
{
    public partial class View
    {
        string        _carrouselActive;
        Company       _company;
        List<Machine> _computers;
        List<Machine> _consoles;
        string        _description;

        bool    _loaded;
        Company _soldTo;
        [Parameter]
        public int Id { get; set; }

        public bool ComputersCollapsed { get; set; } = true;
        public bool ConsolesCollapsed  { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            if(Id <= 0)
            {
                _loaded = true;

                return;
            }

            _company = await Service.GetCompanyAsync(Id);
            List<Machine> machines = await Service.GetMachinesAsync(Id);

            _computers = machines.Where(m => m.Type == MachineType.Computer).ToList();
            _consoles  = machines.Where(m => m.Type == MachineType.Console).ToList();

            _description = await Service.GetDescriptionAsync(Id);
            _soldTo      = await Service.GetSoldToAsync(_company.SoldToId);

            _loaded = true;
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