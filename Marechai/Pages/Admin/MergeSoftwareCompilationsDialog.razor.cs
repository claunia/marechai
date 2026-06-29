using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class MergeSoftwareCompilationsDialog
{
    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; } = null!;

    SoftwareCompilationDto      SelectedTarget { get; set; }
    List<SoftwareCompilationDto> SelectedSources { get; set; } = [];
    SoftwareCompilationDto      _sourcePickerValue;

    async Task<IEnumerable<SoftwareCompilationDto>> SearchTargetAsync(string value, CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(value)) return [];

        return await SoftwareCompilationsService.GetPagedAsync(0, 20, value);
    }

    async Task<IEnumerable<SoftwareCompilationDto>> SearchSourcesAsync(string value, CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(value)) return [];

        List<SoftwareCompilationDto> results = await SoftwareCompilationsService.GetPagedAsync(0, 20, value);

        return results.Where(c => c.Id != SelectedTarget?.Id &&
                                   SelectedSources.All(s => s.Id != c.Id));
    }

    void OnTargetChanged(SoftwareCompilationDto compilation)
    {
        SelectedTarget = compilation;
        SelectedSources.Clear();
        _sourcePickerValue = null;
    }

    void OnSourcePicked(SoftwareCompilationDto compilation)
    {
        if(compilation is null) return;

        if(SelectedSources.All(s => s.Id != compilation.Id)) SelectedSources.Add(compilation);

        _sourcePickerValue = null;
    }

    void RemoveSource(SoftwareCompilationDto compilation) => SelectedSources.Remove(compilation);

    void Cancel() => MudDialog.Cancel();

    void Merge()
    {
        var result = new MergeSoftwareCompilationsDialogResult
        {
            TargetId  = (ulong)(SelectedTarget.Id ?? 0),
            SourceIds = SelectedSources.Select(s => (ulong)(s.Id ?? 0)).ToList()
        };

        MudDialog.Close(DialogResult.Ok(result));
    }
}
