using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class GpuPhotos
{
    const long MaxFileSize = 50 * 1024 * 1024; // 50 MB

    string?               _errorMessage;
    bool                  _isLoading = true;
    bool                  _isUploading;
    List<LicenseDto>?     _licenses;
    string?               _gpuName;
    List<GpuPhotoDto>?    _photos;
    IBrowserFile?         _selectedFile;
    LicenseDto?           _selectedLicense;
    string?               _sourceUrl;
    string?               _successMessage;

    [Parameter] public int GpuId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        GpuDto? gpu = await GpusService.GetByIdAsync(GpuId);
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
            GpuPhotoDto? photo = await GpuPhotosService.GetAsync(guid);

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

            (GpuPhotoDto? photo, string? error) =
                await GpuPhotosService.UploadPhotoAsync(GpuId, _selectedLicense.Id ?? 0, _sourceUrl, fileBytes,
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

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await GpuPhotosService.DeletePhotoAsync(photo.Id ?? Guid.Empty);

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
