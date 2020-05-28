using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Machine
    {
        List<CompanyViewModel>       _companies;
        bool                         _creating;
        bool                         _editing;
        List<MachineFamilyViewModel> _families;
        bool                         _loaded;
        MachineViewModel             _model;
        bool                         _noFamily;
        bool                         _prototype;
        bool                         _unknownIntroduced;
        bool                         _unknownModel;
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

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/machines/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _companies = await CompaniesService.GetAsync();
            _families  = await MachineFamiliesService.GetAsync();
            _model     = _creating ? new MachineViewModel() : await Service.GetAsync(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/machines/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _noFamily          = !_model.FamilyId.HasValue;
            _prototype         = _model.Introduced?.Year == 1000;
            _unknownIntroduced = !_model.Introduced.HasValue;
            _unknownModel      = string.IsNullOrWhiteSpace(_model.Model);
        }

        void OnEditClicked()
        {
            _editing = true;
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnCancelClicked()
        {
            _editing = false;

            if(_creating)
            {
                NavigationManager.ToBaseRelativePath("admin/machines");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_noFamily)
                _model.FamilyId = null;
            else if(_model.FamilyId < 0)
                return;

            if(_unknownIntroduced)
                _model.Introduced = null;
            else if(_prototype)
                _model.Introduced = new DateTime(1000, 1, 1);
            else if(_model.Introduced?.Date >= DateTime.UtcNow.Date)
                return;

            if(_unknownModel)
                _model.Model = null;
            else if(string.IsNullOrWhiteSpace(_model.Model))
                return;

            if(_creating)
                Id = await Service.CreateAsync(_model);
            else
                await Service.UpdateAsync(_model);

            _editing  = false;
            _creating = false;
            _model    = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        void ValidateName(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Name must contain less than 255 characters."], 255);

        void ValidateModel(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Model must contain less than 50 characters."], 50);

        void ValidateIntroduced(ValidatorEventArgs e) => Validators.ValidateIntroducedDate(e);
    }
}