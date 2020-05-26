using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class MachineFamily
    {
        List<CompanyViewModel>        _companies;
        bool                          _editable;
        bool                          _loaded;
        Database.Models.MachineFamily _model;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            if(Id <= 0)
                return;

            _companies = await CompaniesService.GetAsync();
            _model     = await Service.GetAsync(Id);

            StateHasChanged();
        }
    }
}