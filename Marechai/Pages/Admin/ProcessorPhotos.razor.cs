using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class ProcessorPhotos
{
    string                    _errorMessage;
    bool                       _isLoading = true;
    List<LicenseDto>          _licenses;
    string                    _processorName;
    List<ProcessorPhotoDto>   _photos;
    string                    _successMessage;

    [Parameter] public int ProcessorId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ProcessorDto processor = await ProcessorsService.GetByIdAsync(ProcessorId);
        _processorName = processor?.Name;
        _licenses      = await ProcessorPhotosService.GetAllLicensesAsync();

        await LoadPhotosAsync();
    }

    async Task LoadPhotosAsync()
    {
        _isLoading = true;

        List<Guid> guids = await ProcessorPhotosService.GetGuidsByProcessorAsync(ProcessorId);

        var photos = new List<ProcessorPhotoDto>();

        foreach(Guid guid in guids)
        {
            ProcessorPhotoDto photo = await ProcessorPhotosService.GetAsync(guid);

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

        var parameters = new DialogParameters<ProcessorPhotosBatchUploadDialog>
        {
            { x => x.ProcessorId,   ProcessorId },
            { x => x.ProcessorName, _processorName ?? string.Empty },
            { x => x.Licenses,      _licenses ?? new List<LicenseDto>() }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<ProcessorPhotosBatchUploadDialog>(L["Upload Photos"], parameters,
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

    async Task ConfirmDeletePhoto(ProcessorPhotoDto photo)
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
                await ProcessorPhotosService.DeletePhotoAsync(photo.Id ?? Guid.Empty);

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
