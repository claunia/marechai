using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using MudBlazor;

namespace Marechai.Pages.Admin;

/// <summary>
/// Code-behind for /admin/software/duplicates.
/// </summary>
/// <remarks>
/// Pagination is over <em>groups</em>, not individual software rows — the server
/// returns one <see cref="SoftwareDuplicateGroupDto"/> per page slot, each
/// already populated with the full child item list.
/// </remarks>
public partial class SoftwareDuplicates
{
    /// <summary>Optional kind filter. When set, <see cref="_excludeDlc"/> is ignored.</summary>
    SoftwareKind? _kindFilter;

    /// <summary>When true (and <see cref="_kindFilter"/> is null), DLC/Addon entries are excluded.</summary>
    bool _excludeDlc;

    string _errorMessage;
    string _successMessage;

    MudDataGrid<SoftwareDuplicateGroupDto> _dataGrid;

    async Task<GridData<SoftwareDuplicateGroupDto>> ServerReload(
        GridState<SoftwareDuplicateGroupDto> state,
        CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        Task<int> countTask =
            SoftwareService.GetAdminDuplicateGroupsCountAsync(_kindFilter, _excludeDlc, cancellationToken);

        Task<List<SoftwareDuplicateGroupDto>> dataTask =
            SoftwareService.GetAdminDuplicateGroupsAsync(skip, take, _kindFilter, _excludeDlc, cancellationToken);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<SoftwareDuplicateGroupDto>
        {
            Items      = dataTask.Result,
            TotalItems = countTask.Result
        };
    }

    async Task OnKindChanged(SoftwareKind? value)
    {
        _kindFilter = value;
        await _dataGrid.ReloadServerData();
    }

    async Task OnExcludeDlcChanged(bool value)
    {
        _excludeDlc = value;
        await _dataGrid.ReloadServerData();
    }

    Task ReloadAsync() => _dataGrid.ReloadServerData();

    string KindLabel(SoftwareKind? kind) => kind switch
    {
        SoftwareKind.OperatingSystem     => L["Operating System"],
        SoftwareKind.Game                => L["Game"],
        SoftwareKind.Dlc                 => L["DLC / Addon"],
        SoftwareKind.SystemSoftware      => L["System software"],
        SoftwareKind.Application         => L["Application"],
        SoftwareKind.DevelopmentSoftware => L["Development software"],
        SoftwareKind.ServerSoftware      => L["Server software"],
        SoftwareKind.Middleware          => L["Middleware"],
        SoftwareKind.Firmware            => L["Firmware"],
        SoftwareKind.EmbeddedSoftware    => L["Embedded software"],
        _                                => L["Software"]
    };

    async Task OpenMergeDialog(SoftwareDuplicateItemDto item)
    {
        DialogParameters<SoftwareMergeDialog> parameters = new()
        {
            { x => x.SourceId,   (int)(item.Id ?? 0) },
            { x => x.SourceName, item.Name }
        };

        IDialogReference dialog = await DialogService.ShowAsync<SoftwareMergeDialog>(
                                      L["Merge Software"], parameters,
                                      new DialogOptions
                                      {
                                          MaxWidth  = MaxWidth.Medium,
                                          FullWidth = true
                                      });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false })
        {
            _successMessage = string.Format(L["Software '{0}' merged successfully."], item.Name);
            await _dataGrid.ReloadServerData();
        }
    }
}
