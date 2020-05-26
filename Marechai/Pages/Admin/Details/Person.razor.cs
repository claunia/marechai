using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Person
    {
        List<Iso31661Numeric>  _countries;
        bool                   _editable;
        bool                   _loaded;
        Database.Models.Person _model;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            if(Id <= 0)
                return;

            _countries = await CountriesService.GetAsync();
            _model     = await Service.GetAsync(Id);

            StateHasChanged();
        }
    }
}