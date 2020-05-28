using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Screen
    {
        bool                      _creating;
        bool                      _editing;
        bool                      _loaded;
        ScreenViewModel           _model;
        List<ResolutionViewModel> _resolutions;
        bool                      _unknownColors;
        bool                      _unknownHeight;
        bool                      _unknownType;
        bool                      _unknownWidth;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/screens/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _resolutions = await ResolutionsService.GetAsync();
            _model       = _creating ? new ScreenViewModel() : await Service.GetAsync(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/screens/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownWidth  = !_model.Width.HasValue;
            _unknownType   = string.IsNullOrWhiteSpace(_model.Type);
            _unknownHeight = !_model.Height.HasValue;
            _unknownColors = !_model.Colors.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/screens");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownWidth)
                _model.Width = null;
            else if(_model.Width < 0)
                return;

            if(_unknownHeight)
                _model.Height = null;
            else if(_model.Height < 0)
                return;

            if(_unknownType)
                _model.Type = null;
            else if(string.IsNullOrWhiteSpace(_model.Type))
                return;

            if(_unknownColors)
                _model.EffectiveColors = null;
            else if(_model.EffectiveColors < 0)
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

        void ValidateDoubleBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateDouble(e, 1, 131072);

        void ValidateLongBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateLong(e, 2);

        void ValidateType(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Screen type cannot be bigger than 256 characters."], 256);
    }
}