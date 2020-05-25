using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class DocumentPerson
    {
        bool                           _editable;
        bool                           _loaded;
        Database.Models.DocumentPerson _model;
        List<PersonViewModel>          _people;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            if(Id <= 0)
                return;

            _people = await PeopleService.GetAsync();
            _model  = await Service.GetAsync(Id);

            StateHasChanged();
        }
    }
}