/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.Helpers;
using Marechai.Shared;
using Marechai.ViewModels;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SkiaSharp;
using Svg.Skia;
using Tewr.Blazor.FileReader;

namespace Marechai.Pages.Admin.Details
{
    public partial class Company
    {
        const int                   _maxUploadSize = 5 * 1048576;
        bool                        _addingDescription;
        AuthenticationState         _authState;
        List<CompanyViewModel>      _companies;
        List<Iso31661Numeric>       _countries;
        bool                        _creating;
        CompanyLogo                 _currentLogo;
        int?                        _currentLogoYear;
        bool                        _deleteInProgress;
        CompanyDescriptionViewModel _description;
        bool                        _editing;
        Modal                       _frmDelete;
        Modal                       _frmLogoYear;
        Modal                       _frmUpload;
        ElementReference            _inputUpload;
        bool                        _loaded;
        List<CompanyLogo>           _logos;
        CompanyViewModel            _model;
        MarkdownPipeline            _pipeline;
        double                      _progressValue;
        bool                        _readonlyDescription;
        bool                        _savingLogo;
        string                      _selectedDescriptionTab;
        bool                        _unknownAddress;
        bool                        _unknownCity;
        bool                        _unknownCountry;
        bool                        _unknownFacebook;
        bool                        _unknownFounded;
        bool                        _unknownLegalName;
        bool                        _unknownLogoYear;
        bool                        _unknownPostalCode;
        bool                        _unknownProvince;
        bool                        _unknownSold;
        bool                        _unknownSoldTo;
        bool                        _unknownTwitter;
        bool                        _unknownWebsite;
        bool                        _uploaded;
        string                      _uploadedPngData;
        string                      _uploadedSvgData;
        string                      _uploadedWebpData;
        bool                        _uploadError;
        string                      _uploadErrorMessage;
        bool                        _uploading;
        MemoryStream                _uploadMs;
        bool                        _yearChangeInProgress;

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

            _countries              = await CountriesService.GetAsync();
            _companies              = await Service.GetAsync();
            _model                  = _creating ? new CompanyViewModel() : await Service.GetAsync(Id);
            _logos                  = await CompanyLogosService.GetByCompany(Id);
            _description            = await Service.GetDescriptionAsync(Id);
            _selectedDescriptionTab = "markdown";
            _authState              = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/companies/edit/",
                                                                 StringComparison.InvariantCulture);

            _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            if(_description?.Markdown != null)
                _description.Html = Markdown.ToHtml(_description.Markdown);

            if(_editing)
                SetCheckboxes();

            if(firstRender)
            {
                DotNetObjectReference<Company> dotNetReference = DotNetObjectReference.Create(this);
                await JSRuntime.InvokeVoidAsync("SetDotNetClassReference", dotNetReference);
            }

            _readonlyDescription = true;

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
            _unknownLegalName  = string.IsNullOrWhiteSpace(_model.LegalName);
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

            if(_unknownLegalName)
                _model.LegalName = null;
            else if(string.IsNullOrWhiteSpace(_model.LegalName))
                return;

            if(_creating)
                Id = await Service.CreateAsync(_model, (await UserManager.GetUserAsync(_authState.User)).Id);
            else
                await Service.UpdateAsync(_model, (await UserManager.GetUserAsync(_authState.User)).Id);

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

        void ValidateLegalName(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Legal name must be smaller than 256 characters."], 256);

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

            await CompanyLogosService.DeleteAsync(_currentLogo.Id,
                                                  (await UserManager.GetUserAsync(_authState.User)).Id);

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

            await CompanyLogosService.ChangeYearAsync(_currentLogo.Id, _unknownLogoYear ? null : _currentLogoYear,
                                                      (await UserManager.GetUserAsync(_authState.User)).Id);

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

        void ShowUploadModal()
        {
            _uploadError        = false;
            _uploadErrorMessage = "";
            _uploading          = false;
            _uploadMs           = null;
            _uploaded           = false;
            _progressValue      = 0;
            _uploadedSvgData    = "";
            _uploadedPngData    = "";
            _uploadedWebpData   = "";
            _savingLogo         = false;
            _frmUpload.Show();
        }

        void HideUploadModal() => _frmUpload.Hide();

        void UploadModalClosing(ModalClosingEventArgs e)
        {
            _uploadError        = false;
            _uploadErrorMessage = "";
            _uploading          = false;
            _uploadMs           = null;
            _uploaded           = false;
            _progressValue      = 0;
            _uploadedSvgData    = "";
            _uploadedPngData    = "";
            _uploadedWebpData   = "";
            _savingLogo         = false;
        }

        async Task UploadFile()
        {
            IFileReference file = (await FileReaderService.CreateReference(_inputUpload).EnumerateFilesAsync()).
                FirstOrDefault();

            if(file is null)
                return;

            IFileInfo fileInfo = await file.ReadFileInfoAsync();

            if(fileInfo.Size > _maxUploadSize)
            {
                _uploadError        = true;
                _uploadErrorMessage = L["The selected file is too big."];

                return;
            }

            _uploading = true;

            await using AsyncDisposableStream fs     = await file.OpenReadAsync();
            byte[]                            buffer = new byte[20480];

            try
            {
                double lastProgress = 0;
                int    count;
                _uploadMs = new MemoryStream();

                while((count = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await _uploadMs.WriteAsync(buffer, 0, count);

                    double progress = ((double)fs.Position * 100) / fs.Length;

                    if(!(progress > lastProgress + 0.01))
                        continue;

                    _progressValue = progress;
                    await InvokeAsync(StateHasChanged);
                    await Task.Yield();
                    lastProgress = progress;
                }
            }
            catch(Exception)
            {
                _uploading          = false;
                _uploadError        = true;
                _uploadErrorMessage = L["There was an error uploading the file."];

                return;
            }

            _uploading = false;
            await InvokeAsync(StateHasChanged);
            await Task.Yield();

            bool validSvg = true;

            buffer = new byte[6];

            _uploadMs.Position = 0;
            _uploadMs.Read(buffer, 0, 6);

            if(!Encoding.UTF8.GetString(buffer).StartsWith("<?xml", StringComparison.InvariantCulture) &&
               !Encoding.UTF8.GetString(buffer).StartsWith("<svg", StringComparison.InvariantCulture))
                validSvg = false;

            _uploadMs.Seek(-6, SeekOrigin.End);
            _uploadMs.Read(buffer, 0, 6);

            // LF
            if(buffer[^1] == 0x0A)
            {
                _uploadMs.Seek(-7, SeekOrigin.End);
                _uploadMs.Read(buffer, 0, 6);
            }

            // CR
            if(buffer[^1] == 0x0D)
            {
                _uploadMs.Seek(-8, SeekOrigin.End);
                _uploadMs.Read(buffer, 0, 6);
            }

            if(!Encoding.UTF8.GetString(buffer).StartsWith("</svg>", StringComparison.InvariantCulture))
                validSvg = false;

            if(!validSvg)
            {
                _uploadMs           = null;
                _uploadError        = true;
                _uploadErrorMessage = L["The uploaded file is not a SVG file."];

                return;
            }

            _uploadMs.Position = 0;

            var svg = new SKSvg();

            try
            {
                svg.Load(_uploadMs);
            }
            catch(Exception)
            {
                _uploadMs           = null;
                _uploadError        = true;
                _uploadErrorMessage = L["The uploaded file could not be loaded. Is it a correct SVG file?"];

                return;
            }

            var pngStream  = new MemoryStream();
            var webpStream = new MemoryStream();

            try
            {
                SvgRender.RenderSvg(svg, pngStream, SKEncodedImageFormat.Png, 256, 1);
                SvgRender.RenderSvg(svg, webpStream, SKEncodedImageFormat.Webp, 256, 1);
            }
            catch(Exception)
            {
                _uploadMs           = null;
                _uploadError        = true;
                _uploadErrorMessage = L["An error occuring rendering the uploaded file. Is it a correct SVG file?"];

                return;
            }

            _uploadMs.Position  = 0;
            pngStream.Position  = 0;
            webpStream.Position = 0;

            _uploadedSvgData  = $"data:image/svg+xml;base64,{Convert.ToBase64String(_uploadMs.ToArray())}";
            _uploadedPngData  = $"data:image/png;base64,{Convert.ToBase64String(pngStream.ToArray())}";
            _uploadedWebpData = $"data:image/webp;base64,{Convert.ToBase64String(webpStream.ToArray())}";

            _unknownLogoYear = true;
            _uploaded        = true;
        }

        async Task ConfirmUpload()
        {
            _savingLogo        = true;
            _uploadMs.Position = 0;
            var guid = Guid.NewGuid();

            try
            {
                SvgRender.RenderCompanyLogo(guid, _uploadMs, Host.WebRootPath);

                var fs = new FileStream(Path.Combine(Host.WebRootPath, "assets/logos", $"{guid}.svg"),
                                        FileMode.OpenOrCreate, FileAccess.ReadWrite);

                _uploadMs.Position = 0;
                _uploadMs.WriteTo(fs);
                fs.Close();
            }
            catch(Exception)
            {
                _savingLogo         = false;
                _uploadError        = true;
                _uploadErrorMessage = L["An error occuring rendering the uploaded file. Is it a correct SVG file?"];

                return;
            }

            await CompanyLogosService.CreateAsync(Id, guid, _unknownLogoYear ? null : _currentLogoYear,
                                                  (await UserManager.GetUserAsync(_authState.User)).Id);

            _logos = await CompanyLogosService.GetByCompany(Id);

            _frmUpload.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void OnSelectedDescriptionTabChanged(string name)
        {
            if(name == "preview" &&
               !_readonlyDescription)
                _description.Html = Markdown.ToHtml(_description.Markdown, _pipeline);

            _selectedDescriptionTab = name;
        }

        [JSInvokableAttribute("OnCompanyDescriptionChangedDotnet")]
        public void OnDescriptionChanged(string value)
        {
            if(!_readonlyDescription)
                _description.Markdown = value;
        }

        void AddNewDescription()
        {
            _description           = new CompanyDescriptionViewModel();
            _description.CompanyId = Id;
            _readonlyDescription   = false;
            _addingDescription     = true;
        }

        void EditDescription() => _readonlyDescription = false;

        async Task CancelDescription()
        {
            _description         = _addingDescription ? null : await Service.GetDescriptionAsync(Id);
            _readonlyDescription = true;
            await JSRuntime.InvokeVoidAsync("SetCompanyDescriptionText", _description?.Markdown ?? "");
            _addingDescription = false;

            StateHasChanged();
        }

        async Task SaveDescription()
        {
            if(_readonlyDescription)
                return;

            if(string.IsNullOrWhiteSpace(_description.Markdown))
            {
                await CancelDescription();

                return;
            }

            _description.Html = Markdown.ToHtml(_description.Markdown, _pipeline);

            await Service.CreateOrUpdateDescriptionAsync(Id, _description,
                                                         (await UserManager.GetUserAsync(_authState.User)).Id);

            _addingDescription = false;
            await CancelDescription();
        }
    }
}