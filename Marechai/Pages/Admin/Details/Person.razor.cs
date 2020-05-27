using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database.Models;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Person
    {
        List<Iso31661Numeric> _countries;
        bool                  _editing;
        bool                  _loaded;
        PersonViewModel       _model;
        bool                  _unknownAlias;
        bool                  _unknownCountry;
        bool                  _unknownDeathDate;
        bool                  _unknownDisplayName;
        bool                  _unknownFacebook;
        bool                  _unknownName;
        bool                  _unknownSurname;
        bool                  _unknownTwitter;
        bool                  _unknownWebpage;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            if(Id <= 0)
                return;

            _countries = await CountriesService.GetAsync();
            _model     = await Service.GetAsync(Id);

            _editing = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                         StartsWith("admin/people/edit/", StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownAlias       = string.IsNullOrWhiteSpace(_model.Alias);
            _unknownCountry     = !_model.CountryOfBirthId.HasValue;
            _unknownDeathDate   = !_model.DeathDate.HasValue;
            _unknownDisplayName = string.IsNullOrWhiteSpace(_model.DisplayName);
            _unknownFacebook    = string.IsNullOrWhiteSpace(_model.Facebook);
            _unknownName        = string.IsNullOrWhiteSpace(_model.Name);
            _unknownSurname     = string.IsNullOrWhiteSpace(_model.Surname);
            _unknownTwitter     = string.IsNullOrWhiteSpace(_model.Twitter);
            _unknownWebpage     = string.IsNullOrWhiteSpace(_model.Webpage);
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

            if(_unknownCountry)
                _model.CountryOfBirthId = null;
            else if(_model.CountryOfBirthId < 0)
                return;

            if(_model.BirthDate.Date >= DateTime.UtcNow.Date)
                return;

            if(_unknownDeathDate)
                _model.DeathDate = null;
            else if(_model.DeathDate?.Date >= DateTime.UtcNow.Date)
                return;
            else if(_model.DeathDate?.Date <= _model.BirthDate.Date)
                return;

            if(_unknownAlias)
                _model.Alias = null;
            else if(string.IsNullOrWhiteSpace(_model.Alias))
                return;

            if(_unknownDisplayName)
                _model.DisplayName = null;
            else if(string.IsNullOrWhiteSpace(_model.DisplayName))
                return;

            if(_unknownFacebook)
                _model.Facebook = null;
            else if(string.IsNullOrWhiteSpace(_model.Facebook))
                return;

            if(_unknownName)
                _model.Name = null;
            else if(string.IsNullOrWhiteSpace(_model.Name))
                return;

            if(_unknownSurname)
                _model.Surname = null;
            else if(string.IsNullOrWhiteSpace(_model.Surname))
                return;

            if(_unknownTwitter)
                _model.Twitter = null;
            else if(string.IsNullOrWhiteSpace(_model.Twitter))
                return;

            if(_unknownWebpage)
                _model.Webpage = null;
            else if(string.IsNullOrWhiteSpace(_model.Webpage))
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

        void ValidateBirthDate(ValidatorEventArgs e)
        {
            if(!(e.Value is DateTime date))
            {
                e.Status = ValidationStatus.Error;

                return;
            }

            if(date.Date >= DateTime.UtcNow.Date)
            {
                e.ErrorText = L["Birth date must be before today."];
                e.Status    = ValidationStatus.Error;

                return;
            }

            if(_unknownDeathDate || !_model.DeathDate.HasValue)
                return;

            if(date.Date < _model.DeathDate?.Date)
                return;

            e.ErrorText = L["Birth date must be before death date."];
            e.Status    = ValidationStatus.Error;
        }

        void ValidateDeathDate(ValidatorEventArgs e)
        {
            if(!(e.Value is DateTime date))
            {
                e.Status = ValidationStatus.Error;

                return;
            }

            if(date.Date >= DateTime.UtcNow.Date)
            {
                e.ErrorText = L["Death date must be before today."];
                e.Status    = ValidationStatus.Error;

                return;
            }

            if(date.Date > _model.BirthDate.Date)
                return;

            e.ErrorText = L["Death date must be after birth date."];
            e.Status    = ValidationStatus.Error;
        }

        void ValidateWebpage(ValidatorEventArgs e) =>
            Validators.ValidateUrl(e, L["Webpage must be smaller than 255 characters."], 255);

        void ValidateTwitter(ValidatorEventArgs e)
        {
            if(!(e.Value is string twitter))
            {
                e.Status = ValidationStatus.Error;

                return;
            }

            if(twitter.Length < 1 ||
               twitter.Length > 255)
            {
                e.ErrorText = L["Twitter handle must be smaller than 255 characters."];
                e.Status    = ValidationStatus.Error;

                return;
            }

            if(twitter[0] == '@')
                return;

            e.ErrorText = L["Invalid Twitter handle."];
            e.Status    = ValidationStatus.Error;
        }

        void ValidateFacebook(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Facebook username must be smaller than 256 characters."], 256);
    }
}