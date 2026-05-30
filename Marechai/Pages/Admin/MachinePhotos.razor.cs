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
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class MachinePhotos
{
    string                _errorMessage;
    bool                  _isLoading = true;
    List<LicenseDto>      _licenses;
    string                _machineName;
    List<MachinePhotoDto> _photos;
    string                _successMessage;

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

    async Task OpenUploadDialog()
    {
        _errorMessage   = null;
        _successMessage = null;

        var parameters = new DialogParameters<MachinePhotosBatchUploadDialog>
        {
            { x => x.MachineId,   MachineId },
            { x => x.MachineName, _machineName ?? string.Empty },
            { x => x.Licenses,    _licenses ?? new List<LicenseDto>() }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<MachinePhotosBatchUploadDialog>(L["Upload Photos"], parameters,
                                                                          new DialogOptions
                                                                          {
                                                                              MaxWidth         = MaxWidth.Large,
                                                                              FullWidth        = true,
                                                                              CloseOnEscapeKey = false,
                                                                              BackdropClick    = false
                                                                          });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            int succeeded = result.Data is int n ? n : 0;
            if(succeeded > 0)
                _successMessage = string.Format(L["{0} photo(s) uploaded successfully."].Value, succeeded);

            await LoadPhotosAsync();
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
