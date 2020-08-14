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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database;
using Marechai.Database.Models;
using Marechai.Helpers;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Tewr.Blazor.FileReader;

namespace Marechai.Pages.Admin.Details
{
    public partial class Document
    {
        const int                              _maxUploadSize = 25 * 1048576;
        bool                                   _addingCompany;
        int?                                   _addingCompanyId;
        string                                 _addingCompanyRoleId;
        bool                                   _addingMachine;
        bool                                   _addingMachineFamily;
        int?                                   _addingMachineFamilyId;
        int?                                   _addingMachineId;
        bool                                   _addingPerson;
        int?                                   _addingPersonId;
        string                                 _addingPersonRoleId;
        bool                                   _addingScan;
        bool?                                  _addToDatabase;
        bool                                   _allFinished;
        AuthenticationState                    _authState;
        List<DocumentCompanyViewModel>         _companies;
        bool?                                  _convertAvif1440;
        bool?                                  _convertAvif1440Th;
        bool?                                  _convertAvif4k;
        bool?                                  _convertAvif4kTh;
        bool?                                  _convertAvifHd;
        bool?                                  _convertAvifHdTh;
        bool?                                  _convertHeif1440;
        bool?                                  _convertHeif1440Th;
        bool?                                  _convertHeif4k;
        bool?                                  _convertHeif4kTh;
        bool?                                  _convertHeifHd;
        bool?                                  _convertHeifHdTh;
        bool?                                  _convertJp2k1440;
        bool?                                  _convertJp2k1440Th;
        bool?                                  _convertJp2k4k;
        bool?                                  _convertJp2k4kTh;
        bool?                                  _convertJp2kHd;
        bool?                                  _convertJp2kHdTh;
        bool?                                  _convertJpeg1440;
        bool?                                  _convertJpeg1440Th;
        bool?                                  _convertJpeg4k;
        bool?                                  _convertJpeg4kTh;
        bool?                                  _convertJpegHd;
        bool?                                  _convertJpegHdTh;
        bool?                                  _convertWebp1440;
        bool?                                  _convertWebp1440Th;
        bool?                                  _convertWebp4k;
        bool?                                  _convertWebp4kTh;
        bool?                                  _convertWebpHd;
        bool?                                  _convertWebpHdTh;
        List<Iso31661Numeric>                  _countries;
        bool                                   _creating;
        CompanyByDocumentViewModel             _currentCompanyByDocument;
        DocumentByMachineViewModel             _currentDocumentByMachine;
        DocumentByMachineFamilyViewModel       _currentDocumentByMachineFamily;
        PersonByDocumentViewModel              _currentPersonByDocument;
        bool                                   _deleteInProgress;
        string                                 _deleteText;
        string                                 _deleteTitle;
        bool                                   _deletingCompanyByDocument;
        bool                                   _deletingDocumentByMachine;
        bool                                   _deletingDocumentByMachineFamily;
        bool                                   _deletingPersonByDocument;
        bool                                   _deletingScan;
        List<CompanyByDocumentViewModel>       _documentCompanies;
        List<DocumentByMachineFamilyViewModel> _documentMachineFamilies;
        List<DocumentByMachineViewModel>       _documentMachines;
        List<PersonByDocumentViewModel>        _documentPeople;
        bool                                   _editing;
        bool                                   _editingScan;
        bool?                                  _extractExif;
        Modal                                  _frmDelete;
        string                                 _imageFormat;
        ElementReference                       _inputUpload;
        bool                                   _loaded;
        List<MachineFamilyViewModel>           _machineFamilies;
        List<MachineViewModel>                 _machines;
        DocumentViewModel                      _model;
        bool?                                  _moveFile;
        List<DocumentPersonViewModel>          _people;
        double                                 _progressValue;
        List<DocumentRoleViewModel>            _roles;
        bool                                   _savingCompany;
        bool                                   _savingMachine;
        bool                                   _savingMachineFamily;
        bool                                   _savingPerson;
        List<Guid>                             _scans;
        ApplicationUser                        _scanUser;
        DocumentScanViewModel                  _selectedScan;
        bool                                   _unknownCountry;
        bool                                   _unknownNativeTitle;
        bool                                   _unknownPublished;
        bool                                   _unknownScanAuthor;
        bool                                   _unknownScanColorSpace;
        bool                                   _unknownScanComments;
        bool                                   _unknownScanCreationDate;
        bool                                   _unknownScanExifVersion;
        bool                                   _unknownScanHorizontalResolution;
        bool                                   _unknownScanPage;
        bool                                   _unknownScanResolutionUnit;
        bool                                   _unknownScanScannerManufacturer;
        bool                                   _unknownScanScannerModel;
        bool                                   _unknownScanSoftwareUsed;
        bool                                   _unknownScanVerticalResolution;
        bool                                   _uploaded;
        bool                                   _uploadError;
        string                                 _uploadErrorMessage;
        bool                                   _uploading;
        uint?                                  _uploadScanPage;
        uint                                   _uploadScanType;

        [Parameter]
        public long Id { get; set; }

        ushort ScanColorSpace
        {
            get
            {
                if(_selectedScan.ColorSpace is null)
                    return 0;

                return (ushort)_selectedScan.ColorSpace;
            }
            set => _selectedScan.ColorSpace = (ColorSpace)value;
        }

        ushort ScanResolutionUnit
        {
            get
            {
                if(_selectedScan.ResolutionUnit is null)
                    return 0;

                return (ushort)_selectedScan.ResolutionUnit;
            }
            set => _selectedScan.ResolutionUnit = (ResolutionUnit)value;
        }

        uint ScanType
        {
            get
            {
                if(_selectedScan.ResolutionUnit is null)
                    return 0;

                return (uint)_selectedScan.ResolutionUnit;
            }
            set => _selectedScan.Type = (DocumentScanType)value;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/documents/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _countries               = await CountriesService.GetAsync();
            _companies               = await CompaniesService.GetAsync();
            _roles                   = await DocumentRolesService.GetEnabledAsync();
            _machineFamilies         = await MachineFamiliesService.GetAsync();
            _machines                = await MachinesService.GetAsync();
            _model                   = _creating ? new DocumentViewModel() : await Service.GetAsync(Id);
            _authState               = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            _addingCompanyRoleId     = _roles.First().Id;
            _documentCompanies       = await CompaniesByDocumentService.GetByDocument(Id);
            _addingMachineFamilyId   = _machineFamilies.First().Id;
            _documentMachineFamilies = await DocumentsByMachineFamilyService.GetByDocument(Id);
            _addingMachineId         = _machines.First().Id;
            _documentMachines        = await DocumentsByMachineService.GetByDocument(Id);
            _addingPersonRoleId      = _roles.First().Id;
            _documentPeople          = await PeopleByDocumentService.GetByDocument(Id);
            _scans                   = await DocumentScansService.GetGuidsByDocumentAsync(Id);

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/documents/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownCountry     = !_model.CountryId.HasValue;
            _unknownNativeTitle = string.IsNullOrWhiteSpace(_model.NativeTitle);
            _unknownPublished   = !_model.Published.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/documents");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownNativeTitle)
                _model.NativeTitle = null;
            else if(string.IsNullOrWhiteSpace(_model.NativeTitle))
                return;

            if(_unknownCountry)
                _model.CountryId = null;
            else if(_model.CountryId < 0)
                return;

            if(_unknownPublished)
                _model.Published = null;
            else if(_model.Published?.Date >= DateTime.UtcNow.Date)
                return;

            if(string.IsNullOrWhiteSpace(_model.Title))
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

        void ValidateTitle(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Title must be smaller than 256 characters."], 256);

        void ValidatePublished(ValidatorEventArgs e) => Validators.ValidateDate(e);

        void ValidateNativeTitle(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Native title must be smaller than 256 characters."], 256);

        void OnAddCompanyClick()
        {
            _addingCompany   = true;
            _savingCompany   = false;
            _addingCompanyId = _companies.First().Id;
        }

        void CancelAddCompany()
        {
            _addingCompany   = false;
            _savingCompany   = false;
            _addingCompanyId = null;
        }

        async Task ConfirmAddCompany()
        {
            if(_addingCompanyId is null ||
               _addingCompanyId <= 0)
            {
                CancelAddCompany();

                return;
            }

            _savingCompany = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await CompaniesByDocumentService.CreateAsync(_addingCompanyId.Value, Id, _addingCompanyRoleId,
                                                         (await UserManager.GetUserAsync(_authState.User)).Id);

            _documentCompanies = await CompaniesByDocumentService.GetByDocument(Id);

            _addingCompany   = false;
            _savingCompany   = false;
            _addingCompanyId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ShowCompanyDeleteModal(long itemId)
        {
            _currentCompanyByDocument  = _documentCompanies.FirstOrDefault(n => n.Id == itemId);
            _deletingCompanyByDocument = true;
            _deleteTitle               = L["Delete company from this document"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the company {0} with role {1} from this document?"],
                              _currentCompanyByDocument?.Company, _currentCompanyByDocument?.Role);

            _frmDelete.Show();
        }

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress                = false;
            _deletingCompanyByDocument       = false;
            _currentCompanyByDocument        = null;
            _deletingDocumentByMachineFamily = false;
            _currentDocumentByMachineFamily  = null;
            _deletingDocumentByMachine       = false;
            _currentDocumentByMachine        = null;
            _deletingPersonByDocument        = false;
            _currentPersonByDocument         = null;
            _deletingScan                    = false;
            _selectedScan                    = null;
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_deletingCompanyByDocument)
                await ConfirmDeleteCompanyByMachine();
            else if(_deletingDocumentByMachineFamily)
                await ConfirmDeleteDocumentByMachineFamily();
            else if(_deletingDocumentByMachine)
                await ConfirmDeleteDocumentByMachine();
            else if(_deletingPersonByDocument)
                await ConfirmDeletePersonByDocument();
            else if(_deletingScan)
                await ConfirmDeleteScan();
        }

        async Task ConfirmDeleteCompanyByMachine()
        {
            if(_currentCompanyByDocument is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await CompaniesByDocumentService.DeleteAsync(_currentCompanyByDocument.Id,
                                                         (await UserManager.GetUserAsync(_authState.User)).Id);

            _documentCompanies = await CompaniesByDocumentService.GetByDocument(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void OnAddFamilyClick()
        {
            _addingMachineFamily   = true;
            _savingMachineFamily   = false;
            _addingMachineFamilyId = _machineFamilies.First().Id;
        }

        void CancelAddFamily()
        {
            _addingMachineFamily   = false;
            _savingMachineFamily   = false;
            _addingMachineFamilyId = null;
        }

        async Task ConfirmAddFamily()
        {
            if(_addingMachineFamilyId is null ||
               _addingMachineFamilyId <= 0)
            {
                CancelAddFamily();

                return;
            }

            _savingMachineFamily = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await DocumentsByMachineFamilyService.CreateAsync(_addingMachineFamilyId.Value, Id,
                                                              (await UserManager.GetUserAsync(_authState.User)).Id);

            _documentMachineFamilies = await DocumentsByMachineFamilyService.GetByDocument(Id);

            _addingMachineFamily   = false;
            _savingMachineFamily   = false;
            _addingMachineFamilyId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ShowMachineFamilyDeleteModal(long itemId)
        {
            _currentDocumentByMachineFamily  = _documentMachineFamilies.FirstOrDefault(n => n.Id == itemId);
            _deletingDocumentByMachineFamily = true;
            _deleteTitle                     = L["Delete machine family from this document"];

            _deleteText = string.Format(L["Are you sure you want to delete the machine family {0} from this document?"],
                                        _currentDocumentByMachineFamily?.MachineFamily);

            _frmDelete.Show();
        }

        async Task ConfirmDeleteDocumentByMachineFamily()
        {
            if(_currentDocumentByMachineFamily is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await DocumentsByMachineFamilyService.DeleteAsync(_currentDocumentByMachineFamily.Id,
                                                              (await UserManager.GetUserAsync(_authState.User)).Id);

            _documentMachineFamilies = await DocumentsByMachineFamilyService.GetByDocument(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void OnAddMachineClick()
        {
            _addingMachine   = true;
            _savingMachine   = false;
            _addingMachineId = _machines.First().Id;
        }

        void CancelAddMachine()
        {
            _addingMachine   = false;
            _savingMachine   = false;
            _addingMachineId = null;
        }

        async Task ConfirmAddMachine()
        {
            if(_addingMachineId is null ||
               _addingMachineId <= 0)
            {
                CancelAddMachine();

                return;
            }

            _savingMachine = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await DocumentsByMachineService.CreateAsync(_addingMachineId.Value, Id,
                                                        (await UserManager.GetUserAsync(_authState.User)).Id);

            _documentMachines = await DocumentsByMachineService.GetByDocument(Id);

            _addingMachine   = false;
            _savingMachine   = false;
            _addingMachineId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ShowMachineDeleteModal(long itemId)
        {
            _currentDocumentByMachine  = _documentMachines.FirstOrDefault(n => n.Id == itemId);
            _deletingDocumentByMachine = true;
            _deleteTitle               = L["Delete machine from this document"];

            _deleteText = string.Format(L["Are you sure you want to delete the machine {0} from this document?"],
                                        _currentDocumentByMachine?.Machine);

            _frmDelete.Show();
        }

        async Task ConfirmDeleteDocumentByMachine()
        {
            if(_currentDocumentByMachine is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await DocumentsByMachineService.DeleteAsync(_currentDocumentByMachine.Id,
                                                        (await UserManager.GetUserAsync(_authState.User)).Id);

            _documentMachines = await DocumentsByMachineService.GetByDocument(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void OnAddPersonClick()
        {
            _addingPerson   = true;
            _savingPerson   = false;
            _addingPersonId = _people.First().Id;
        }

        void CancelAddPerson()
        {
            _addingPerson   = false;
            _savingPerson   = false;
            _addingPersonId = null;
        }

        async Task ConfirmAddPerson()
        {
            if(_addingPersonId is null ||
               _addingPersonId <= 0)
            {
                CancelAddPerson();

                return;
            }

            _savingPerson = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await PeopleByDocumentService.CreateAsync(_addingPersonId.Value, Id, _addingPersonRoleId,
                                                      (await UserManager.GetUserAsync(_authState.User)).Id);

            _documentPeople = await PeopleByDocumentService.GetByDocument(Id);

            _addingPerson   = false;
            _savingPerson   = false;
            _addingPersonId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ShowPersonDeleteModal(long itemId)
        {
            _currentPersonByDocument  = _documentPeople.FirstOrDefault(n => n.Id == itemId);
            _deletingPersonByDocument = true;
            _deleteTitle              = L["Delete person from this document"];

            _deleteText =
                string.Format(L["Are you sure you want to delete the person {0} with role {1} from this document?"],
                              _currentPersonByDocument?.FullName, _currentPersonByDocument?.Role);

            _frmDelete.Show();
        }

        async Task ConfirmDeletePersonByDocument()
        {
            if(_currentPersonByDocument is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await PeopleByDocumentService.DeleteAsync(_currentPersonByDocument.Id,
                                                      (await UserManager.GetUserAsync(_authState.User)).Id);

            _documentPeople = await PeopleByDocumentService.GetByDocument(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ValidateScanAuthor(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Scan author must be smaller than 256 characters."], 256);

        void ValidateScannerManufacturer(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Scanner manufacturer must be smaller than 256 characters."], 256);

        void ValidateScannerModel(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Scanner model must be smaller than 256 characters."], 256);

        void ValidateDate(ValidatorEventArgs e)
        {
            if(!(e.Value is DateTime item) ||
               item.Year <= 1816           ||
               item      >= DateTime.UtcNow)
                e.Status = ValidationStatus.Error;
            else
                e.Status = ValidationStatus.Success;
        }

        void ValidateDoubleBiggerThanZero(ValidatorEventArgs e)
        {
            if(e.Value is double item &&
               item > 0)
                e.Status = ValidationStatus.Success;
            else
                e.Status = ValidationStatus.Error;
        }

        void ValidateExifVersion(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Exif version must be 255 characters or less."], 255);

        void ValidateLens(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Lens name must be 255 characters or less."], 255);

        void ValidateSoftwareUsed(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Software used must be 255 characters or less."], 255);

        void ValidateComments(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["User comments must be 255 characters or less."], 255);

        void ValidateUnsignedIntegerBiggerThanZero(ValidatorEventArgs e)
        {
            if(e.Value is uint item &&
               item > 0)
                e.Status = ValidationStatus.Success;
            else
                e.Status = ValidationStatus.Error;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        async Task UploadFile()
        {
            var processExiftool = new Process
            {
                StartInfo =
                {
                    FileName               = "exiftool",
                    CreateNoWindow         = true,
                    RedirectStandardError  = true,
                    RedirectStandardOutput = true
                }
            };

            var processIdentify = new Process
            {
                StartInfo =
                {
                    FileName               = "identify",
                    CreateNoWindow         = true,
                    RedirectStandardError  = true,
                    RedirectStandardOutput = true
                }
            };

            var processConvert = new Process
            {
                StartInfo =
                {
                    FileName               = "convert",
                    CreateNoWindow         = true,
                    RedirectStandardError  = true,
                    RedirectStandardOutput = true
                }
            };

            string identifyOutput;
            string convertOutput;
            string exiftoolOutput;

            try
            {
                processIdentify.Start();
                identifyOutput = await processIdentify.StandardOutput.ReadToEndAsync();
                processIdentify.WaitForExit();
                processConvert.Start();
                convertOutput = await processConvert.StandardOutput.ReadToEndAsync();
                processConvert.WaitForExit();
                processExiftool.Start();
                exiftoolOutput = await processExiftool.StandardOutput.ReadToEndAsync();
                processExiftool.WaitForExit();
            }
            catch(Exception)
            {
                _uploadError        = true;
                _uploadErrorMessage = L["Cannot run ImageMagick please contact the administrator."];

                return;
            }

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

            string tmpPath = Path.GetTempFileName();

            FileStream outFs;

            try
            {
                outFs = new FileStream(tmpPath, FileMode.Open, FileAccess.ReadWrite);
            }
            catch(Exception)
            {
                _uploadError        = true;
                _uploadErrorMessage = L["There was an error uploading the file."];

                return;
            }

            _uploading = true;

            await using AsyncDisposableStream fs     = await file.OpenReadAsync();
            byte[]                            buffer = new byte[20480];

            try
            {
                double lastProgress = 0;
                int    count;

                while((count = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await outFs.WriteAsync(buffer, 0, count);

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

            outFs.Close();
            _uploading = false;
            await InvokeAsync(StateHasChanged);
            await Task.Yield();

            processIdentify = new Process
            {
                StartInfo =
                {
                    FileName               = "identify",
                    CreateNoWindow         = true,
                    RedirectStandardError  = true,
                    RedirectStandardOutput = true,
                    ArgumentList =
                    {
                        tmpPath
                    }
                }
            };

            processIdentify.Start();
            identifyOutput = await processIdentify.StandardOutput.ReadToEndAsync();
            processIdentify.WaitForExit();

            if(processIdentify.ExitCode != 0 ||
               string.IsNullOrWhiteSpace(identifyOutput))
            {
                _uploading          = false;
                _uploadError        = true;
                _uploadErrorMessage = L["The uploaded file was not recognized as an image."];
                File.Delete(tmpPath);

                return;
            }

            string[] pieces = identifyOutput.Substring(tmpPath.Length).
                                             Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if(pieces.Length < 2)
            {
                _uploading          = false;
                _uploadError        = true;
                _uploadErrorMessage = L["The uploaded file was not recognized as an image."];
                File.Delete(tmpPath);

                return;
            }

            // TODO: Move this to Helpers, keep progress

            string extension = ImageMagick.GetExtension(pieces[0]);

            if(string.IsNullOrWhiteSpace(extension))
            {
                _uploading          = false;
                _uploadError        = true;
                _uploadErrorMessage = L["The uploaded file was not recognized as an image."];
                File.Delete(tmpPath);

                return;
            }

            _imageFormat = pieces[0];
            _uploaded    = true;

            _selectedScan = new DocumentScanViewModel
            {
                UserId            = (await UserManager.GetUserAsync(_authState.User)).Id,
                DocumentId        = Id,
                Id                = Guid.NewGuid(),
                OriginalExtension = extension,
                UploadDate        = DateTime.UtcNow,
                Page              = _unknownScanPage ? null : _uploadScanPage,
                Type              = (DocumentScanType)_uploadScanType
            };

            try
            {
                processExiftool = new Process
                {
                    StartInfo =
                    {
                        FileName               = "exiftool",
                        CreateNoWindow         = true,
                        RedirectStandardError  = true,
                        RedirectStandardOutput = true,
                        ArgumentList =
                        {
                            "-n",
                            "-json",
                            tmpPath
                        }
                    }
                };

                processExiftool.Start();
                exiftoolOutput = await processExiftool.StandardOutput.ReadToEndAsync();
                processExiftool.WaitForExit();

                Exif[] exif = JsonSerializer.Deserialize<Exif[]>(exiftoolOutput);

                if(exif?.Length >= 1)
                    exif[0].ToViewModel(_selectedScan);

                _extractExif = true;
            }
            catch(Exception)
            {
                _extractExif = false;
            }

            string originalFilePath = Path.Combine(Host.WebRootPath, "assets", "scans", "documents", "originals",
                                                   $"{_selectedScan.Id}{_selectedScan.OriginalExtension}");

            try
            {
                File.Move(tmpPath, originalFilePath);
                _moveFile = true;
            }
            catch(Exception)
            {
                _moveFile = false;
                File.Delete(tmpPath);

                return;
            }

            await Task.Yield();
            await InvokeAsync(StateHasChanged);

            var photos = new Photos();
            photos.FinishedAll                        += OnFinishedAll;
            photos.FinishedRenderingJpeg4k            += OnFinishedRenderingJpeg4k;
            photos.FinishedRenderingJpeg1440          += OnFinishedRenderingJpeg1440;
            photos.FinishedRenderingJpegHd            += OnFinishedRenderingJpegHd;
            photos.FinishedRenderingJpeg4kThumbnail   += OnFinishedRenderingJpeg4kThumbnail;
            photos.FinishedRenderingJpeg1440Thumbnail += OnFinishedRenderingJpeg1440Thumbnail;
            photos.FinishedRenderingJpegHdThumbnail   += OnFinishedRenderingJpegHdThumbnail;
            photos.FinishedRenderingJp2k4k            += OnFinishedRenderingJp2k4k;
            photos.FinishedRenderingJp2k1440          += OnFinishedRenderingJp2k1440;
            photos.FinishedRenderingJp2kHd            += OnFinishedRenderingJp2kHd;
            photos.FinishedRenderingJp2k4kThumbnail   += OnFinishedRenderingJp2k4kThumbnail;
            photos.FinishedRenderingJp2k1440Thumbnail += OnFinishedRenderingJp2k1440Thumbnail;
            photos.FinishedRenderingJp2kHdThumbnail   += OnFinishedRenderingJp2kHdThumbnail;
            photos.FinishedRenderingWebp4k            += OnFinishedRenderingWebp4k;
            photos.FinishedRenderingWebp1440          += OnFinishedRenderingWebp1440;
            photos.FinishedRenderingWebpHd            += OnFinishedRenderingWebpHd;
            photos.FinishedRenderingWebp4kThumbnail   += OnFinishedRenderingWebp4kThumbnail;
            photos.FinishedRenderingWebp1440Thumbnail += OnFinishedRenderingWebp1440Thumbnail;
            photos.FinishedRenderingWebpHdThumbnail   += OnFinishedRenderingWebpHdThumbnail;
            photos.FinishedRenderingHeif4k            += OnFinishedRenderingHeif4k;
            photos.FinishedRenderingHeif1440          += OnFinishedRenderingHeif1440;
            photos.FinishedRenderingHeifHd            += OnFinishedRenderingHeifHd;
            photos.FinishedRenderingHeif4kThumbnail   += OnFinishedRenderingHeif4kThumbnail;
            photos.FinishedRenderingHeif1440Thumbnail += OnFinishedRenderingHeif1440Thumbnail;
            photos.FinishedRenderingHeifHdThumbnail   += OnFinishedRenderingHeifHdThumbnail;
            photos.FinishedRenderingAvif4k            += OnFinishedRenderingAvif4k;
            photos.FinishedRenderingAvif1440          += OnFinishedRenderingAvif1440;
            photos.FinishedRenderingAvifHd            += OnFinishedRenderingAvifHd;
            photos.FinishedRenderingAvif4kThumbnail   += OnFinishedRenderingAvif4kThumbnail;
            photos.FinishedRenderingAvif1440Thumbnail += OnFinishedRenderingAvif1440Thumbnail;
            photos.FinishedRenderingAvifHdThumbnail   += OnFinishedRenderingAvifHdThumbnail;

            #pragma warning disable 4014
            Task.Run(() => photos.ConversionWorker(Host.WebRootPath, _selectedScan.Id, originalFilePath, _imageFormat,
                                                   true, "documents"));
            #pragma warning restore 4014
        }

        async Task OnFinishedRenderingJpeg4k(bool result)
        {
            _convertJpeg4k = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJpeg1440(bool result)
        {
            _convertJpeg1440 = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJpegHd(bool result)
        {
            _convertJpegHd = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJpeg4kThumbnail(bool result)
        {
            _convertJpeg4kTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJpeg1440Thumbnail(bool result)
        {
            _convertJpeg1440Th = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJpegHdThumbnail(bool result)
        {
            _convertJpegHdTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2k4k(bool result)
        {
            _convertJp2k4k = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2k1440(bool result)
        {
            _convertJp2k1440 = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2kHd(bool result)
        {
            _convertJp2kHd = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2k4kThumbnail(bool result)
        {
            _convertJp2k4kTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2k1440Thumbnail(bool result)
        {
            _convertJp2k1440Th = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingJp2kHdThumbnail(bool result)
        {
            _convertJp2kHdTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebp4k(bool result)
        {
            _convertWebp4k = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebp1440(bool result)
        {
            _convertWebp1440 = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebpHd(bool result)
        {
            _convertWebpHd = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebp4kThumbnail(bool result)
        {
            _convertWebp4kTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebp1440Thumbnail(bool result)
        {
            _convertWebp1440Th = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingWebpHdThumbnail(bool result)
        {
            _convertWebpHdTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeif4k(bool result)
        {
            _convertHeif4k = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeif1440(bool result)
        {
            _convertHeif1440 = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeifHd(bool result)
        {
            _convertHeifHd = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeif4kThumbnail(bool result)
        {
            _convertHeif4kTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeif1440Thumbnail(bool result)
        {
            _convertHeif1440Th = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingHeifHdThumbnail(bool result)
        {
            _convertHeifHdTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvif4k(bool result)
        {
            _convertAvif4k = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvif1440(bool result)
        {
            _convertAvif1440 = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvifHd(bool result)
        {
            _convertAvifHd = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvif4kThumbnail(bool result)
        {
            _convertAvif4kTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvif1440Thumbnail(bool result)
        {
            _convertAvif1440Th = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedRenderingAvifHdThumbnail(bool result)
        {
            _convertAvifHdTh = result;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task OnFinishedAll(bool result)
        {
            try
            {
                await DocumentScansService.CreateAsync(_selectedScan,
                                                       (await UserManager.GetUserAsync(_authState.User)).Id);

                _addToDatabase = true;
            }
            catch(Exception e)
            {
                _addToDatabase = false;
                Console.WriteLine(e);

                throw;
            }

            _allFinished = true;
            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task AddScan()
        {
            _editing     = false;
            _editingScan = false;
            _addingScan  = true;

            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task EditScan(Guid scanId)
        {
            _selectedScan = await DocumentScansService.GetAsync(scanId);

            if(_selectedScan is null)
                return;

            _scanUser                        = await UserManager.FindByIdAsync(_selectedScan.UserId);
            _unknownScanAuthor               = string.IsNullOrWhiteSpace(_selectedScan.Author);
            _unknownScanScannerManufacturer  = string.IsNullOrWhiteSpace(_selectedScan.ScannerModel);
            _unknownScanScannerModel         = string.IsNullOrWhiteSpace(_selectedScan.ScannerModel);
            _unknownScanColorSpace           = !_selectedScan.ColorSpace.HasValue;
            _unknownScanCreationDate         = !_selectedScan.CreationDate.HasValue;
            _unknownScanExifVersion          = string.IsNullOrWhiteSpace(_selectedScan.ExifVersion);
            _unknownScanHorizontalResolution = !_selectedScan.HorizontalResolution.HasValue;
            _unknownScanResolutionUnit       = !_selectedScan.ResolutionUnit.HasValue;
            _unknownScanSoftwareUsed         = string.IsNullOrWhiteSpace(_selectedScan.SoftwareUsed);
            _unknownScanVerticalResolution   = !_selectedScan.VerticalResolution.HasValue;
            _unknownScanComments             = string.IsNullOrWhiteSpace(_selectedScan.Comments);
            _unknownScanPage                 = !_selectedScan.Page.HasValue;

            _editing     = false;
            _editingScan = true;
            _addingScan  = false;

            await Task.Yield();
            await InvokeAsync(StateHasChanged);
        }

        async Task ShowScanDeleteModal(Guid scanId)
        {
            _selectedScan = await DocumentScansService.GetAsync(scanId);

            if(_selectedScan is null)
                return;

            _deletingScan = true;
            _deleteTitle  = L["Delete scan from this document"];

            _deleteText = _selectedScan.Page.HasValue
                              ? string.
                                  Format(L["Are you sure you want to delete the scan type {0} for page {1} from this document?"],
                                         _selectedScan.Type, _selectedScan.Page)
                              : string.
                                  Format(L["Are you sure you want to delete the scan type {0} from this document?"],
                                         _selectedScan.Type);

            _frmDelete.Show();
        }

        async Task ConfirmDeleteScan()
        {
            if(_selectedScan is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            // TODO: Delete files
            await DocumentScansService.DeleteAsync(_selectedScan.Id,
                                                   (await UserManager.GetUserAsync(_authState.User)).Id);

            _scans = await DocumentScansService.GetGuidsByDocumentAsync(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void OnCancelScanClicked()
        {
            _scanUser                        = null;
            _unknownScanAuthor               = true;
            _unknownScanScannerManufacturer  = true;
            _unknownScanScannerModel         = true;
            _unknownScanColorSpace           = true;
            _unknownScanCreationDate         = true;
            _unknownScanExifVersion          = true;
            _unknownScanHorizontalResolution = true;
            _unknownScanResolutionUnit       = true;
            _unknownScanSoftwareUsed         = true;
            _unknownScanVerticalResolution   = true;
            _unknownScanComments             = true;
            _unknownScanPage                 = true;
            _selectedScan                    = null;

            _editing     = false;
            _editingScan = false;
            _addingScan  = false;

            StateHasChanged();
        }

        async void OnSaveScanClicked()
        {
            if(_unknownScanAuthor)
                _selectedScan.Author = null;
            else if(string.IsNullOrWhiteSpace(_selectedScan.Author))
                return;

            if(_unknownScanScannerManufacturer)
                _selectedScan.ScannerModel = null;
            else if(string.IsNullOrWhiteSpace(_selectedScan.ScannerModel))
                return;

            if(_unknownScanScannerModel)
                _selectedScan.ScannerManufacturer = null;
            else if(string.IsNullOrWhiteSpace(_selectedScan.ScannerManufacturer))
                return;

            if(_unknownScanColorSpace)
                _selectedScan.ColorSpace = null;
            else if(!_selectedScan.ColorSpace.HasValue)
                return;

            if(_unknownScanCreationDate)
                _selectedScan.CreationDate = null;
            else if(_selectedScan.CreationDate > DateTime.UtcNow)
                return;

            if(_unknownScanExifVersion)
                _selectedScan.ExifVersion = null;
            else if(string.IsNullOrWhiteSpace(_selectedScan.ExifVersion))
                return;

            if(_unknownScanHorizontalResolution)
                _selectedScan.HorizontalResolution = null;
            else if(!_selectedScan.HorizontalResolution.HasValue)
                return;

            if(_unknownScanResolutionUnit)
                _selectedScan.ResolutionUnit = null;
            else if(!_selectedScan.ResolutionUnit.HasValue)
                return;

            if(_unknownScanSoftwareUsed)
                _selectedScan.SoftwareUsed = null;
            else if(string.IsNullOrWhiteSpace(_selectedScan.SoftwareUsed))
                return;

            if(_unknownScanVerticalResolution)
                _selectedScan.VerticalResolution = null;
            else if(!_selectedScan.VerticalResolution.HasValue)
                return;

            if(_unknownScanComments)
                _selectedScan.Comments = null;
            else if(string.IsNullOrWhiteSpace(_selectedScan.Comments))
                return;

            if(_unknownScanPage)
                _selectedScan.Page = null;
            else if(!_selectedScan.Page.HasValue ||
                    _selectedScan.Page == 0)
                return;

            await DocumentScansService.UpdateAsync(_selectedScan, (await UserManager.GetUserAsync(_authState.User)).Id);

            OnCancelScanClicked();
            StateHasChanged();
        }
    }
}