using System;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Shared;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class License
    {
        bool                    _creating;
        bool                    _editing;
        bool                    _loaded;
        Database.Models.License _model;
        bool                    _unknownLink;
        bool                    _unknownText;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/licenses/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _model = _creating ? new Database.Models.License() : await Service.GetAsync(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/licenses/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownText = string.IsNullOrWhiteSpace(_model.Text);
            _unknownLink = string.IsNullOrWhiteSpace(_model.Link);
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
                NavigationManager.ToBaseRelativePath("admin/licenses");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownText)
                _model.Text = null;
            else if(string.IsNullOrWhiteSpace(_model.Text))
                return;

            if(string.IsNullOrWhiteSpace(_model.Text) ||
               _model.Name?.Length > 255)
                return;

            if(string.IsNullOrWhiteSpace(_model.SPDX) ||
               _model.SPDX?.Length > 255)
                return;

            if(_unknownLink)
                _model.Link = null;
            else if(string.IsNullOrWhiteSpace(_model.Link) ||
                    _model.Link?.Length > 512)
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
            Validators.ValidateString(e, L["License name cannot contain more than 255 characters."], 255);

        void ValidateSpdx(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["SPDX identifier cannot contain more than 255 characters."], 255);

        void ValidateLink(ValidatorEventArgs e) =>
            Validators.ValidateUrl(e, L["License text link must be smaller than 512 characters."], 512);

        void ValidateText(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["License text cannot contain more than 131072 characters."], 131072);
    }
}