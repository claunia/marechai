using System;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Resolution
    {
        bool                _creating;
        bool                _editing;
        bool                _loaded;
        ResolutionViewModel _model;
        bool                _unknownColors;
        bool                _unknownPalette;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/resolutions/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _model = _creating ? new ResolutionViewModel() : await Service.GetAsync(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/resolutions/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownColors  = !_model.Colors.HasValue;
            _unknownPalette = !_model.Colors.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/resolutions");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownColors)
                _model.Colors = null;
            else if(_model.Colors <= 0)
                return;

            if(_unknownPalette)
                _model.Palette = null;
            else if(_model.Palette <= 0)
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

        void ValidateIntegerBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateInteger(e, 1);

        void ValidateLongBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateLong(e, 2);
    }
}