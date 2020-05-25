using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Company
    {
        List<CompanyViewModel>  _companies;
        List<Iso31661Numeric>   _countries;
        bool                    _editable;
        bool                    _loaded;
        Database.Models.Company _model;
        [Parameter]
        public int Id { get; set; }

        int Status
        {
            get => (int)_model.Status;
            set => _model.Status = (CompanyStatus)value;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            if(Id <= 0)
                return;

            _countries = await CountriesService.GetAsync();
            _companies = await Service.GetAsync();
            _model     = await Service.GetAsync(Id);

            StateHasChanged();
        }
    }
}