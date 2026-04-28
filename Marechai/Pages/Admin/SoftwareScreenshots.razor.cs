using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareScreenshots
{
    string?                    _errorMessage;
    bool                       _isLoading = true;
    List<SoftwarePlatformDto>? _platforms;
    List<Guid?>?               _screenshotIds;
    SoftwarePlatformDto?       _selectedPlatform;
    string?                    _softwareName;
    string?                    _successMessage;

    [Parameter] public int SoftwareId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        SoftwareDto? software = await SoftwareService.GetSoftwareByIdAsync(SoftwareId);
        _softwareName = software?.Name;
        _platforms    = await SoftwareService.GetPlatformsAsync();
        await LoadDataAsync();
    }

    async Task LoadDataAsync()
    {
        _isLoading     = true;
        _screenshotIds = await SoftwareService.GetScreenshotIdsAsync(SoftwareId);
        _isLoading     = false;
    }

    async Task OnFileSelected(IBrowserFile? file)
    {
        if(file is null) return;

        const int maxSize = 50 * 1024 * 1024;

        try
        {
            await using Stream stream     = file.OpenReadStream(maxSize);
            using var          memStream   = new MemoryStream();
            await stream.CopyToAsync(memStream);
            byte[] fileBytes = memStream.ToArray();

            ulong? platformId = _selectedPlatform?.Id is not null
                                   ? (ulong)_selectedPlatform.Id.Value
                                   : null;

            SoftwareScreenshotDto? result =
                await SoftwareService.UploadScreenshotAsync(SoftwareId, fileBytes, file.Name, platformId);

            if(result is not null)
            {
                _successMessage = L["Screenshot uploaded successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = L["Failed to upload screenshot."];
            }
        }
        catch(Exception ex)
        {
            _errorMessage = ex.Message;
        }
    }

    async Task ConfirmDeleteScreenshot(Guid screenshotId)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                L["Are you sure you want to delete this screenshot? This action cannot be undone."]
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Screenshot"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult? result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string? errorMessage) = await SoftwareService.DeleteScreenshotAsync(screenshotId);

            if(succeeded)
            {
                _successMessage = L["Screenshot deleted successfully."];
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
