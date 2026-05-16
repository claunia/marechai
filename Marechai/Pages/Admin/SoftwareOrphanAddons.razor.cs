using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using MudBlazor;

namespace Marechai.Pages.Admin;

/// <summary>
///     Code-behind for /admin/software/orphan-addons. The grid pages directly
///     over <see cref="SoftwareAddonDto"/> rows (no grouping); per-row and bulk
///     link operations both delegate to <see cref="LinkBaseSoftwareDialog"/>
///     which receives the full row DTO list and decides single-vs-bulk wiring
///     internally based on count.
/// </summary>
public partial class SoftwareOrphanAddons
{
    string _searchText;
    bool   _onlyOrphans = true;

    string _errorMessage;
    string _successMessage;

    MudDataGrid<SoftwareAddonDto> _dataGrid;

    /// <summary>
    ///     Two-way bound to the grid's multiselect; the bulk button is enabled
    ///     when this is non-empty. Initialised as an empty hashset so MudBlazor
    ///     can mutate it without needing a setter.
    /// </summary>
    HashSet<SoftwareAddonDto> _selectedItems = new();

    async Task<GridData<SoftwareAddonDto>> ServerReload(GridState<SoftwareAddonDto> state,
                                                        CancellationToken cancellationToken)
    {
        int skip = state.Page * state.PageSize;
        int take = state.PageSize;

        string sortBy         = null;
        bool   sortDescending = false;

        SortDefinition<SoftwareAddonDto> sort = state.SortDefinitions.FirstOrDefault();

        if(sort is not null)
        {
            sortBy         = sort.SortBy;
            sortDescending = sort.Descending;
        }

        Task<int> countTask =
            SoftwareService.GetOrphanAddonsCountAsync(_searchText, _onlyOrphans, cancellationToken);

        Task<List<SoftwareAddonDto>> dataTask =
            SoftwareService.GetOrphanAddonsPagedAsync(skip, take, _searchText, sortBy, sortDescending,
                                                      _onlyOrphans, cancellationToken);

        await Task.WhenAll(countTask, dataTask);

        return new GridData<SoftwareAddonDto>
        {
            Items      = dataTask.Result,
            TotalItems = countTask.Result
        };
    }

    async Task OnSearch(string text)
    {
        _searchText = text;
        // Reset selection — the rows in view are about to change underneath.
        _selectedItems = new HashSet<SoftwareAddonDto>();
        await _dataGrid.ReloadServerData();
    }

    async Task OnOnlyOrphansChanged(bool value)
    {
        _onlyOrphans   = value;
        _selectedItems = new HashSet<SoftwareAddonDto>();
        await _dataGrid.ReloadServerData();
    }

    async Task ReloadAsync()
    {
        _selectedItems = new HashSet<SoftwareAddonDto>();
        await _dataGrid.ReloadServerData();
    }

    string KindLabel(SoftwareKind kind) => kind switch
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

    string ReasonLabel(AddonOrphanReason reason) => reason switch
    {
        AddonOrphanReason.Linked              => L["Linked"],
        AddonOrphanReason.NoBase              => L["No base"],
        AddonOrphanReason.SelfReference       => L["Self reference"],
        AddonOrphanReason.DanglingFk          => L["Dangling base"],
        AddonOrphanReason.ChainedDlc          => L["Chained DLC"],
        AddonOrphanReason.NameMismatch        => L["Name mismatch"],
        AddonOrphanReason.MisclassifiedAsGame => L["Misclassified as Game"],
        _                                     => L["—"]
    };

    Color ReasonColor(AddonOrphanReason reason) => reason switch
    {
        AddonOrphanReason.Linked              => Color.Success,
        AddonOrphanReason.NoBase              => Color.Default,
        AddonOrphanReason.SelfReference       => Color.Error,
        AddonOrphanReason.DanglingFk          => Color.Error,
        AddonOrphanReason.ChainedDlc          => Color.Warning,
        AddonOrphanReason.NameMismatch        => Color.Warning,
        AddonOrphanReason.MisclassifiedAsGame => Color.Secondary,
        _                                     => Color.Default
    };

    /// <summary>Per-row link dialog: passes a single-item list.</summary>
    Task OpenLinkDialog(SoftwareAddonDto item) => ShowLinkDialogAsync([item]);

    /// <summary>Bulk link dialog: passes the current selection snapshot.</summary>
    Task OpenBulkLinkDialog() => ShowLinkDialogAsync(_selectedItems.ToList());

    /// <summary>
    ///     Opens <see cref="LinkBaseSoftwareDialog"/> with the supplied rows.
    ///     When the dialog completes with a success result, refreshes the grid
    ///     and surfaces the aggregated message.
    /// </summary>
    async Task ShowLinkDialogAsync(IReadOnlyList<SoftwareAddonDto> rows)
    {
        if(rows.Count == 0) return;

        DialogParameters<LinkBaseSoftwareDialog> parameters = new()
        {
            { x => x.Addons, rows.ToList() }
        };

        string title = rows.Count == 1
                           ? L["Link to base software"]
                           : string.Format(L["Link {0} rows to base software"], rows.Count);

        IDialogReference dialog = await DialogService.ShowAsync<LinkBaseSoftwareDialog>(
                                      title, parameters,
                                      new DialogOptions
                                      {
                                          MaxWidth  = MaxWidth.Medium,
                                          FullWidth = true
                                      });

        DialogResult result = await dialog.Result;

        if(result is { Canceled: false, Data: LinkBaseSoftwareDialogResult linkResult })
        {
            _errorMessage   = linkResult.ErrorMessage;
            _successMessage = linkResult.SuccessMessage;
            _selectedItems  = new HashSet<SoftwareAddonDto>();
            await _dataGrid.ReloadServerData();
        }
    }
}
