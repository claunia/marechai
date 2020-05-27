using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class DocumentPerson
    {
        bool                    _editing;
        bool                    _loaded;
        DocumentPersonViewModel _model;
        bool                    _noLinkedPerson;
        List<PersonViewModel>   _people;
        bool                    _unknownAlias;
        bool                    _unknownDisplayName;
        bool                    _unknownName;
        bool                    _unknownSurname;

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

            _editing = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                         StartsWith("admin/document_people/edit/", StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownAlias       = string.IsNullOrWhiteSpace(_model.Alias);
            _noLinkedPerson     = !_model.PersonId.HasValue;
            _unknownDisplayName = string.IsNullOrWhiteSpace(_model.DisplayName);
            _unknownName        = string.IsNullOrWhiteSpace(_model.Name);
            _unknownSurname     = string.IsNullOrWhiteSpace(_model.Surname);
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
            _model   = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownAlias)
                _model.Alias = null;
            else if(string.IsNullOrWhiteSpace(_model.Alias))
                return;

            if(_noLinkedPerson)
                _model.PersonId = null;
            else if(_model.PersonId < 0)
                return;

            if(_unknownAlias)
                _model.Alias = null;
            else if(string.IsNullOrWhiteSpace(_model.Alias))
                return;

            if(_unknownDisplayName)
                _model.Alias = null;
            else if(string.IsNullOrWhiteSpace(_model.Alias))
                return;

            if(_unknownName)
                _model.Name = null;
            else if(string.IsNullOrWhiteSpace(_model.Name))
                return;

            if(_unknownSurname)
                _model.Surname = null;
            else if(string.IsNullOrWhiteSpace(_model.Surname))
                return;

            if((_unknownName  && !_unknownSurname) ||
               (!_unknownName && _unknownSurname))
                return;

            // TODO: Show error here
            if(_unknownName    &&
               _unknownSurname &&
               _unknownAlias   &&
               _unknownDisplayName)
                return;

            _editing = false;
            await Service.UpdateAsync(_model);
            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        void ValidateName(ValidatorEventArgs e)
        {
            if(!(e.Value is string name))
            {
                e.Status = ValidationStatus.Error;

                return;
            }

            if(name.Length < 1 ||
               name.Length > 256)
            {
                e.ErrorText = L["Name must be smaller than 256 characters."];
                e.Status    = ValidationStatus.Error;

                return;
            }

            if(!string.IsNullOrWhiteSpace(_model.Surname) &&
               !_unknownSurname)
                return;

            e.ErrorText = L["Both name and surname must be known and filled, or both unknown."];
            e.Status    = ValidationStatus.Error;
        }

        void ValidateSurname(ValidatorEventArgs e)
        {
            if(!(e.Value is string surname))
            {
                e.Status = ValidationStatus.Error;

                return;
            }

            if(surname.Length < 1 ||
               surname.Length > 256)
            {
                e.ErrorText = L["Surname must be smaller than 256 characters."];
                e.Status    = ValidationStatus.Error;

                return;
            }

            if(!string.IsNullOrWhiteSpace(_model.Surname) &&
               !_unknownSurname)
                return;

            e.ErrorText = L["Both name and surname must be known and filled, or both unknown."];
            e.Status    = ValidationStatus.Error;
        }

        void ValidateAlias(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Alias must be smaller than 256 characters."], 256);

        void ValidateDisplayName(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Display name must be smaller than 256 characters."], 256);
    }
}