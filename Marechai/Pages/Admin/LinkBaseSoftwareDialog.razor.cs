using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Admin;

/// <summary>
///     Aggregate result returned from <see cref="LinkBaseSoftwareDialog"/> via
///     <see cref="DialogResult.Ok{T}"/>. The owning page consumes it to drive
///     the alert banner above the grid.
/// </summary>
public sealed class LinkBaseSoftwareDialogResult
{
    public string SuccessMessage { get; init; }
    public string ErrorMessage   { get; init; }
}

/// <summary>
///     Single-component link dialog that handles both single-row and bulk-link
///     flows: the caller passes a list of <see cref="SoftwareAddonDto"/>, and
///     the dialog dispatches to either
///     <see cref="SoftwareService.SetBaseSoftwareAsync"/> or
///     <see cref="SoftwareService.SetBaseSoftwareBulkAsync"/> based on count.
/// </summary>
/// <remarks>
///     <para>
///         The prefix-chip filter is only rendered in single-row mode because
///         the tokenisation of the source name is otherwise ambiguous (two DLCs
///         like "EyePet: Hospital" and "Buzz Quiz Pack" share no useful prefix).
///         Bulk mode therefore always uses a plain
///         <see cref="AddonPrefixMode.Off"/> contains-search.
///     </para>
///     <para>
///         Auto-suggest on open (single-row only): first probe with
///         <see cref="AddonPrefixMode.Exact"/> against the full normalized name;
///         if zero hits, retry with <see cref="AddonPrefixMode.StartsWith"/>
///         against the leading <c>N-1</c> tokens. Pre-select the candidate only
///         when exactly one is returned — multiple equally-likely matches are
///         always left to the operator.
///     </para>
/// </remarks>
public partial class LinkBaseSoftwareDialog
{
    SoftwareDto _selectedTarget;
    string      _errorMessage;
    bool        _isLinking;
    bool        _autoSuggested;

    // Single-row-only state.
    string[]                                   _tokens = [];
    bool                                       _filterEnabled = true;
    AddonPrefixMode                            _prefixMode    = AddonPrefixMode.Exact;
    int                                        _prefixWordCount;
    List<BulkSetBaseSoftwareFailureDto>        _partialFailures;

    bool _hasMisclassified;

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public List<SoftwareAddonDto> Addons { get; set; } = new();

    [Inject] SoftwareService SoftwareService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _hasMisclassified = Addons.Any(a => (SoftwareKind)(a.Kind ?? 0) != SoftwareKind.Dlc);

        if(Addons.Count == 1)
        {
            _tokens          = TokenizeForMatch(Addons[0].Name);
            _prefixWordCount = _tokens.Length;
            await TryAutoSuggestAsync();
        }
        else
        {
            // Bulk mode: no filter / prefix UI, plain search.
            _filterEnabled = false;
        }
    }

    /// <summary>
    ///     Mirrors the server-side <c>NormalizePrefixForMatch</c> (lowercase,
    ///     replace non-alphanumeric with space, split on whitespace). Returns
    ///     the resulting ordered token array used by the chip selector.
    /// </summary>
    static string[] TokenizeForMatch(string name)
    {
        if(string.IsNullOrWhiteSpace(name)) return [];

        var sb = new StringBuilder(name.Length);

        foreach(char c in name)
        {
            if(char.IsLetterOrDigit(c))
                sb.Append(char.ToLowerInvariant(c));
            else
                sb.Append(' ');
        }

        return sb.ToString().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
    }

    string BuildPrefix() => string.Join(' ', _tokens.Take(_prefixWordCount));

    /// <summary>
    ///     Single-row open-time probe: exact match on the full name first, then
    ///     N-1-token startswith. Only pre-selects when there's exactly one hit
    ///     so an ambiguous match never picks a wrong parent silently.
    /// </summary>
    async Task TryAutoSuggestAsync()
    {
        if(_tokens.Length == 0) return;

        // Exact full-name probe.
        List<SoftwareDto> exact = await SoftwareService.GetAddonCandidatesAsync(
                                      Addons[0].Name,
                                      string.Join(' ', _tokens),
                                      AddonPrefixMode.Exact, take: 5);

        if(exact.Count == 1)
        {
            _selectedTarget = exact[0];
            _autoSuggested  = true;
            return;
        }

        if(_tokens.Length <= 1) return;

        List<SoftwareDto> partial = await SoftwareService.GetAddonCandidatesAsync(
                                        Addons[0].Name,
                                        string.Join(' ', _tokens.Take(_tokens.Length - 1)),
                                        AddonPrefixMode.StartsWith, take: 5);

        if(partial.Count == 1)
        {
            _selectedTarget   = partial[0];
            _autoSuggested    = true;
            _prefixWordCount  = _tokens.Length - 1;
            _prefixMode       = AddonPrefixMode.StartsWith;
        }
    }

    async Task<IEnumerable<SoftwareDto>> SearchCandidates(string value, CancellationToken ct)
    {
        // Server excludes DLC-kind rows AND misclassified Game-with-DLC-genre
        // rows from candidates, so any source addon cannot be picked as its
        // own parent — no client-side filter needed.
        string prefix    = _filterEnabled ? BuildPrefix() : null;
        AddonPrefixMode mode = _filterEnabled ? _prefixMode : AddonPrefixMode.Off;

        List<SoftwareDto> results = await SoftwareService.GetAddonCandidatesAsync(value, prefix, mode, 50, ct);
        return results ?? [];
    }

    Task OnFilterEnabledChanged(bool value)
    {
        _filterEnabled = value;
        // Reset selection so the user sees a fresh search reflecting the new
        // filter state on next dropdown open.
        _selectedTarget = null;
        _autoSuggested  = false;
        return Task.CompletedTask;
    }

    Task OnPrefixModeChanged(AddonPrefixMode value)
    {
        _prefixMode     = value;
        _selectedTarget = null;
        _autoSuggested  = false;
        return Task.CompletedTask;
    }

    Task OnPrefixWordCountChanged(int value)
    {
        _prefixWordCount = value;
        _selectedTarget  = null;
        _autoSuggested   = false;
        return Task.CompletedTask;
    }

    async Task ConfirmLink()
    {
        if(_selectedTarget?.Id is null) return;

        _isLinking       = true;
        _errorMessage    = null;
        _partialFailures = null;

        ulong baseId = (ulong)_selectedTarget.Id.Value;

        if(Addons.Count == 1)
        {
            ulong rowId = (ulong)(Addons[0].Id ?? 0);
            (bool ok, string err) = await SoftwareService.SetBaseSoftwareAsync(rowId, baseId);

            _isLinking = false;

            if(ok)
            {
                MudDialog.Close(DialogResult.Ok(new LinkBaseSoftwareDialogResult
                {
                    SuccessMessage = string.Format(L["Linked \"{0}\" to \"{1}\"."],
                                                   Addons[0].Name, _selectedTarget.Name)
                }));
            }
            else
            {
                _errorMessage = string.Format(L["Linking failed: {0}"], err);
            }

            return;
        }

        // Bulk path.
        List<ulong> ids = Addons.Where(a => a.Id.HasValue).Select(a => (ulong)a.Id.Value).ToList();

        (BulkSetBaseSoftwareResultDto result, string bulkErr) =
            await SoftwareService.SetBaseSoftwareBulkAsync(ids, baseId);

        _isLinking = false;

        if(result is null)
        {
            _errorMessage = string.Format(L["Linking failed: {0}"], bulkErr);
            return;
        }

        int updated   = result.Updated ?? 0;
        int failedCnt = result.Failed?.Count ?? 0;

        if(failedCnt == 0)
        {
            MudDialog.Close(DialogResult.Ok(new LinkBaseSoftwareDialogResult
            {
                SuccessMessage = string.Format(L["Linked {0} row(s) to \"{1}\"."], updated, _selectedTarget.Name)
            }));
            return;
        }

        // Partial success: keep the dialog open so the operator can see which
        // rows failed and why, then dismiss explicitly.
        _partialFailures = result.Failed;
        _errorMessage    = updated > 0
                               ? string.Format(L["Linked {0} of {1} row(s) to \"{2}\". {3} failed."],
                                               updated, Addons.Count, _selectedTarget.Name, failedCnt)
                               : string.Format(L["No rows linked. {0} failed."], failedCnt);
    }

    void Cancel() => MudDialog.Cancel();
}
