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
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareCovers
{
    List<SoftwareCoverDto>       _covers;
    string                       _errorMessage;
    bool                         _isLoading = true;
    List<SoftwareReleaseDto>     _releases;
    SoftwareReleaseDto           _selectedRelease;
    string                       _softwareName;
    string                       _successMessage;

    [Parameter] public int SoftwareId { get; set; }
    [Parameter] public int? ReleaseId { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        SoftwareDto software = await SoftwareService.GetSoftwareByIdAsync(SoftwareId);
        _softwareName = software?.Name;
        _releases     = await SoftwareService.GetReleasesBySoftwareAsync(SoftwareId);

        if(ReleaseId is > 0)
        {
            SoftwareReleaseDto release = await SoftwareReleasesService.GetByIdAsync(ReleaseId.Value);

            if(release?.SoftwareId == SoftwareId)
                _selectedRelease = release;
            else
                _selectedRelease = null;
        }
        else
        {
            _selectedRelease = null;
        }

        await LoadDataAsync();
    }

    async Task LoadDataAsync()
    {
        _isLoading = true;
        _covers    = await SoftwareService.GetCoversBySoftwareAsync(SoftwareId);

        if(_selectedRelease?.Id is not null)
            _covers = _covers.Where(c => c.SoftwareReleaseId == _selectedRelease.Id).ToList();

        _isLoading = false;
    }

    async Task OpenBatchUploadDialog()
    {
        if(_selectedRelease?.Id is null) return;

        DialogParameters<SoftwareCoversBatchUploadDialog> parameters = new()
        {
            { x => x.SoftwareReleaseId, (ulong)_selectedRelease.Id.Value },
            { x => x.ReleaseTitle,      FormatReleaseLabel(_selectedRelease) }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<SoftwareCoversBatchUploadDialog>(L["Upload Covers"], parameters,
                                                                            new DialogOptions
                                                                            {
                                                                                MaxWidth      = MaxWidth.Large,
                                                                                FullWidth     = true,
                                                                                CloseOnEscapeKey = false,
                                                                                BackdropClick = false
                                                                            });

        DialogResult result = await dialog.Result;
        if(result is { Canceled: false })
        {
            int uploaded = result.Data is int i ? i : 0;
            if(uploaded > 0)
                _successMessage = string.Format(L["Uploaded {0} cover(s) successfully."].Value, uploaded);
            await LoadDataAsync();
        }
    }

    async Task OnCoverReleaseChanged(SoftwareCoverDto cover, SoftwareReleaseDto release)
    {
        int? newReleaseId = release?.Id;

        if(cover.SoftwareReleaseId == newReleaseId) return;

        var dto = new SoftwareCoverDto
        {
            Id                = cover.Id,
            SoftwareReleaseId = newReleaseId,
            Type              = cover.Type,
            CanonicalCaption  = cover.CanonicalCaption,
            Caption           = cover.CanonicalCaption,
            OriginalExtension = cover.OriginalExtension
        };

        (bool succeeded, string error) = await SoftwareService.UpdateCoverAsync(cover.Id!.Value, dto);

        if(succeeded)
        {
            cover.SoftwareReleaseId = newReleaseId;
            cover.ReleaseTitle      = release?.Title;
            cover.PlatformName      = release?.Platform;
        }
        else
        {
            _errorMessage = error;
        }
    }

    async Task OnCoverTypeChanged(SoftwareCoverDto cover, int? newType)
    {
        if(cover.Type == newType) return;

        var dto = new SoftwareCoverDto
        {
            Id                = cover.Id,
            SoftwareReleaseId = cover.SoftwareReleaseId,
            Type              = newType,
            CanonicalCaption  = cover.CanonicalCaption,
            Caption           = cover.CanonicalCaption,
            OriginalExtension = cover.OriginalExtension
        };

        (bool succeeded, string error) = await SoftwareService.UpdateCoverAsync(cover.Id!.Value, dto);

        if(succeeded)
        {
            cover.Type     = newType;
            cover.TypeName = GetTypeName(newType);
        }
        else
        {
            _errorMessage = error;
        }
    }

    async Task OnCaptionChanged(SoftwareCoverDto cover, string newCaption)
    {
        // The text field is bound to CanonicalCaption (English source-of-truth). The server
        // PUT handler reads dto.CanonicalCaption first, then falls back to dto.Caption.
        if(cover.CanonicalCaption == newCaption) return;

        var dto = new SoftwareCoverDto
        {
            Id                = cover.Id,
            SoftwareReleaseId = cover.SoftwareReleaseId,
            Type              = cover.Type,
            CanonicalCaption  = newCaption,
            Caption           = newCaption,
            OriginalExtension = cover.OriginalExtension
        };

        (bool succeeded, string error) = await SoftwareService.UpdateCoverAsync(cover.Id!.Value, dto);

        if(succeeded)
        {
            cover.CanonicalCaption = newCaption;
            // Reflect the new canonical value in the localized field too until the worker
            // produces a translated row on its next sweep.
            cover.Caption = newCaption;
        }
        else
            _errorMessage = error;
    }

    void GoBack()
    {
        if(_selectedRelease?.SoftwareVersionId is > 0)
            NavigationManager.NavigateTo($"/admin/software/versions/{_selectedRelease.SoftwareVersionId}/releases");
        else
            NavigationManager.NavigateTo($"/admin/software/{SoftwareId}/releases");
    }

    async Task ConfirmDeleteCover(SoftwareCoverDto cover)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                L["Are you sure you want to delete this cover? This action cannot be undone."]
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Cover"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await SoftwareService.DeleteCoverAsync(cover.Id ?? Guid.Empty);

            if(succeeded)
            {
                _successMessage = L["Cover deleted successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    static string FormatReleaseLabel(SoftwareReleaseDto release)
    {
        if(release is null)
            return string.Empty;

        List<string> parts = [];

        if(!string.IsNullOrWhiteSpace(release.Title))
            parts.Add(release.Title);

        if(!string.IsNullOrWhiteSpace(release.SoftwareVersion))
            parts.Add(release.SoftwareVersion);

        if(!string.IsNullOrWhiteSpace(release.Platform))
            parts.Add(release.Platform);

        if(!string.IsNullOrWhiteSpace(release.Publisher))
            parts.Add(release.Publisher);

        if(release.ReleaseDate is not null)
            parts.Add(release.ReleaseDate.Value.ToString("yyyy-MM-dd"));

        return string.Join(" • ", parts);
    }

    static string GetTypeName(int? type) => type switch
    {
        0 => "Front",
        1 => "Back",
        2 => "InsideFront",
        3 => "InsideBack",
        4 => "Media",
        5 => "Spine",
        6 => "Manual",
        7 => "Other",
        _ => "Unknown"
    };
}
