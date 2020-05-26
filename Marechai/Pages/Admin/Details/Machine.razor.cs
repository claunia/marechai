using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.Database;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Machine
    {
        List<CompanyViewModel>       _companies;
        bool                         _editable;
        List<MachineFamilyViewModel> _families;
        bool                         _loaded;
        Database.Models.Machine      _model;
        [Parameter]
        public int Id { get; set; }

        int Type
        {
            get => (int)_model.Type;
            set => _model.Type = (MachineType)value;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            if(Id <= 0)
                return;

            _companies = await CompaniesService.GetAsync();
            _families  = await MachineFamiliesService.GetAsync();
            _model     = await Service.GetAsync(Id);

            StateHasChanged();
        }
    }
}