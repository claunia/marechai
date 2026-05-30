using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareScreenshots
{
    string                        _errorMessage;
    bool                           _isLoading = true;
    List<SoftwarePlatformDto>     _platforms;
    List<SoftwareScreenshotDto>   _screenshots;
    string                        _softwareName;
    string                        _successMessage;

    [Parameter] public int SoftwareId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        SoftwareDto software = await SoftwareService.GetSoftwareByIdAsync(SoftwareId);
        _softwareName = software?.Name;
        _platforms    = await SoftwareService.GetPlatformsAsync();
        await LoadDataAsync();
    }

    async Task LoadDataAsync()
    {
        _isLoading   = true;
        _screenshots = await SoftwareService.GetScreenshotsBySoftwareAsync(SoftwareId);
        _isLoading   = false;
    }

    async Task OpenUploadDialog()
    {
        _errorMessage   = null;
        _successMessage = null;

        var parameters = new DialogParameters<SoftwareScreenshotsBatchUploadDialog>
        {
            { x => x.SoftwareId,   SoftwareId },
            { x => x.SoftwareName, _softwareName ?? string.Empty },
            { x => x.Platforms,    _platforms ?? new List<SoftwarePlatformDto>() }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<SoftwareScreenshotsBatchUploadDialog>(L["Upload Screenshots"], parameters,
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
                _successMessage = string.Format(L["{0} screenshot(s) uploaded successfully."].Value, succeeded);

            await LoadDataAsync();
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

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await SoftwareService.DeleteScreenshotAsync(screenshotId);

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

    async Task OnScreenshotPlatformChanged(SoftwareScreenshotDto screenshot, SoftwarePlatformDto platform)
    {
        int? newPlatformId = platform?.Id;

        if(screenshot.SoftwarePlatformId == newPlatformId) return;

        screenshot.SoftwarePlatformId = newPlatformId;
        screenshot.PlatformName       = platform?.Name;

        // Preserve the existing canonical group on the wire DTO so the server's resolve-or-create
        // doesn't clear it when we only meant to change the platform.
        var dto = new SoftwareScreenshotDto
        {
            Id                 = screenshot.Id,
            SoftwareId         = screenshot.SoftwareId,
            SoftwarePlatformId = newPlatformId,
            SoftwareVersionId  = screenshot.SoftwareVersionId,
            Caption            = screenshot.Caption,
            CanonicalCaption   = screenshot.CanonicalCaption ?? screenshot.Caption,
            CanonicalGroupName = screenshot.CanonicalGroupName,
            OriginalExtension  = screenshot.OriginalExtension
        };

        (bool succeeded, string error) = await SoftwareService.UpdateScreenshotAsync(screenshot.Id!.Value, dto);

        if(!succeeded)
            _errorMessage = error;
    }

    async Task OnScreenshotGroupChanged(SoftwareScreenshotDto screenshot, string canonicalGroupName)
    {
        string trimmed = canonicalGroupName?.Trim();

        // Treat "" identically to null — clearing the autocomplete drops the FK.
        if(string.IsNullOrWhiteSpace(trimmed)) trimmed = null;

        if(string.Equals(screenshot.CanonicalGroupName ?? string.Empty, trimmed ?? string.Empty,
                         StringComparison.Ordinal))
            return;

        screenshot.CanonicalGroupName = trimmed;
        screenshot.GroupName          = trimmed;

        var dto = new SoftwareScreenshotDto
        {
            Id                 = screenshot.Id,
            SoftwareId         = screenshot.SoftwareId,
            SoftwarePlatformId = screenshot.SoftwarePlatformId,
            SoftwareVersionId  = screenshot.SoftwareVersionId,
            Caption            = screenshot.Caption,
            CanonicalCaption   = screenshot.CanonicalCaption ?? screenshot.Caption,
            CanonicalGroupName = trimmed,
            OriginalExtension  = screenshot.OriginalExtension
        };

        (bool succeeded, string error) = await SoftwareService.UpdateScreenshotAsync(screenshot.Id!.Value, dto);

        if(!succeeded)
            _errorMessage = error;
    }

    /// <summary>
    ///     Backing search delegate for the group <c>MudAutocomplete</c>. Returns the canonical
    ///     English names matching <paramref name="search" /> so the autocomplete displays the
    ///     same string it submits back to the server's resolve-or-create endpoint. Server-side
    ///     filtering keeps the wire payload bounded (top-25) and avoids holding the full group
    ///     catalog in memory client-side.
    /// </summary>
    async Task<IEnumerable<string>> SearchGroupsAsync(string search, CancellationToken cancellationToken)
    {
        List<SoftwareScreenshotGroupDto> groups = await SoftwareService.GetScreenshotGroupsAsync(search);

        return groups.Select(g => g.CanonicalName)
                     .Where(n => !string.IsNullOrWhiteSpace(n))
                     .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
