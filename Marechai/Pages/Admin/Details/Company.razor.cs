using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Company
    {
        List<CompanyViewModel> _companies;
        List<Iso31661Numeric>  _countries;
        bool                   _creating;
        CompanyLogo            _currentLogo;
        int?                   _currentLogoYear;
        bool                   _deleteInProgress;
        bool                   _editing;
        Modal                  _frmDelete;
        Modal                  _frmLogoYear;
        bool                   _loaded;
        List<CompanyLogo>      _logos;
        CompanyViewModel       _model;
        bool                   _unknownAddress;
        bool                   _unknownCity;
        bool                   _unknownCountry;
        bool                   _unknownFacebook;
        bool                   _unknownFounded;
        bool                   _unknownLogoYear;
        bool                   _unknownPostalCode;
        bool                   _unknownProvince;
        bool                   _unknownSold;
        bool                   _unknownSoldTo;
        bool                   _unknownTwitter;
        bool                   _unknownWebsite;

        bool _yearChangeInProgress;
        [Parameter]
        public int Id { get; set; }

        int Status
        {
            get => (int)_model.Status;
            set => _model.Status = (CompanyStatus)value;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/companies/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _countries = await CountriesService.GetAsync();
            _companies = await Service.GetAsync();
            _model     = _creating ? new CompanyViewModel() : await Service.GetAsync(Id);
            _logos     = await CompanyLogosService.GetByCompany(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/companies/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownWebsite    = string.IsNullOrWhiteSpace(_model.Website);
            _unknownCountry    = !_model.CountryId.HasValue;
            _unknownFounded    = !_model.Founded.HasValue;
            _unknownAddress    = string.IsNullOrWhiteSpace(_model.Address);
            _unknownFacebook   = string.IsNullOrWhiteSpace(_model.Facebook);
            _unknownCity       = string.IsNullOrWhiteSpace(_model.City);
            _unknownProvince   = string.IsNullOrWhiteSpace(_model.Province);
            _unknownTwitter    = string.IsNullOrWhiteSpace(_model.Twitter);
            _unknownPostalCode = string.IsNullOrWhiteSpace(_model.PostalCode);
            _unknownSold       = !_model.Sold.HasValue;
            _unknownSoldTo     = !_model.SoldToId.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/companies");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownWebsite)
                _model.Website = null;
            else if(string.IsNullOrWhiteSpace(_model.Website))
                return;

            if(_unknownCountry)
                _model.CountryId = null;
            else if(_model.CountryId < 0)
                return;

            if(_unknownFounded)
                _model.Founded = null;
            else if(_model.Founded?.Date >= DateTime.UtcNow.Date)
                return;

            if(_unknownAddress)
                _model.Address = null;
            else if(string.IsNullOrWhiteSpace(_model.Address))
                return;

            if(_unknownFacebook)
                _model.Facebook = null;
            else if(string.IsNullOrWhiteSpace(_model.Facebook))
                return;

            if(_unknownFacebook)
                _model.Facebook = null;
            else if(string.IsNullOrWhiteSpace(_model.Facebook))
                return;

            if(_unknownCity)
                _model.City = null;
            else if(string.IsNullOrWhiteSpace(_model.City))
                return;

            if(string.IsNullOrWhiteSpace(_model.Name))
                return;

            if(_unknownProvince)
                _model.Province = null;
            else if(string.IsNullOrWhiteSpace(_model.Province))
                return;

            if(_unknownTwitter)
                _model.Twitter = null;
            else if(string.IsNullOrWhiteSpace(_model.Twitter))
                return;

            if(_unknownPostalCode)
                _model.PostalCode = null;
            else if(string.IsNullOrWhiteSpace(_model.PostalCode))
                return;

            if(_unknownSoldTo)
                _model.SoldToId = null;
            else if(_model.SoldToId < 0)
                return;

            if(_unknownSold)
                _model.Sold = null;
            else if(_model.Sold?.Date >= DateTime.UtcNow.Date)
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
            Validators.ValidateString(e, L["Name must be smaller than 256 characters."], 256);

        void ValidateFounded(ValidatorEventArgs e) => Validators.ValidateDate(e);

        void ValidateAddress(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Address must be smaller than 80 characters."], 80);

        void ValidateCity(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["City name must be smaller than 80 characters."], 80);

        void ValidateProvince(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Province name must be smaller than 80 characters."], 80);

        void ValidatePostalCode(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Postal code name must be smaller than 25 characters."], 25);

        void ValidateSold(ValidatorEventArgs e) => Validators.ValidateDate(e);

        void ValidateWebsite(ValidatorEventArgs e) =>
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

        void ShowDeleteModal(int itemId)
        {
            _currentLogo = _logos.FirstOrDefault(n => n.Id == itemId);
            _frmDelete.Show();
        }

        void HideDeleteModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_currentLogo is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await CompanyLogosService.DeleteAsync(_currentLogo.Id);
            _logos = await CompanyLogosService.GetByCompany(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void DeleteModalClosing(ModalClosingEventArgs e) => _currentLogo = null;

        void ShowLogoYearModal(int itemId)
        {
            _currentLogo     = _logos.FirstOrDefault(n => n.Id == itemId);
            _currentLogoYear = _currentLogo?.Year;
            _unknownLogoYear = _currentLogoYear is null;
            _frmLogoYear.Show();
        }

        void HideLogoYearModal() => _frmLogoYear.Hide();

        async void ConfirmLogoYear()
        {
            if(_currentLogo is null)
                return;

            _yearChangeInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await CompanyLogosService.ChangeYearAsync(_currentLogo.Id, _unknownLogoYear ? null : _currentLogoYear);
            _logos = await CompanyLogosService.GetByCompany(Id);

            _yearChangeInProgress = false;
            _frmLogoYear.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void LogoYearModalClosing(ModalClosingEventArgs e)
        {
            _currentLogo     = null;
            _currentLogoYear = null;
        }

        void ValidateLogoYear(ValidatorEventArgs e)
        {
            if(!(e.Value is int item) ||
               item <= 1000           ||
               item > DateTime.UtcNow.Year)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }
    }
}