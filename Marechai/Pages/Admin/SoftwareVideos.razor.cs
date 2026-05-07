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

using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareVideos
{
    string                   _detectedVideoId;
    string                   _errorMessage;
    bool                     _isLoading = true;
    bool                     _isSaving;
    string                   _manualVideoId;
    string                   _provider = "YouTube";
    string                   _softwareName;
    string                   _successMessage;
    string                   _title;
    string                   _urlInput;
    List<SoftwareVideoDto>   _videos;

    [Parameter] public int SoftwareId { get; set; }

    bool CanLink => !string.IsNullOrWhiteSpace(EffectiveProvider) && !string.IsNullOrWhiteSpace(EffectiveVideoId);

    string EffectiveProvider => string.IsNullOrWhiteSpace(_manualVideoId)
                                    ? "YouTube"
                                    : (_provider ?? string.Empty).Trim();

    string EffectiveVideoId => !string.IsNullOrWhiteSpace(_manualVideoId)
                                   ? _manualVideoId.Trim()
                                   : _detectedVideoId;

    protected override async Task OnInitializedAsync()
    {
        SoftwareDto software = await SoftwareService.GetSoftwareByIdAsync(SoftwareId);
        _softwareName = software?.Name;
        await LoadDataAsync();
    }

    async Task LoadDataAsync()
    {
        _isLoading = true;
        _videos    = await SoftwareService.GetVideosBySoftwareAsync(SoftwareId);
        _isLoading = false;
    }

    void OnUrlInputChanged()
    {
        _detectedVideoId = YouTubeUrlParser.TryExtract(_urlInput, out string id) ? id : null;
    }

    async Task LinkVideoAsync()
    {
        _errorMessage   = null;
        _successMessage = null;

        if(!CanLink) return;

        _isSaving = true;

        try
        {
            (SoftwareVideoDto dto, string error) = await SoftwareService.CreateVideoAsync(SoftwareId,
                EffectiveProvider, EffectiveVideoId, string.IsNullOrWhiteSpace(_title) ? null : _title.Trim());

            if(dto is not null)
            {
                _successMessage  = L["Video linked successfully."];
                _urlInput        = null;
                _detectedVideoId = null;
                _manualVideoId   = null;
                _title           = null;
                _provider        = "YouTube";
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = string.IsNullOrWhiteSpace(error) ? L["Failed to link video."] : error;
            }
        }
        finally
        {
            _isSaving = false;
        }
    }

    async Task OnTitleChangedAsync(SoftwareVideoDto video, string newTitle)
    {
        if(video.Title == newTitle) return;

        (bool succeeded, string error) =
            await SoftwareService.UpdateVideoTitleAsync(video.Id!.Value, newTitle);

        if(succeeded)
        {
            video.Title     = newTitle;
            _successMessage = L["Video updated successfully."];
        }
        else
        {
            _errorMessage = string.IsNullOrWhiteSpace(error) ? L["Failed to update video."] : error;
        }
    }

    async Task ConfirmDeleteAsync(SoftwareVideoDto video)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                L["Are you sure you want to delete this video? This action cannot be undone."]
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Video"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is not { Canceled: false }) return;

        (bool succeeded, string errorMessage) =
            await SoftwareService.DeleteVideoAsync(video.Id ?? 0);

        if(succeeded)
        {
            _successMessage = L["Video deleted successfully."];
            await LoadDataAsync();
        }
        else
        {
            _errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? L["Failed to delete video."] : errorMessage;
        }
    }
}
