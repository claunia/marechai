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
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareCovers
{
    const long MaxFileSize = 50 * 1024 * 1024; // 50 MB

    string                       _caption;
    List<SoftwareCoverDto>       _covers;
    string                       _errorMessage;
    bool                         _isLoading = true;
    bool                         _isUploading;
    List<SoftwareReleaseDto>     _releases;
    IBrowserFile                 _selectedFile;
    SoftwareReleaseDto           _selectedRelease;
    int?                         _selectedType;
    string                       _softwareName;
    string                       _successMessage;

    [Parameter] public int SoftwareId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        SoftwareDto software = await SoftwareService.GetSoftwareByIdAsync(SoftwareId);
        _softwareName = software?.Name;
        _releases     = await SoftwareService.GetReleasesBySoftwareAsync(SoftwareId);
        await LoadDataAsync();
    }

    async Task LoadDataAsync()
    {
        _isLoading = true;
        _covers    = await SoftwareService.GetCoversBySoftwareAsync(SoftwareId);
        _isLoading = false;
    }

    void OnFileSelected(IBrowserFile file) => _selectedFile = file;

    async Task UploadCover()
    {
        if(_selectedFile is null || _selectedRelease is null || _selectedType is null)
            return;

        _isUploading    = true;
        _errorMessage   = null;
        _successMessage = null;

        try
        {
            await using Stream stream = _selectedFile.OpenReadStream(MaxFileSize);
            using var          ms     = new MemoryStream();
            await stream.CopyToAsync(ms);
            byte[] fileBytes = ms.ToArray();

            SoftwareCoverDto result =
                await SoftwareService.UploadCoverAsync((ulong)_selectedRelease.Id!.Value, _selectedType.Value,
                                                       _caption, fileBytes, _selectedFile.Name);

            if(result is not null)
            {
                _successMessage = L["Cover uploaded successfully."];
                _selectedFile   = null;
                _caption        = null;
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = L["Failed to upload cover."];
            }
        }
        catch(Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isUploading = false;
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
