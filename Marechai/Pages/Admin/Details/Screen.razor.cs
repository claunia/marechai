using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Screen
    {
        bool                      _editable;
        bool                      _loaded;
        Database.Models.Screen    _model;
        List<ResolutionViewModel> _resolutions;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            if(Id <= 0)
                return;

            _resolutions = await ResolutionsService.GetAsync();
            _model       = await Service.GetAsync(Id);

            StateHasChanged();
        }
    }
}