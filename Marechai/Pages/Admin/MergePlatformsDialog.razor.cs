using System.Collections.Generic;
using System.Linq;
using Marechai.ApiClient.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class MergePlatformsDialog
{
    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public List<SoftwarePlatformDto> Platforms { get; set; } = [];

    SoftwarePlatformDto SelectedTarget { get; set; }
    HashSet<ulong> SelectedSources { get; set; } = [];

    List<SoftwarePlatformDto> AvailablePlatforms => Platforms;

    List<SoftwarePlatformDto> MergeCandidates =>
        Platforms.Where(p => p.Id != SelectedTarget?.Id).ToList();

    void OnTargetChanged(SoftwarePlatformDto platform)
    {
        SelectedTarget = platform;
        SelectedSources.Clear();
    }

    void OnPlatformToggled(ulong platformId, bool isSelected)
    {
        if(isSelected)
            SelectedSources.Add(platformId);
        else
            SelectedSources.Remove(platformId);
    }

    bool IsSelected(ulong platformId) => SelectedSources.Contains(platformId);

    void Cancel() => MudDialog.Cancel();

    void Merge()
    {
        var result = new MergePlatformsDialogResult
        {
            TargetId = (ulong)(SelectedTarget.Id ?? 0),
            SourceIds = SelectedSources.ToList()
        };

        MudDialog.Close(DialogResult.Ok(result));
    }
}
