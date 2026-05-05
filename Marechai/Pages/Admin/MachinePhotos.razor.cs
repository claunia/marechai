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

public partial class MachinePhotos
{
    const long MaxFileSize = 50 * 1024 * 1024; // 50 MB

    string               _errorMessage;
    bool                  _isLoading = true;
    bool                  _isUploading;
    List<LicenseDto>     _licenses;
    string               _machineName;
    List<MachinePhotoDto> _photos;
    IBrowserFile         _selectedFile;
    LicenseDto           _selectedLicense;
    string               _sourceUrl;
    string               _successMessage;

    [Parameter] public int MachineId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        MachineDto machine = await MachinesService.GetByIdAsync(MachineId);
        _machineName = machine?.Name;
        _licenses    = await MachinePhotosService.GetAllLicensesAsync();

        await LoadPhotosAsync();
    }

    async Task LoadPhotosAsync()
    {
        _isLoading = true;

        List<Guid> guids = await MachinePhotosService.GetGuidsByMachineAsync(MachineId);

        var photos = new List<MachinePhotoDto>();

        foreach(Guid guid in guids)
        {
            MachinePhotoDto photo = await MachinePhotosService.GetAsync(guid);

            if(photo is not null)
                photos.Add(photo);
        }

        _photos    = photos;
        _isLoading = false;
    }

    void OnFileSelected(IBrowserFile file) => _selectedFile = file;

    async Task UploadPhoto()
    {
        if(_selectedFile is null || _selectedLicense is null)
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

            (MachinePhotoDto photo, string error) =
                await MachinePhotosService.UploadPhotoAsync(MachineId, _selectedLicense.Id ?? 0, _sourceUrl, fileBytes,
                                                            _selectedFile.Name);

            if(photo is not null)
            {
                _successMessage = L["Photo uploaded successfully."];
                _selectedFile   = null;
                _sourceUrl      = null;
                await LoadPhotosAsync();
            }
            else
            {
                _errorMessage = error ?? L["Failed to upload photo."];
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

    async Task ConfirmDeletePhoto(MachinePhotoDto photo)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                L["Are you sure you want to delete this photo? This action cannot be undone."]
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Photo"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await MachinePhotosService.DeletePhotoAsync(photo.Id ?? Guid.Empty);

            if(succeeded)
            {
                _successMessage = L["Photo deleted successfully."];
                await LoadPhotosAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
