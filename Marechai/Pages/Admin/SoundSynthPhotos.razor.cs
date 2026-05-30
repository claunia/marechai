using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoundSynthPhotos
{
    string                    _errorMessage;
    bool                      _isLoading = true;
    List<LicenseDto>          _licenses;
    string                    _soundSynthName;
    List<SoundSynthPhotoDto>  _photos;
    string                    _successMessage;

    [Parameter] public int SoundSynthId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        SoundSynthDto synth = await SoundSynthsService.GetByIdAsync(SoundSynthId);
        _soundSynthName = synth?.Name;
        _licenses       = await SoundSynthPhotosService.GetAllLicensesAsync();

        await LoadPhotosAsync();
    }

    async Task LoadPhotosAsync()
    {
        _isLoading = true;

        List<Guid> guids = await SoundSynthPhotosService.GetGuidsBySoundSynthAsync(SoundSynthId);

        var photos = new List<SoundSynthPhotoDto>();

        foreach(Guid guid in guids)
        {
            SoundSynthPhotoDto photo = await SoundSynthPhotosService.GetAsync(guid);

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

        var parameters = new DialogParameters<SoundSynthPhotosBatchUploadDialog>
        {
            { x => x.SoundSynthId,   SoundSynthId },
            { x => x.SoundSynthName, _soundSynthName ?? string.Empty },
            { x => x.Licenses,       _licenses ?? new List<LicenseDto>() }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<SoundSynthPhotosBatchUploadDialog>(L["Upload Photos"], parameters,
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

    async Task ConfirmDeletePhoto(SoundSynthPhotoDto photo)
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
            (bool succeeded, string errorMessage) =
                await SoundSynthPhotosService.DeletePhotoAsync(photo.Id ?? Guid.Empty);

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
