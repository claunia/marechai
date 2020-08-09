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
using System.Threading.Tasks;
using Blazorise;
using Marechai.Database;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Admin.Details
{
    public partial class SoftwareVariant
    {
        AuthenticationState            _authState;
        bool                           _creating;
        bool                           _editing;
        bool                           _loaded;
        SoftwareVariantViewModel       _model;
        List<SoftwareVariantViewModel> _softwareVariants;
        List<SoftwareVersionViewModel> _softwareVersions;
        bool                           _unknownCatalogueNumber;
        bool                           _unknownIntroduced;
        bool                           _unknownMinimumMemory;
        bool                           _unknownName;
        bool                           _unknownParent;
        bool                           _unknownPartNumber;
        bool                           _unknownProductCode;
        bool                           _unknownRecommendedMemory;
        bool                           _unknownRequiredStorage;

        bool _unknownSerialNumber;
        bool _unknownVersion;

        [Parameter]
        public ulong Id { get; set; }

        uint Distribution
        {
            get => (uint)_model.DistributionMode;
            set => _model.DistributionMode = (DistributionMode)value;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/software_variants/create",
                                                     StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _softwareVariants = await Service.GetAsync();
            _softwareVersions = await SoftwareVersionsService.GetAsync();
            _model            = _creating ? new SoftwareVariantViewModel() : await Service.GetAsync(Id);
            _authState        = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/software_variants/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownName              = string.IsNullOrWhiteSpace(_model.Name);
            _unknownVersion           = string.IsNullOrWhiteSpace(_model.Version);
            _unknownParent            = !_model.ParentId.HasValue;
            _unknownMinimumMemory     = !_model.MinimumMemory.HasValue;
            _unknownRecommendedMemory = !_model.RecommendedMemory.HasValue;
            _unknownRequiredStorage   = !_model.RequiredStorage.HasValue;
            _unknownPartNumber        = string.IsNullOrWhiteSpace(_model.PartNumber);
            _unknownSerialNumber      = string.IsNullOrWhiteSpace(_model.SerialNumber);
            _unknownProductCode       = string.IsNullOrWhiteSpace(_model.ProductCode);
            _unknownCatalogueNumber   = string.IsNullOrWhiteSpace(_model.CatalogueNumber);
            _unknownIntroduced        = !_model.Introduced.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/software_variants");

                return;
            }

            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        async void OnSaveClicked()
        {
            if(_unknownName)
                _model.Name = null;
            else if(string.IsNullOrWhiteSpace(_model.Name))
                return;

            if(_unknownVersion)
                _model.Version = null;
            else if(string.IsNullOrWhiteSpace(_model.Version))
                return;

            if(_unknownPartNumber)
                _model.PartNumber = null;
            else if(string.IsNullOrWhiteSpace(_model.PartNumber))
                return;

            if(_unknownSerialNumber)
                _model.SerialNumber = null;
            else if(string.IsNullOrWhiteSpace(_model.SerialNumber))
                return;

            if(_unknownProductCode)
                _model.ProductCode = null;
            else if(string.IsNullOrWhiteSpace(_model.ProductCode))
                return;

            if(_unknownCatalogueNumber)
                _model.CatalogueNumber = null;
            else if(string.IsNullOrWhiteSpace(_model.CatalogueNumber))
                return;

            if(_unknownParent)
                _model.ParentId = null;
            else if(_model.ParentId < 1)
                return;

            if(_unknownIntroduced)
                _model.Introduced = null;
            else if(_model.Introduced?.Date >= DateTime.UtcNow.Date)
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

        void ValidateVersion(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Version must be smaller than 256 characters."], 256);

        void ValidatePartNumber(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Part number must be smaller than 256 characters."], 256);

        void ValidateSerialNumber(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Serial number must be smaller than 256 characters."], 256);

        void ValidateProductCode(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Product number must be smaller than 256 characters."], 256);

        void ValidateCatalogueNumber(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Catalogue number must be smaller than 256 characters."], 256);

        void ValidateIntroduced(ValidatorEventArgs e) => Validators.ValidateDate(e);

        void ValidateUnsignedLongBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateUnsignedLong(e);
    }
}