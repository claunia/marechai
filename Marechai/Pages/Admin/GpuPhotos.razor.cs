using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class GpuPhotos
{
    string               _errorMessage;
    bool                  _isLoading = true;
    List<LicenseDto>     _licenses;
    string               _gpuName;
    List<GpuPhotoDto>    _photos;
    string               _successMessage;

    [Parameter] public int GpuId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        GpuDto gpu = await GpusService.GetByIdAsync(GpuId);
        _gpuName  = gpu?.Name;
        _licenses = await GpuPhotosService.GetAllLicensesAsync();

        await LoadPhotosAsync();
    }

    async Task LoadPhotosAsync()
    {
        _isLoading = true;

        List<Guid> guids = await GpuPhotosService.GetGuidsByGpuAsync(GpuId);

        var photos = new List<GpuPhotoDto>();

        foreach(Guid guid in guids)
        {
            GpuPhotoDto photo = await GpuPhotosService.GetAsync(guid);

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

        var parameters = new DialogParameters<GpuPhotosBatchUploadDialog>
        {
            { x => x.GpuId,    GpuId },
            { x => x.GpuName,  _gpuName ?? string.Empty },
            { x => x.Licenses, _licenses ?? new List<LicenseDto>() }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<GpuPhotosBatchUploadDialog>(L["Upload Photos"], parameters,
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

    async Task ConfirmDeletePhoto(GpuPhotoDto photo)
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
            (bool succeeded, string errorMessage) = await GpuPhotosService.DeletePhotoAsync(photo.Id ?? Guid.Empty);

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
