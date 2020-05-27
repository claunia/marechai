using System;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Shared;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class InstructionSet
    {
        bool                                    _editing;
        bool                                    _loaded;
        Database.Models.InstructionSet _model;
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

            _editing = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                         StartsWith("admin/instruction_sets/edit/",
                                                    StringComparison.InvariantCulture);

            StateHasChanged();
        }

        void OnEditClicked()
        {
            _editing = true;
            StateHasChanged();
        }

        async void OnCancelClicked()
        {
            _editing = false;
            _model   = await Service.GetAsync(Id);
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(string.IsNullOrWhiteSpace(_model.Name) ||
               _model.Name.Length > 45                ||
               !Service.VerifyUnique(_model.Name))
                return;

            _editing = false;
            await Service.UpdateAsync(_model);
            _model = await Service.GetAsync(Id);
            StateHasChanged();
        }

        void ValidateName(ValidatorEventArgs e)
        {
            Validators.ValidateString(e, L["Instruction set name cannot contain more than 45 characters."], 45);

            if(e.Status != ValidationStatus.Success)
                return;

            if(Service.VerifyUnique(_model.Name))
                return;

            e.Status    = ValidationStatus.Error;
            e.ErrorText = L["Instruction set name must be unique."];
        }
    }
}