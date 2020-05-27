using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Gpu
    {
        List<CompanyViewModel> _companies;
        bool                   _creating;
        bool                   _editing;
        bool                   _loaded;
        GpuViewModel           _model;
        bool                   _prototype;
        bool                   _unknownCompany;
        bool                   _unknownDieSize;
        bool                   _unknownIntroduced;
        bool                   _unknownModelCode;
        bool                   _unknownPackage;
        bool                   _unknownProcess;
        bool                   _unknownProcessNm;
        bool                   _unknownTransistors;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/gpus/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _companies = await CompaniesService.GetAsync();
            _model     = _creating ? new GpuViewModel() : await Service.GetAsync(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/gpus/edit/", StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownCompany     = !_model.CompanyId.HasValue;
            _unknownDieSize     = !_model.DieSize.HasValue;
            _unknownIntroduced  = !_model.Introduced.HasValue;
            _unknownModelCode   = string.IsNullOrWhiteSpace(_model.ModelCode);
            _unknownPackage     = string.IsNullOrWhiteSpace(_model.Package);
            _unknownProcess     = string.IsNullOrWhiteSpace(_model.Process);
            _unknownProcessNm   = !_model.ProcessNm.HasValue;
            _unknownTransistors = !_model.Transistors.HasValue;
            _prototype          = _model.Introduced?.Year == 1000;
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
                NavigationManager.ToBaseRelativePath("admin/document_people");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownCompany)
                _model.CompanyId = null;
            else if(_model.CompanyId < 0)
                return;

            if(_unknownModelCode)
                _model.ModelCode = null;
            else if(string.IsNullOrWhiteSpace(_model.ModelCode))
                return;

            if(_unknownIntroduced)
                _model.Introduced = null;
            else if(_prototype)
                _model.Introduced = new DateTime(1000, 1, 1);
            else if(_model.Introduced >= DateTime.UtcNow.Date)
                return;

            if(_unknownPackage)
                _model.Package = null;
            else if(string.IsNullOrWhiteSpace(_model.Package))
                return;

            if(_unknownProcess)
                _model.Process = null;
            else if(string.IsNullOrWhiteSpace(_model.Process))
                return;

            if(_unknownProcessNm)
                _model.ProcessNm = null;
            else if(_model.ProcessNm < 1)
                return;

            if(_unknownDieSize)
                _model.DieSize = null;
            else if(_model.DieSize < 1)
                return;

            if(_unknownTransistors)
                _model.Transistors = null;
            else if(_model.Transistors < 0)
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
            Validators.ValidateString(e, L["Name must be 50 characters or less."], 50);

        void ValidateModelCode(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Model code must be 45 characters or less."], 45);

        void ValidateIntroduced(ValidatorEventArgs e) => Validators.ValidateIntroducedDate(e);

        void ValidatePackage(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Package must be 45 characters or less."], 45);

        void ValidateLongBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateLong(e);

        void ValidateFloatBiggerThanOne(ValidatorEventArgs e) => Validators.ValidateFloat(e, 1);

        void ValidateProcess(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Process must be 45 characters or less."], 45);
    }
}