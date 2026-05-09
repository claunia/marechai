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

public partial class MagazineIssues
{
    string                 _errorMessage;
    List<MagazineIssueDto> _issues = [];
    bool                   _loaded;
    string                 _magazineTitle;
    string                 _successMessage;

    [Parameter] public long MagazineId { get; set; }

    protected override async Task OnInitializedAsync() => await LoadAllAsync();

    async Task LoadAllAsync()
    {
        _loaded = false;

        Task<MagazineDto>            magTask    = Service.GetMagazineAsync(MagazineId);
        Task<List<MagazineIssueDto>> issuesTask = Service.GetIssuesByMagazineAsync(MagazineId);

        await Task.WhenAll(magTask, issuesTask);

        _magazineTitle = magTask.Result?.Title ?? L["Magazine not found."];
        _issues        = issuesTask.Result ?? [];
        _loaded        = true;
    }

    async Task ReloadAsync()
    {
        _issues = await Service.GetIssuesByMagazineAsync(MagazineId);
    }

    void GoBack() => NavigationManager.NavigateTo("/admin/magazines");

    static string FormatDate(DateTimeOffset? date, int? precision = 0)
    {
        if(date is null) return "";
        if((precision ?? 0) == 2) return date.Value.Year.ToString();
        if((precision ?? 0) == 1) return date.Value.ToString("MMMM yyyy");
        return date.Value.Date.ToShortDateString();
    }

    async Task OpenAddDialog()
    {
        DialogParameters<MagazineIssueDialog> parameters = new()
        {
            { x => x.MagazineId, MagazineId },
            { x => x.PublishedPrecision, 0 }
        };

        IDialogReference dialog = await DialogService.ShowAsync<MagazineIssueDialog>(L["Add Issue"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth  = MaxWidth.Medium,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: MagazineIssueDialogResult data })
        {
            var dto = new MagazineIssueDto
            {
                MagazineId         = data.MagazineId,
                Caption            = data.Caption,
                NativeCaption      = data.NativeCaption,
                Published          = data.Published.HasValue ? new DateTimeOffset(data.Published.Value, TimeSpan.Zero) : null,
                PublishedPrecision = data.PublishedPrecision,
                ProductCode        = data.ProductCode,
                Pages              = data.Pages.HasValue ? (int?)data.Pages.Value : null,
                IssueNumber        = data.IssueNumber.HasValue ? (int?)data.IssueNumber.Value : null
            };

            (long? id, string errorMessage) = await Service.CreateIssueAsync(dto);

            if(id is not null)
            {
                _successMessage = L["Issue created successfully."];
                await ReloadAsync();
            }
            else
            {
                _errorMessage = errorMessage ?? L["Failed to save the issue."];
            }
        }
    }

    async Task OpenEditDialog(MagazineIssueDto issue)
    {
        MagazineIssueDto full = await Service.GetIssueByIdAsync(issue.Id ?? 0);

        if(full is null)
        {
            _errorMessage = L["Failed to load the issue."];

            return;
        }

        DialogParameters<MagazineIssueDialog> parameters = new()
        {
            { x => x.MagazineId, MagazineId },
            { x => x.Caption, full.Caption },
            { x => x.NativeCaption, full.NativeCaption },
            { x => x.Published, full.Published?.UtcDateTime },
            { x => x.PublishedPrecision, (int)(full.PublishedPrecision ?? 0) },
            { x => x.ProductCode, full.ProductCode },
            { x => x.Pages, full.Pages.HasValue ? (short?)full.Pages.Value : null },
            { x => x.IssueNumber, full.IssueNumber.HasValue && full.IssueNumber.Value >= 0 ? (uint?)full.IssueNumber.Value : null }
        };

        IDialogReference dialog = await DialogService.ShowAsync<MagazineIssueDialog>(L["Edit Issue"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth  = MaxWidth.Medium,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: MagazineIssueDialogResult data })
        {
            var dto = new MagazineIssueDto
            {
                Id                 = full.Id,
                MagazineId         = data.MagazineId,
                Caption            = data.Caption,
                NativeCaption      = data.NativeCaption,
                Published          = data.Published.HasValue ? new DateTimeOffset(data.Published.Value, TimeSpan.Zero) : null,
                PublishedPrecision = data.PublishedPrecision,
                ProductCode        = data.ProductCode,
                Pages              = data.Pages.HasValue ? (int?)data.Pages.Value : null,
                IssueNumber        = data.IssueNumber.HasValue ? (int?)data.IssueNumber.Value : null
            };

            (bool succeeded, string errorMessage) = await Service.UpdateIssueAsync(full.Id ?? 0, dto);

            if(succeeded)
            {
                _successMessage = L["Issue updated successfully."];
                await ReloadAsync();
            }
            else
            {
                _errorMessage = errorMessage ?? L["Failed to save the issue."];
            }
        }
    }

    async Task ConfirmDelete(MagazineIssueDto issue)
    {
        string displayName = issue.IssueNumber.HasValue
                                 ? $"#{issue.IssueNumber.Value} {issue.Caption}".Trim()
                                 : issue.Caption ?? "";

        DialogParameters<DeleteConfirmDialog> parameters = new()
        {
            { x => x.ContentText, string.Format(L["Are you sure you want to delete the issue {0}?"], displayName) }
        };

        IDialogReference dialog = await DialogService.ShowAsync<DeleteConfirmDialog>(L["Delete Issue"], parameters,
                                                                                     new DialogOptions
                                                                                     {
                                                                                         MaxWidth  = MaxWidth.ExtraSmall,
                                                                                         FullWidth = true
                                                                                     });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            (bool succeeded, string errorMessage) = await Service.DeleteIssueAsync(issue.Id ?? 0);

            if(succeeded)
            {
                _successMessage = L["Issue deleted successfully."];
                await ReloadAsync();
            }
            else
            {
                _errorMessage = errorMessage ?? L["Failed to delete the issue."];
            }
        }
    }
}
