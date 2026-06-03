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

public partial class WwpcImportReviewDialog
{
    [Inject] SoftwareVersionsService VersionsSvc { get; set; }

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter]          public long        InitialId { get; set; }

    WwpcPendingDetailDto          _detail;
    WwpcNameMatchCandidatesDto    _matches;
    WwpcCompanyMatchCandidatesDto _vendorFuzzy;
    List<SoftwareGenreDto>        _allGenres        = new();
    List<SoftwarePlatformDto>     _platforms        = new();
    List<CompanyDto>              _allCompanies     = new();
    List<SoftwareVersionDto>      _existingVersions = new();
    List<VersionRow>              _versionDecisions    = new();
    List<ScreenshotRow>           _screenshotDecisions = new();
    CompanyDto                    _vendor;
    SoftwarePlatformDto           _bulkPlatform;
    string                        _museumEdited;
    string                        _nameOverride;
    byte                          _kindOverride = (byte)SoftwareKind.Application;
    List<int>                     _selectedGenreIds = new();
    long?                         _mergeTargetId;
    string                        _mergeTargetName;
    bool                          _loading = true;
    bool                          _busy;
    string                        _lastError;

    bool             _showCreateCompany;
    bool             _creatingCompany;
    string           _newCompanyName;

    bool HasExactMatch => _matches?.Matches?.Any(m => m.MatchKind == (int)WwpcNameMatchKind.ExactNormalized) ?? false;

    protected override async Task OnInitializedAsync()
    {
        _allGenres = (await SoftwareSvc.GetAllGenresAsync(includeUnused: true))?
                    .Where(g => g.Type == (int)SoftwareGenreType.Category)
                    .OrderBy(g => g.Name).ToList() ?? new();
        _platforms    = await PlatformsSvc.GetAllAsync() ?? new();
        _allCompanies = await CompaniesSvc.GetAsync() ?? new();
        await LoadDetailAsync(InitialId);
    }

    async Task LoadDetailAsync(long id)
    {
        _loading          = true;
        _lastError        = null;
        _mergeTargetId    = null;
        _mergeTargetName  = null;
        _existingVersions = new();
        _detail           = await Service.GetByIdAsync(id);
        if(_detail == null)
        {
            _loading = false;
            return;
        }

        _museumEdited     = _detail.EnglishDescriptionMuseum ?? string.Empty;
        _nameOverride     = _detail.Name ?? string.Empty;
        _kindOverride     = (byte)SoftwareKind.Application;
        _selectedGenreIds = (_detail.SuggestedGenreIds ?? new List<int?>()).Where(x => x.HasValue)
                                                                            .Select(x => x!.Value).ToList();

        _versionDecisions = (_detail.Versions ?? new List<WwpcVersionDto>()).Select(v => new VersionRow
        {
            Id                       = v.Id ?? 0,
            Include                  = v.IsEnabledByDefault ?? true,
            MajorRelease             = v.MajorRelease,
            OriginalVersionString    = v.VersionString,
            VersionStringOverride    = string.IsNullOrEmpty(v.MajorRelease) ? v.VersionString
                                          : $"{v.MajorRelease} / {v.VersionString}",
            Language                 = v.Language,
            Architecture             = v.Architecture,
            MediaKind                = v.MediaKind,
            SizeText                 = v.SizeText,
            DownloadUrl              = v.DownloadUrl,
            LinkToExistingVersionId  = null
        })
        // Group by major release (natural sort), then by minor version within each group.
        // Sorting on the combined override produces the same visual order in the UI table.
        .OrderBy(v => v.VersionStringOverride, NaturalStringComparer.Instance)
        .ToList();

        _screenshotDecisions = (_detail.Screenshots ?? new List<WwpcScreenshotDto>()).Select(s => new ScreenshotRow
        {
            Id              = s.Id ?? 0,
            Include         = s.IsEnabledByDefault ?? true,
            MajorRelease    = s.MajorRelease,
            SourceUrl       = s.SourceUrl,
            ImageUrl        = s.ImageUrl,
            OriginalCaption = s.Caption,
            CaptionOverride = s.Caption,
            Platform        = ResolvePlatformById(s.SuggestedSoftwarePlatformId)
        }).ToList();

        // Pre-select the auto-linked vendor company, if any.
        _vendor = null;
        if(_detail.SuggestedVendorCompanyId.HasValue)
            _vendor = _allCompanies.FirstOrDefault(c => c.Id == _detail.SuggestedVendorCompanyId.Value);

        _showCreateCompany = false;
        _newCompanyName    = null;

        await RefreshMatchesAsync();
        await RefreshVendorFuzzyAsync();
        _loading = false;
        StateHasChanged();
    }

    SoftwarePlatformDto ResolvePlatformById(int? id)
    {
        if(!id.HasValue) return null;
        return _platforms.FirstOrDefault(p => p.Id == id.Value);
    }

    async Task RefreshMatchesAsync()
    {
        if(_detail == null) return;
        long id = _detail.Id ?? 0;
        if(id == 0) return;
        _matches = await Service.GetNameMatchesAsync(id, _nameOverride);
    }

    async Task RefreshVendorFuzzyAsync()
    {
        if(_detail == null) return;
        long id = _detail.Id ?? 0;
        if(id == 0) return;
        _vendorFuzzy = await Service.GetVendorMatchesAsync(id, _detail.VendorName);
    }

    async Task OnNameChanged(string _)
    {
        await RefreshMatchesAsync();
        StateHasChanged();
    }

    async Task EnterMergeModeAsync(long? targetId, string targetName)
    {
        _mergeTargetId   = targetId;
        _mergeTargetName = targetName;
        if(targetId.HasValue)
            _existingVersions = await VersionsSvc.GetBySoftwareAsync((int)targetId.Value) ?? new();
    }

    void ExitMergeMode()
    {
        _mergeTargetId   = null;
        _mergeTargetName = null;
        _existingVersions = new();
        foreach(VersionRow v in _versionDecisions) v.LinkToExistingVersionId = null;
    }

    void PickVendor(WwpcCompanyMatchCandidateDto m)
    {
        if(!m.CompanyId.HasValue) return;
        _vendor = _allCompanies.FirstOrDefault(c => c.Id == m.CompanyId.Value);
    }

    Task<IEnumerable<CompanyDto>> SearchCompanyAsync(string value, CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(value))
            return Task.FromResult<IEnumerable<CompanyDto>>(
                _allCompanies.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).Take(50));

        string query = value.Trim();
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

    void OpenCreateCompany()
    {
        _showCreateCompany = true;
        string seed = _detail?.VendorName?.Trim();
        if(string.IsNullOrWhiteSpace(seed)) seed = _detail?.Name?.Trim();
        _newCompanyName = seed;
        StateHasChanged();
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
                Snackbar.Add(string.Format(L["Failed to create company: {0}"].Value, error ?? "unknown error"),
                             Severity.Error);
                return;
            }
            dto.Id = id;
            _allCompanies.Add(dto);
            _vendor            = dto;
            _showCreateCompany = false;
            _newCompanyName    = null;
            Snackbar.Add(string.Format(L["Created company #{0}: {1}"].Value, id, dto.Name), Severity.Success);
        }
        finally { _creatingCompany = false; }
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

    void ApplyBulkPlatform()
    {
        if(_bulkPlatform is null) return;
        foreach(ScreenshotRow s in _screenshotDecisions) s.Platform = _bulkPlatform;
    }

    string KindLabel(int? kind) => kind.HasValue ? ((SoftwareKind)kind.Value).Humanize() : "—";

    string ProductTypeLabel(int? t) => t.HasValue ? ((WwpcProductType)t.Value).Humanize() : "—";

    static string AbsoluteWwpcUrl(string url)
    {
        if(string.IsNullOrWhiteSpace(url)) return url;
        if(url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return url;
        return "https://winworldpc.com" + (url.StartsWith('/') ? url : "/" + url);
    }

    async Task AcceptAsync()
    {
        if(_detail == null) return;
        _busy      = true;
        _lastError = null;

        var dto = new AcceptWwpcImportDto
        {
            Mode                    = _mergeTargetId.HasValue
                                          ? (int)WwpcAcceptMode.MergeIntoExisting
                                          : (int)WwpcAcceptMode.CreateNew,
            TargetSoftwareId        = _mergeTargetId.HasValue ? (int)_mergeTargetId.Value : null,
            NameOverride            = _nameOverride,
            KindOverride            = _kindOverride,
            MuseumDescriptionEdited = _museumEdited,
            VendorCompanyId         = _vendor?.Id,
            GenreIds                = _selectedGenreIds.Cast<int?>().ToList(),
            Versions = _versionDecisions.Select(v => new AcceptWwpcVersionDecisionDto
            {
                WwpcVersionId           = v.Id,
                Include                 = v.Include,
                VersionStringOverride   = v.VersionStringOverride,
                LinkToExistingVersionId = v.LinkToExistingVersionId
            }).ToList(),
            Screenshots = _screenshotDecisions.Select(s => new AcceptWwpcScreenshotDecisionDto
            {
                WwpcScreenshotId   = s.Id,
                Include            = s.Include,
                CaptionOverride    = s.CaptionOverride,
                SoftwarePlatformId = s.Platform?.Id,
                SoftwareVersionId  = null
            }).ToList()
        };

        (AcceptWwpcImportResultDto result, string error) = await Service.AcceptAsync(_detail.Id ?? 0, dto);
        _busy = false;

        if(error != null || result?.Success != true)
        {
            _lastError = error ?? result?.Error ?? "Accept failed.";
            return;
        }

        Snackbar.Add(string.Format(L["Promoted to software #{0} ({1} versions, {2} screenshots, {3} descriptions, {4} genres)."].Value,
                                    result.PromotedSoftwareId, result.InsertedVersionCount, result.InsertedScreenshotCount,
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
        WwpcPendingDetailDto next = await Service.GetNextAsync(currentId);
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
        public long   Id                       { get; set; }
        public bool   Include                  { get; set; }
        public string MajorRelease             { get; set; }
        public string OriginalVersionString    { get; set; }
        public string VersionStringOverride    { get; set; }
        public string Language                 { get; set; }
        public string Architecture             { get; set; }
        public string MediaKind                { get; set; }
        public string SizeText                 { get; set; }
        public string DownloadUrl              { get; set; }
        public int?   LinkToExistingVersionId  { get; set; }
    }

    sealed class ScreenshotRow
    {
        public long                Id              { get; set; }
        public bool                Include         { get; set; }
        public string              MajorRelease    { get; set; }
        public string              SourceUrl       { get; set; }
        public string              ImageUrl        { get; set; }
        public string              OriginalCaption { get; set; }
        public string              CaptionOverride { get; set; }
        public SoftwarePlatformDto Platform        { get; set; }
    }
}
