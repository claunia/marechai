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
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Marechai.Pages.Admin.Details
{
    public partial class SoundSynth
    {
        AuthenticationState    _authState;
        List<CompanyViewModel> _companies;
        bool                   _creating;
        bool                   _editing;
        bool                   _loaded;
        SoundSynthViewModel    _model;
        bool                   _prototype;
        bool                   _unknownCompany;
        bool                   _unknownIntroduced;
        bool                   _unknownModelCode;
        bool                   _unknownSampleRate;
        bool                   _unknownSampleResolution;
        bool                   _unknownSquareWaveChannels;
        bool                   _unknownType;
        bool                   _unknownVoices;
        bool                   _unknownWhiteNoiseChannels;
        [Parameter]
        public int Id { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(_loaded)
                return;

            _loaded = true;

            _creating = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                          StartsWith("admin/sound_synths/create", StringComparison.InvariantCulture);

            if(Id <= 0 &&
               !_creating)
                return;

            _companies = await CompaniesService.GetAsync();
            _model     = _creating ? new SoundSynthViewModel() : await Service.GetAsync(Id);
            _authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            _editing = _creating || NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                                      StartsWith("admin/sound_synths/edit/",
                                                                 StringComparison.InvariantCulture);

            if(_editing)
                SetCheckboxes();

            StateHasChanged();
        }

        void SetCheckboxes()
        {
            _unknownCompany            = !_model.CompanyId.HasValue;
            _unknownModelCode          = string.IsNullOrWhiteSpace(_model.ModelCode);
            _unknownIntroduced         = !_model.Introduced.HasValue;
            _prototype                 = _model.Introduced?.Year == 1000;
            _unknownVoices             = !_model.Voices.HasValue;
            _unknownSampleRate         = !_model.Frequency.HasValue;
            _unknownSampleResolution   = !_model.Depth.HasValue;
            _unknownSquareWaveChannels = !_model.SquareWave.HasValue;
            _unknownWhiteNoiseChannels = !_model.WhiteNoise.HasValue;
            _unknownType               = !_model.Type.HasValue;
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
                NavigationManager.ToBaseRelativePath("admin/sound_synths");

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

            if(_unknownVoices)
                _model.Voices = null;
            else if(_model.Voices < 0)
                return;

            if(_unknownSampleRate)
                _model.Frequency = null;
            else if(_model.Frequency < 0)
                return;

            if(_unknownSampleResolution)
                _model.Depth = null;
            else if(_model.Depth < 0)
                return;

            if(_unknownSquareWaveChannels)
                _model.SquareWave = null;
            else if(_model.SquareWave < 0)
                return;

            if(_unknownWhiteNoiseChannels)
                _model.WhiteNoise = null;
            else if(_model.WhiteNoise < 0)
                return;

            if(_unknownType)
                _model.Type = null;
            else if(_model.Type < 0)
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

        void ValidateModelCode(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Model code must be 45 characters or less."], 45);

        void ValidateIntroduced(ValidatorEventArgs e) => Validators.ValidateIntroducedDate(e);

        void ValidateIntegerBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateInteger(e);

        void ValidateDoubleBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateDouble(e);

        void ValidateName(ValidatorEventArgs e) =>
            Validators.ValidateString(e, L["Name must be 50 characters or less."], 50);
    }
}