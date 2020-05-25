using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class InstructionSetExtension
    {
        bool                                    _editable;
        bool                                    _loaded;
        Database.Models.InstructionSetExtension _model;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            if(Id <= 0)
                return;

            _model = await Service.GetAsync(Id);

            StateHasChanged();
        }
    }
}