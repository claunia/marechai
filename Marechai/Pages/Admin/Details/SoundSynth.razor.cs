using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class SoundSynth
    {
        List<CompanyViewModel> _companies;
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

            if(Id <= 0)
                return;

            _companies = await CompaniesService.GetAsync();
            _model     = await Service.GetAsync(Id);

            _editing = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLowerInvariant().
                                         StartsWith("admin/sound_synths/edit/", StringComparison.InvariantCulture);

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
            _model   = await Service.GetAsync(Id);
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

            _editing = false;
            await Service.UpdateAsync(_model);
            _model = await Service.GetAsync(Id);
            SetCheckboxes();
            StateHasChanged();
        }

        void ValidateModelCode(ValidatorEventArgs e) =>
            Validators.ValidateStringWithMaxLength(e, L["Model code must be 45 characters or less."], 45);

        void ValidateIntroduced(ValidatorEventArgs e) => Validators.ValidateIntroducedDate(e);

        void ValidateIntegerBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateIntegerBiggerThanZero(e);

        void ValidateDoubleBiggerThanZero(ValidatorEventArgs e) => Validators.ValidateDoubleBiggerThanZero(e);
    }
}