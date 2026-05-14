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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwarePromoArt
{
    const long MaxFileSize = 50 * 1024 * 1024; // 50 MB

    string                          _caption;
    string                          _errorMessage;
    SoftwarePromoArtDto             _fullscreenItem;
    string                          _groupName;
    List<SoftwarePromoArtGroupDto>  _groups;
    bool                            _isLoading = true;
    bool                            _isUploading;
    List<SoftwarePromoArtDto>       _promoArt;
    IBrowserFile                    _selectedFile;
    string                          _softwareName;
    string                          _successMessage;

    [Parameter] public int SoftwareId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        SoftwareDto software = await SoftwareService.GetSoftwareByIdAsync(SoftwareId);
        _softwareName = software?.Name;
        _groups       = await SoftwareService.GetPromoArtGroupsAsync();
        await LoadDataAsync();
    }

    async Task LoadDataAsync()
    {
        _isLoading = true;
        _promoArt  = await SoftwareService.GetPromoArtBySoftwareAsync(SoftwareId);
        _isLoading = false;
    }

    Task<IEnumerable<string>> SearchGroups(string value, CancellationToken cancellationToken)
    {
        IEnumerable<string> all = _groups?.Select(g => g.Name).Where(n => !string.IsNullOrWhiteSpace(n)) ??
                                  Enumerable.Empty<string>();

        if(string.IsNullOrWhiteSpace(value)) return Task.FromResult(all);

        return Task.FromResult(all.Where(n => n.Contains(value, StringComparison.OrdinalIgnoreCase)));
    }

    void OnFileSelected(IBrowserFile file) => _selectedFile = file;

    void OpenFullscreen(SoftwarePromoArtDto promo) => _fullscreenItem = promo;

    async Task UploadPromoArt()
    {
        if(_selectedFile is null || string.IsNullOrWhiteSpace(_groupName))
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

            // Map the displayed (localized) group name back to the canonical English name so the
            // server-side get-or-create keys on the same row regardless of the user's locale.
            // Free-text new entries fall through and are uploaded as-is — the worker will
            // translate them on the next tick.
            string canonicalGroupName =
                _groups?.FirstOrDefault(g =>
                                            string.Equals(g.Name, _groupName, StringComparison.OrdinalIgnoreCase))
                       ?.CanonicalName
             ?? _groupName;

            SoftwarePromoArtDto result = await SoftwareService.UploadPromoArtAsync((ulong)SoftwareId, canonicalGroupName,
                                                                                    _caption, fileBytes,
                                                                                    _selectedFile.Name);

            if(result is not null)
            {
                _successMessage = L["Promo art uploaded successfully."];
                _selectedFile   = null;
                _caption        = null;
                _groups         = await SoftwareService.GetPromoArtGroupsAsync();
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = L["Failed to upload promo art."];
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

    async Task OnGroupChanged(SoftwarePromoArtDto promo, string newGroup)
    {
        if(string.IsNullOrWhiteSpace(newGroup) || promo.GroupName == newGroup) return;

        // Map the picked (localized) name back to canonical English (see UploadPromoArt).
        string canonicalGroupName =
            _groups?.FirstOrDefault(g => string.Equals(g.Name, newGroup, StringComparison.OrdinalIgnoreCase))
                   ?.CanonicalName
         ?? newGroup;

        var dto = new UpdateSoftwarePromoArtRequest
        {
            GroupName = canonicalGroupName,
            Caption   = promo.Caption
        };

        (bool succeeded, string error) = await SoftwareService.UpdatePromoArtAsync(promo.Id!.Value, dto);

        if(succeeded)
        {
            promo.GroupName = newGroup;
            _groups         = await SoftwareService.GetPromoArtGroupsAsync();
            await LoadDataAsync();
        }
        else
        {
            _errorMessage = error;
        }
    }

    async Task OnCaptionChanged(SoftwarePromoArtDto promo, string newCaption)
    {
        if(promo.Caption == newCaption) return;

        // promo.GroupName is the LOCALIZED display name returned by the server. Map it back to
        // canonical English before submitting so the get-or-create path keys on the existing row.
        string canonicalGroupName =
            _groups?.FirstOrDefault(g => string.Equals(g.Name, promo.GroupName, StringComparison.OrdinalIgnoreCase))
                   ?.CanonicalName
         ?? promo.GroupName;

        var dto = new UpdateSoftwarePromoArtRequest
        {
            GroupName = canonicalGroupName,
            Caption   = newCaption
        };

        (bool succeeded, string error) = await SoftwareService.UpdatePromoArtAsync(promo.Id!.Value, dto);

        if(succeeded)
            promo.Caption = newCaption;
        else
            _errorMessage = error;
    }

    async Task ConfirmDeletePromoArt(SoftwarePromoArtDto promo)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            {
                x => x.ContentText,
                L["Are you sure you want to delete this promo art? This action cannot be undone."]
            }
        };

        IDialogReference dialog =
            await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Promo Art"], parameters,
                                                               new DialogOptions
                                                               {
                                                                   MaxWidth  = MaxWidth.ExtraSmall,
                                                                   FullWidth = true
                                                               });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) =
                await SoftwareService.DeletePromoArtAsync(promo.Id ?? Guid.Empty);

            if(succeeded)
            {
                _successMessage = L["Promo art deleted successfully."];
                _groups         = await SoftwareService.GetPromoArtGroupsAsync();
                await LoadDataAsync();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
