using System;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Shared;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class InstructionSet
    {
        bool                           _creating;
        bool                           _editing;
        bool                           _loaded;
        Database.Models.InstructionSet _model;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/instruction_sets/create",
                                                     StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _model = _creating ? new Database.Models.InstructionSet() : await Service.GetAsync(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
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

            if(_creating)
            {
                NavigationManager.ToBaseRelativePath("admin/instruction_sets");

                return;
            }

            _model = await Service.GetAsync(Id);
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(string.IsNullOrWhiteSpace(_model.Name) ||
               _model.Name.Length > 45                ||
               !Service.VerifyUnique(_model.Name))
                return;

            if(_creating)
                Id = await Service.CreateAsync(_model);
            else
                await Service.UpdateAsync(_model);

            _editing  = false;
            _creating = false;
            _model    = await Service.GetAsync(Id);
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