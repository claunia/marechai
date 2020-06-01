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
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using Marechai.Shared;
using Marechai.ViewModels;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Admin.Details
{
    public partial class Screen
    {
        bool                              _addingResolution;
        int?                              _addingResolutionId;
        bool                              _creating;
        ResolutionByScreenViewModel       _currentResolution;
        bool                              _deleteInProgress;
        string                            _deleteText;
        string                            _deleteTitle;
        bool                              _editing;
        Modal                             _frmDelete;
        bool                              _loaded;
        ScreenViewModel                   _model;
        List<ResolutionViewModel>         _resolutions;
        bool                              _savingResolution;
        List<ResolutionByScreenViewModel> _screenResolutions;
        bool                              _unknownColors;
        bool                              _unknownHeight;
        bool                              _unknownType;
        bool                              _unknownWidth;
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

            _resolutions       = await ResolutionsService.GetAsync();
            _model             = _creating ? new ScreenViewModel() : await Service.GetAsync(Id);
            _screenResolutions = await ResolutionsByScreenService.GetByScreen(Id);

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

        void ShowResolutionDeleteModal(long itemId)
        {
            _currentResolution = _screenResolutions.FirstOrDefault(n => n.Id == itemId);
            _deleteTitle       = L["Delete resolution from this screen"];

            _deleteText = string.Format(L["Are you sure you want to delete the resolution {0} from this screen?"],
                                        _currentResolution);

            _frmDelete.Show();
        }

        void HideModal() => _frmDelete.Hide();

        async void ConfirmDelete()
        {
            if(_currentResolution is null)
                return;

            _deleteInProgress = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await ResolutionsByScreenService.DeleteAsync(_currentResolution.Id);
            _screenResolutions = await ResolutionsByScreenService.GetByScreen(Id);

            _deleteInProgress = false;
            _frmDelete.Hide();

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }

        void ModalClosing(ModalClosingEventArgs obj)
        {
            _deleteInProgress  = false;
            _currentResolution = null;
        }

        void OnAddResolutionClick()
        {
            _addingResolution   = true;
            _savingResolution   = false;
            _addingResolutionId = _resolutions.First().Id;
        }

        void CancelAddResolution()
        {
            _addingResolution   = false;
            _savingResolution   = false;
            _addingResolutionId = null;
        }

        async Task ConfirmAddResolution()
        {
            if(_addingResolutionId is null ||
               _addingResolutionId <= 0)
            {
                CancelAddResolution();

                return;
            }

            _savingResolution = true;

            // Yield thread to let UI to update
            await Task.Yield();

            await ResolutionsByScreenService.CreateAsync(_addingResolutionId.Value, Id);
            _screenResolutions = await ResolutionsByScreenService.GetByScreen(Id);

            _addingResolution   = false;
            _savingResolution   = false;
            _addingResolutionId = null;

            // Yield thread to let UI to update
            await Task.Yield();

            // Tell we finished loading
            StateHasChanged();
        }
    }
}