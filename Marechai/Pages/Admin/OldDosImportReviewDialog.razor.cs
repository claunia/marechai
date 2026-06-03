/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Helpers;
using Marechai.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Marechai.Pages.Admin;

public partial class OldDosImportReviewDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter]          public long        InitialId { get; set; }

    OldDosPendingDetailDto       _detail;
    OldDosNameMatchCandidatesDto _matches;
    List<SoftwareGenreDto>       _allGenres = new();
    List<SoftwarePlatformDto>    _platforms = new();
    List<CompanyDto>             _allCompanies = new();
    List<VersionRow>             _versionDecisions = new();
    CompanyDto                   _developer;
    string                       _museumEdited;
    string                       _nameOverride;
    byte                         _kindOverride = (byte)SoftwareKind.Game;
    List<int>                    _selectedGenreIds = new();
    long?                        _mergeTargetId;
    string                       _mergeTargetName;
    bool                         _loading = true;
    bool                         _busy;
    string                       _lastError;

    // Inline "create new company" panel state.
    bool             _showCreateCompany;
    bool             _creatingCompany;
    string           _newCompanyName;
    List<CompanyDto> _similarCompanies;

    const double COMPANY_SIMILARITY_THRESHOLD = 0.85;

    bool HasExactMatch => _matches?.Matches?.Any(m => m.MatchKind == (int)OldDosNameMatchKind.ExactNormalized) ?? false;

    protected override async Task OnInitializedAsync()
    {
        // Preload reference data once.
        _allGenres = (await SoftwareSvc.GetAllGenresAsync(includeUnused: true))?
                    .Where(g => g.Type == (int)SoftwareGenreType.Category)
                    .OrderBy(g => g.Name).ToList() ?? new();
        _platforms = await PlatformsSvc.GetAllAsync() ?? new();
        // Whole-catalog preload of companies so the developer autocomplete can JW-rank against
        // every company, not just whatever substring-match the server returns first. Acceptable
        // because the dialog is admin-only and the company table is small enough.
        _allCompanies = await CompaniesSvc.GetAsync() ?? new();
        await LoadDetailAsync(InitialId);
    }

    async Task LoadDetailAsync(long id)
    {
        _loading       = true;
        _lastError     = null;
        _mergeTargetId = null;
        _mergeTargetName = null;
        _detail        = await Service.GetByIdAsync(id);
        if(_detail == null)
        {
            _loading = false;
            return;
        }

        _museumEdited     = _detail.EnglishDescriptionMuseum ?? string.Empty;
        _nameOverride     = _detail.Name ?? string.Empty;
        _kindOverride     = (byte)SoftwareKind.Game;
        _selectedGenreIds = (_detail.SuggestedGenreIds ?? new List<int?>()).Where(x => x.HasValue).Select(x => x!.Value).ToList();

        _versionDecisions = (_detail.Versions ?? new List<OldDosVersionDto>()).Select(v => new VersionRow
        {
            Id                    = v.Id ?? 0,
            Include               = v.IsEnabledByDefault ?? true,
            OriginalVersionString = v.VersionString,
            VersionStringOverride = v.VersionString,
            ReleaseDateOverride   = v.ReleaseDate?.LocalDateTime,
            PrecisionByte         = null,
            OsHint                = v.OsHint,
            DownloadUrl           = v.DownloadUrl,
            FileName              = v.FileName,
            Platform              = ResolvePlatformByOsHint(v.OsHint)
        })
        .OrderBy(v => v.OriginalVersionString, NaturalStringComparer.Instance)
        .ToList();

        // Try to resolve a developer suggestion from existing companies using Jaro-Winkler over
        // the preloaded catalog. Auto-select only on a strong (>0.92) match; otherwise leave
        // empty so the admin makes an explicit choice.
        _developer = null;
        if(!string.IsNullOrWhiteSpace(_detail.DeveloperName))
        {
            CompanyDto best = _allCompanies
                             .Select(c => (Company: c, Score: JaroWinkler.Similarity(_detail.DeveloperName, c.Name ?? string.Empty)))
                             .Where(t => t.Score >= 0.92)
                             .OrderByDescending(t => t.Score)
                             .Select(t => t.Company)
                             .FirstOrDefault();
            _developer = best;
        }

        // Reset the create-new panel for the new item.
        _showCreateCompany = false;
        _newCompanyName    = null;
        _similarCompanies  = null;

        await RefreshMatchesAsync();
        _loading = false;
        StateHasChanged();
    }

    SoftwarePlatformDto ResolvePlatformByOsHint(string osHint)
    {
        if(string.IsNullOrWhiteSpace(osHint)) return null;
        return _platforms.FirstOrDefault(p =>
                   !string.IsNullOrEmpty(p.Name) &&
                   (string.Equals(p.Name, osHint, StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains(osHint, StringComparison.OrdinalIgnoreCase) ||
                    osHint.Contains(p.Name, StringComparison.OrdinalIgnoreCase)));
    }

    async Task RefreshMatchesAsync()
    {
        if(_detail == null) return;
        long id = _detail.Id ?? 0;
        if(id == 0) return;
        _matches = await Service.GetNameMatchesAsync(id, _nameOverride);
    }

    async Task OnNameChanged(string _)
    {
        await RefreshMatchesAsync();
        StateHasChanged();
    }

    void EnterMergeMode(long? targetId, string targetName)
    {
        _mergeTargetId   = targetId;
        _mergeTargetName = targetName;
    }

    void ExitMergeMode()
    {
        _mergeTargetId   = null;
        _mergeTargetName = null;
    }

    Task<IEnumerable<CompanyDto>> SearchCompanyAsync(string value, CancellationToken ct)
    {
        // Empty query → show top of catalog alphabetically.
        if(string.IsNullOrWhiteSpace(value))
            return Task.FromResult<IEnumerable<CompanyDto>>(
                _allCompanies.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).Take(50));

        string query = value.Trim();
        // Pre-filter to anything containing the query (case-insensitive) OR with JW ≥ 0.6,
        // then rank by JW similarity. Loose floor avoids missing transliterations / typos that
        // are exactly the old-dos.ru reality ("Symantek" vs "Symantec", etc.).
        IEnumerable<(CompanyDto Company, double Score)> scored = _allCompanies
           .Select(c => (Company: c, Score: JaroWinkler.Similarity(query, c.Name ?? string.Empty)))
           .Where(t => t.Score >= 0.6 ||
                       (t.Company.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));

        return Task.FromResult<IEnumerable<CompanyDto>>(
            scored.OrderByDescending(t => t.Score)
                  .ThenBy(t => t.Company.Name, StringComparer.OrdinalIgnoreCase)
                  .Select(t => t.Company)
                  .Take(50));
    }

    void OpenCreateCompanyAsync()
    {
        _showCreateCompany = true;
        // Always re-seed from the current item's raw old-dos developer string. Without
        // overriding any earlier value here the field can stay blank when reopening after a
        // prior cancel, and old-dos pages where DeveloperName is "-" already arrive as null
        // (cleaned up by the parser) — in that case fall back to the staged software name as
        // a soft hint the admin can edit.
        string seed = _detail?.DeveloperName?.Trim();
        if(string.IsNullOrWhiteSpace(seed))
            seed = _detail?.Name?.Trim();
        _newCompanyName = seed;
        RefreshSimilarCompanies();
        StateHasChanged();
    }

    void OnNewCompanyNameChanged(string _) => RefreshSimilarCompanies();

    void RefreshSimilarCompanies()
    {
        if(string.IsNullOrWhiteSpace(_newCompanyName))
        {
            _similarCompanies = null;
            return;
        }
        string q = _newCompanyName.Trim();
        _similarCompanies = _allCompanies
                           .Select(c => (Company: c, Score: JaroWinkler.Similarity(q, c.Name ?? string.Empty)))
                           .Where(t => t.Score >= COMPANY_SIMILARITY_THRESHOLD)
                           .OrderByDescending(t => t.Score)
                           .Select(t => t.Company)
                           .Take(5)
                           .ToList();
    }

    async Task CreateCompanyAsync()
    {
        if(string.IsNullOrWhiteSpace(_newCompanyName)) return;
        _creatingCompany = true;
        try
        {
            var dto = new CompanyDto
            {
                Name   = _newCompanyName.Trim(),
                Status = (int)CompanyStatus.Unknown
            };
            (int? id, string error) = await CompaniesSvc.CreateAsync(dto);
            if(id is null)
            {
                Snackbar.Add(string.Format(L["Failed to create company: {0}"].Value, error ?? "unknown error"), Severity.Error);
                return;
            }

            dto.Id = id;
            _allCompanies.Add(dto);
            _developer         = dto;
            _showCreateCompany = false;
            _newCompanyName    = null;
            _similarCompanies  = null;
            Snackbar.Add(string.Format(L["Created company #{0}: {1}"].Value, id, dto.Name), Severity.Success);
        }
        finally
        {
            _creatingCompany = false;
        }
    }

    Task<IEnumerable<SoftwareGenreDto>> SearchGenreAsync(string value, CancellationToken ct)
    {
        IEnumerable<SoftwareGenreDto> q = _allGenres.Where(g => !_selectedGenreIds.Contains(g.Id ?? 0));
        if(!string.IsNullOrWhiteSpace(value))
            q = q.Where(g => g.Name?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false);
        return Task.FromResult(q.Take(20));
    }

    Task<IEnumerable<SoftwarePlatformDto>> SearchPlatformAsync(string value, CancellationToken ct)
    {
        // Empty query — show the catalog alphabetically (no Take limit; small list, admin-only).
        if(string.IsNullOrWhiteSpace(value))
            return Task.FromResult<IEnumerable<SoftwarePlatformDto>>(
                _platforms.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase));

        string query = value.Trim();
        // Rank by exact match (case-insensitive) first, then Jaro-Winkler similarity, then alpha.
        // Includes any substring match too so "DOS" surfaces the literal "DOS" platform AND every
        // platform whose name contains "DOS" (FreeDOS, DR-DOS, etc.).
        IEnumerable<(SoftwarePlatformDto Platform, double Score, bool ExactMatch)> scored = _platforms
           .Select(p => (Platform: p,
                         Score: JaroWinkler.Similarity(query, p.Name ?? string.Empty),
                         ExactMatch: string.Equals(p.Name, query, StringComparison.OrdinalIgnoreCase)))
           .Where(t => t.ExactMatch || t.Score >= 0.6 ||
                       (t.Platform.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));

        return Task.FromResult<IEnumerable<SoftwarePlatformDto>>(
            scored.OrderByDescending(t => t.ExactMatch)
                  .ThenByDescending(t => t.Score)
                  .ThenBy(t => t.Platform.Name, StringComparer.OrdinalIgnoreCase)
                  .Select(t => t.Platform));
    }

    void AddGenre(SoftwareGenreDto g)
    {
        if(g?.Id is { } id && !_selectedGenreIds.Contains(id))
            _selectedGenreIds.Add(id);
    }

    void RemoveGenre(int id) => _selectedGenreIds.Remove(id);

    string KindLabel(int? kind) => kind.HasValue ? ((SoftwareKind)kind.Value).Humanize() : "—";

    async Task AcceptAsync()
    {
        if(_detail == null) return;
        if(_developer?.Id is null)
        {
            Snackbar.Add(L["Pick a developer company first."], Severity.Warning);
            return;
        }

        _busy      = true;
        _lastError = null;

        var dto = new AcceptOldDosImportDto
        {
            Mode               = _mergeTargetId.HasValue ? (int)OldDosAcceptMode.MergeIntoExisting : (int)OldDosAcceptMode.CreateNew,
            TargetSoftwareId   = _mergeTargetId.HasValue ? (int)_mergeTargetId.Value : null,
            NameOverride       = _nameOverride,
            KindOverride       = _kindOverride,
            MuseumDescriptionEdited = _museumEdited,
            DeveloperCompanyId = _developer.Id ?? 0,
            GenreIds           = _selectedGenreIds.Cast<int?>().ToList(),
            Versions           = _versionDecisions.Select(v => new AcceptOldDosVersionDecisionDto
            {
                OldDosVersionId             = v.Id,
                Include                     = v.Include,
                VersionStringOverride       = v.VersionStringOverride,
                ReleaseDateOverride         = v.ReleaseDateOverride.HasValue
                                                  ? new DateTimeOffset(v.ReleaseDateOverride.Value, TimeSpan.Zero)
                                                  : null,
                ReleaseDatePrecisionOverride = v.PrecisionByte,
                SoftwarePlatformIdOverride   = v.Platform?.Id is { } pid ? (int?)(int)pid : null
            }).ToList()
        };

        (AcceptOldDosImportResultDto result, string error) = await Service.AcceptAsync(_detail.Id ?? 0, dto);
        _busy = false;

        if(error != null || result?.Success != true)
        {
            _lastError = error ?? result?.Error ?? "Accept failed.";
            return;
        }

        Snackbar.Add(string.Format(L["Promoted to software #{0} ({1} versions, {2} releases, {3} descriptions, {4} genres)."].Value,
                                    result.PromotedSoftwareId, result.InsertedVersionCount, result.InsertedReleaseCount,
                                    result.InsertedDescriptionCount, result.InsertedGenreCount),
                     Severity.Success);
        await AdvanceToNextAsync();
    }

    async Task SkipAsync()
    {
        if(_detail == null) return;
        _busy = true;
        if(await Service.SkipAsync(_detail.Id ?? 0)) Snackbar.Add(L["Skipped."], Severity.Info);
        _busy = false;
        await AdvanceToNextAsync();
    }

    async Task DiscardAsync()
    {
        if(_detail == null) return;
        _busy = true;
        if(await Service.DiscardAsync(_detail.Id ?? 0)) Snackbar.Add(L["Discarded."], Severity.Warning);
        _busy = false;
        await AdvanceToNextAsync();
    }

    async Task AdvanceToNextAsync()
    {
        long currentId = _detail?.Id ?? 0;
        OldDosPendingDetailDto next = await Service.GetNextAsync(currentId);
        if(next?.Id is null)
        {
            MudDialog.Close(DialogResult.Ok(true));
            return;
        }
        await LoadDetailAsync(next.Id.Value);
    }

    void Cancel() => MudDialog.Close(DialogResult.Cancel());

    sealed class VersionRow
    {
        public long                Id                    { get; set; }
        public bool                Include               { get; set; }
        public string              OriginalVersionString { get; set; }
        public string              VersionStringOverride { get; set; }
        public DateTime?           ReleaseDateOverride   { get; set; }
        public byte?               PrecisionByte         { get; set; }
        public string              OsHint                { get; set; }
        public string              DownloadUrl           { get; set; }
        public string              FileName              { get; set; }
        public SoftwarePlatformDto Platform              { get; set; }
    }
}
