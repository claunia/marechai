/*******************************************************************************
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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class SoftwareAttributes
{
    string                            _errorMessage;
    string                            _successMessage;
    MudDataGrid<SoftwareAttributeDto> _dataGrid;

    SoftwareDto                      _filterSoftware;
    SoftwareReleaseLookupDto         _filterRelease;
    string                           _filterCategory;
    string                           _filterKey;
    List<SoftwareReleaseLookupDto>   _releasesForCurrentSoftware;

    static string ReleaseToString(SoftwareReleaseLookupDto r)
    {
        if(r is null) return null;

        string title = string.IsNullOrWhiteSpace(r.Title) ? "—" : r.Title;

        return string.IsNullOrWhiteSpace(r.PlatformName) ? title : $"{title} — {r.PlatformName}";
    }

    async Task<GridData<SoftwareAttributeDto>> ServerReload(GridState<SoftwareAttributeDto> state,
                                                            CancellationToken               cancellationToken)
    {
        int page     = state.Page + 1;
        int pageSize = state.PageSize;

        SoftwareAttributePageDto result = await SoftwareAttributesService.GetPagedAsync(
            (int?)_filterSoftware?.Id, (int?)_filterRelease?.Id, _filterCategory, _filterKey, page, pageSize);

        return new GridData<SoftwareAttributeDto>
        {
            Items      = result?.Items ?? new List<SoftwareAttributeDto>(),
            TotalItems = result?.TotalCount ?? 0
        };
    }

    Task<IEnumerable<SoftwareDto>> SearchSoftwareAsync(string text, CancellationToken cancellationToken) =>
        SearchSoftwareAsync(text);

    async Task<IEnumerable<SoftwareDto>> SearchSoftwareAsync(string text)
    {
        List<SoftwareDto> results = await SoftwareService.SearchSoftwareAsync(text ?? string.Empty);

        return results;
    }

    Task<IEnumerable<SoftwareReleaseLookupDto>> SearchReleasesAsync(string text, CancellationToken cancellationToken) =>
        SearchReleasesAsync(text);

    async Task<IEnumerable<SoftwareReleaseLookupDto>> SearchReleasesAsync(string text)
    {
        if(_filterSoftware?.Id is null) return [];

        if(_releasesForCurrentSoftware is null)
        {
            _releasesForCurrentSoftware =
                await SoftwareAttributesService.LookupReleasesAsync(_filterSoftware.Id ?? 0);
        }

        if(string.IsNullOrWhiteSpace(text)) return _releasesForCurrentSoftware;

        return _releasesForCurrentSoftware
              .Where(r => (r.Title ?? string.Empty).Contains(text, System.StringComparison.OrdinalIgnoreCase) ||
                          (r.PlatformName ?? string.Empty).Contains(text, System.StringComparison.OrdinalIgnoreCase));
    }

    Task<IEnumerable<string>> SearchCategoriesAsync(string text, CancellationToken cancellationToken) =>
        SearchCategoriesAsync(text);

    async Task<IEnumerable<string>> SearchCategoriesAsync(string text)
    {
        List<string> all = await SoftwareAttributesService.GetDistinctCategoriesAsync();

        if(string.IsNullOrWhiteSpace(text)) return all;

        return all.Where(c => c.Contains(text, System.StringComparison.OrdinalIgnoreCase));
    }

    Task<IEnumerable<string>> SearchKeysAsync(string text, CancellationToken cancellationToken) =>
        SearchKeysAsync(text);

    async Task<IEnumerable<string>> SearchKeysAsync(string text)
    {
        List<string> all = await SoftwareAttributesService.GetDistinctKeysAsync(_filterCategory);

        if(string.IsNullOrWhiteSpace(text)) return all;

        return all.Where(k => k.Contains(text, System.StringComparison.OrdinalIgnoreCase));
    }

    async Task OnFilterSoftwareChanged(SoftwareDto software)
    {
        _filterSoftware             = software;
        _filterRelease              = null;
        _releasesForCurrentSoftware = null;
        await _dataGrid.ReloadServerData();
    }

    async Task OnFilterReleaseChanged(SoftwareReleaseLookupDto release)
    {
        _filterRelease = release;
        await _dataGrid.ReloadServerData();
    }

    async Task OnFilterCategoryChanged(string category)
    {
        _filterCategory = category;
        await _dataGrid.ReloadServerData();
    }

    async Task OnFilterKeyChanged(string key)
    {
        _filterKey = key;
        await _dataGrid.ReloadServerData();
    }

    async Task ClearFilters()
    {
        _filterSoftware             = null;
        _filterRelease              = null;
        _filterCategory             = null;
        _filterKey                  = null;
        _releasesForCurrentSoftware = null;
        await _dataGrid.ReloadServerData();
    }

    async Task OpenAddDialog()
    {
        DialogParameters<SoftwareAttributeDialog> parameters = new()
        {
            { x => x.IsNew, true },
            { x => x.SoftwareReleaseId, (ulong)(_filterRelease?.Id ?? 0) },
            { x => x.SoftwareName, _filterSoftware?.Name },
            { x => x.SoftwareReleaseTitle, _filterRelease?.Title },
            { x => x.PlatformName, _filterRelease?.PlatformName },
            { x => x.Category, _filterCategory ?? string.Empty },
            { x => x.Key, _filterKey ?? string.Empty },
            { x => x.Value, string.Empty }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareAttributeDialog>(L["Add Attribute"], parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareAttributeDialogResult data })
        {
            if(data.SplitApplied)
            {
                _successMessage = data.Summary;
                await _dataGrid.ReloadServerData();

                return;
            }

            var request = new CreateSoftwareAttributeRequest
            {
                SoftwareReleaseId = (int)data.SoftwareReleaseId,
                Category          = data.Category,
                Key               = data.Key,
                Value             = data.Value
            };

            (long? id, string errorMessage) = await SoftwareAttributesService.CreateAsync(request);

            if(id is not null)
            {
                _successMessage = L["Attribute created successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task OpenEditDialog(SoftwareAttributeDto attribute)
    {
        DialogParameters<SoftwareAttributeDialog> parameters = new()
        {
            { x => x.IsNew, false },
            { x => x.AttributeId, attribute.Id ?? 0 },
            { x => x.SoftwareReleaseId, (ulong)(attribute.SoftwareReleaseId ?? 0) },
            { x => x.SoftwareName, attribute.SoftwareName },
            { x => x.SoftwareReleaseTitle, attribute.SoftwareReleaseTitle },
            { x => x.PlatformName, attribute.PlatformName },
            { x => x.Category, attribute.Category ?? string.Empty },
            { x => x.Key, attribute.Key ?? string.Empty },
            { x => x.Value, attribute.Value ?? string.Empty }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareAttributeDialog>(L["Edit Attribute"],
            parameters, new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: SoftwareAttributeDialogResult data })
        {
            if(data.SplitApplied)
            {
                _successMessage = data.Summary;
                await _dataGrid.ReloadServerData();

                return;
            }

            var request = new UpdateSoftwareAttributeRequest
            {
                Category = data.Category,
                Key      = data.Key,
                Value    = data.Value
            };

            (bool succeeded, string errorMessage) =
                await SoftwareAttributesService.UpdateAsync(attribute.Id ?? 0, request);

            if(succeeded)
            {
                _successMessage = L["Attribute updated successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }

    async Task ConfirmDelete(SoftwareAttributeDto attribute)
    {
        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            { x => x.ContentText, L["Are you sure you want to delete this attribute? This action cannot be undone."] }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Attribute"], parameters,
            new DialogOptions { MaxWidth = MaxWidth.ExtraSmall, FullWidth = true });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await SoftwareAttributesService.DeleteAsync(attribute.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Attribute deleted successfully."];
                await _dataGrid.ReloadServerData();
            }
            else
            {
                _errorMessage = errorMessage;
            }
        }
    }
}
