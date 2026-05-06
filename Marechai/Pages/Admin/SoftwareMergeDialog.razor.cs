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
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareMergeDialog
{
    SoftwareMergePreviewDto _preview;
    SoftwareDto             _selectedTarget;
    string                  _releaseTitle;
    string                  _errorMessage;
    bool                    _isLoadingPreview;
    bool                    _isMerging;
    SoftwareDto             _previousTarget;

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public int    SourceId   { get; set; }
    [Parameter] public string SourceName { get; set; } = string.Empty;

    [Inject] SoftwareService SoftwareService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Check if target selection changed and load preview
        if(_selectedTarget is not null && _selectedTarget != _previousTarget && _selectedTarget.Id != SourceId)
        {
            _previousTarget = _selectedTarget;
            await LoadPreview();
            StateHasChanged();
        }
        else if(_selectedTarget is null && _previousTarget is not null)
        {
            _previousTarget = null;
            _preview        = null;
            _errorMessage   = null;
            _releaseTitle   = null;
            StateHasChanged();
        }
    }

    async Task<IEnumerable<SoftwareDto>> SearchSoftware(string value, CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(value)) return [];

        List<SoftwareDto> results = await SoftwareService.GetPagedAsync(0, 20, value);

        // Exclude the source software from results
        return results?.Where(s => s.Id != SourceId) ?? [];
    }

    async Task LoadPreview()
    {
        _isLoadingPreview = true;
        _errorMessage     = null;
        _preview          = null;

        _preview = await SoftwareService.GetMergePreviewAsync((int)(_selectedTarget.Id ?? 0), SourceId);

        if(_preview is null)
            _errorMessage = L["Failed to load merge preview. Please try again."];
        else
            _releaseTitle = _preview.SuggestedReleaseTitle;

        _isLoadingPreview = false;
    }

    async Task ConfirmMerge()
    {
        if(_preview is null || _selectedTarget is null) return;

        _isMerging    = true;
        _errorMessage = null;

        (bool succeeded, string error) =
            await SoftwareService.MergeSoftwareAsync((int)(_selectedTarget.Id ?? 0), SourceId, _releaseTitle);

        _isMerging = false;

        if(succeeded)
            MudDialog.Close(DialogResult.Ok(true));
        else
            _errorMessage = string.Format(L["Merge failed: {0}"], error);
    }

    void Cancel() => MudDialog.Cancel();
}
