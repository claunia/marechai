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
using Marechai.Data;
using Marechai.Helpers;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Marechai.Services;

/// <summary>
///     Filter mode for <see cref="SoftwareService.GetAddonCandidatesAsync"/>.
///     <see cref="Off"/> means no normalized-name predicate (plain
///     <c>Name.Contains(search)</c> server-side). <see cref="Exact"/> requires
///     <c>NormalizeForMatch(name) == prefix</c>; <see cref="StartsWith"/> uses a
///     <c>LIKE prefix%</c> match.
/// </summary>
public enum AddonPrefixMode
{
    Off,
    Exact,
    StartsWith
}

public class SoftwareService(Marechai.ApiClient.Client client, IRequestAdapter requestAdapter, ReferenceDataCache referenceData)
{
    static string ExtractErrorMessage(ApiException ex)
    {
        if(ex is ProblemDetails pd)
            return pd.Detail ?? pd.Title ?? ex.Message;

        return ex.Message;
    }

    // ── CRUD methods ──

    public async Task<(int? id, string error)> CreateAsync(SoftwareDto dto)
    {
        try
        {
            int? id = await client.Software.PostAsync(dto);

            return (id, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> UpdateAsync(int id, SoftwareDto dto)
    {
        try
        {
            await client.Software[id].PutAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteAsync(int id)
    {
        try
        {
            await client.Software[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Company role junction methods ──

    public async Task<List<SoftwareCompanyRoleDto>> GetCompanyRolesAsync(int softwareId)
    {
        try
        {
            List<SoftwareCompanyRoleDto> roles = await client.Software[softwareId].CompanyRoles.GetAsync();

            return roles ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> AddCompanyRoleAsync(SoftwareCompanyRoleDto dto)
    {
        try
        {
            await client.Software.CompanyRoles.PostAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> RemoveCompanyRoleAsync(int softwareId, int companyId,
                                                                              string roleId)
    {
        try
        {
            await client.Software.CompanyRoles[softwareId][companyId][roleId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Links a SoftwareGenre to a Software via the GenresBySoftware junction table.
    ///     Idempotent on the server — duplicate calls return Ok without inserting.
    /// </summary>
    public async Task<(bool succeeded, string error)> AddGenreLinkAsync(ulong softwareId, int genreId)
    {
        try
        {
            await client.Software.GenresBySoftware.PostAsync(new SoftwareGenreLinkDto
            {
                // Kiota widens uint64 wire fields to int? (no unsigned formats in OpenAPI),
                // so cast. Real Software ids fit comfortably below int.MaxValue.
                SoftwareId = (int)softwareId,
                GenreId    = genreId
            });

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Removes a SoftwareGenre link from a Software. Returns (false, ...) when the link
    ///     does not exist (server returns 404).
    /// </summary>
    public async Task<(bool succeeded, string error)> RemoveGenreLinkAsync(ulong softwareId, int genreId)
    {
        try
        {
            // Kiota emits int-typed indexers for both path segments (no unsigned formats in
            // OpenAPI), so cast the ulong softwareId. Values within signed-int range cover
            // all real Software ids in the database.
            await client.Software.GenresBySoftware[(int)softwareId][genreId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Picker methods for admin ──

    public async Task<List<SoftwareRoleDto>> GetSoftwareRolesAsync()
    {
        try
        {
            List<SoftwareRoleDto> roles = await client.Software.Roles.Enabled.GetAsync();

            return roles ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyDto>> GetAllCompaniesAsync()
    {
        try
        {
            List<CompanyDto> companies = await client.Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareFamilyDto>> GetAllFamiliesAsync()
    {
        try
        {
            List<SoftwareFamilyDto> families = await client.Software.Families.GetAsync();

            return families ?? [];
        }
        catch
        {
            return [];
        }
    }

    public Task<List<Iso31661NumericDto>> GetCountriesAsync() => referenceData.GetCountriesAsync();

    // ── Screenshot methods ──

    public async Task<SoftwareScreenshotDto> UploadScreenshotAsync(int     softwareId, byte[] fileBytes,
                                                                     string  fileName,
                                                                     ulong?  softwarePlatformId = null,
                                                                     ulong?  softwareVersionId  = null,
                                                                     string caption             = null,
                                                                     string canonicalGroupName  = null)
    {
        try
        {
            string contentType = Path.GetExtension(fileName)?.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"            => "image/png",
                ".webp"           => "image/webp",
                ".tiff" or ".tif" => "image/tiff",
                ".bmp"            => "image/bmp",
                _                 => "application/octet-stream"
            };

            var body = new MultipartBody();
            body.AddOrReplacePart("file", contentType, new MemoryStream(fileBytes), fileName);
            body.AddOrReplacePart("softwareId", "text/plain", softwareId.ToString());

            if(softwarePlatformId.HasValue)
                body.AddOrReplacePart("softwarePlatformId", "text/plain", softwarePlatformId.Value.ToString());

            if(softwareVersionId.HasValue)
                body.AddOrReplacePart("softwareVersionId", "text/plain", softwareVersionId.Value.ToString());

            if(!string.IsNullOrEmpty(caption))
                body.AddOrReplacePart("caption", "text/plain", caption);

            // Optional canonical English group name — server resolve-or-creates the
            // SoftwareScreenshotGroup row. Omitting the part leaves GroupId null on the new
            // screenshot row.
            if(!string.IsNullOrWhiteSpace(canonicalGroupName))
                body.AddOrReplacePart("canonicalGroupName", "text/plain", canonicalGroupName);

            var pathParams = new Dictionary<string, object> { { "baseurl", requestAdapter.BaseUrl } };

            var requestInfo = new RequestInformation(Method.POST,
                "{+baseurl}/software/screenshots/upload", pathParams);

            requestInfo.Headers.TryAdd("Accept", "application/json");
            requestInfo.SetContentFromParsable(requestAdapter, "multipart/form-data", body);

            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", ProblemDetails.CreateFromDiscriminatorValue },
                { "401", ProblemDetails.CreateFromDiscriminatorValue }
            };

            return await requestAdapter.SendAsync(requestInfo,
                SoftwareScreenshotDto.CreateFromDiscriminatorValue, errorMapping);
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool succeeded, string error)> DeleteScreenshotAsync(Guid screenshotId)
    {
        try
        {
            await client.Software.Screenshots[screenshotId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Picker methods ──

    public async Task<int> GetSoftwareCountAsync(SoftwareKind? kind = null)
    {
        try
        {
            int? count = await client.Software.Count.GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            });

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            int? year = await client.Software.MinimumYear.GetAsync();

            return year ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> GetMaximumYearAsync()
    {
        try
        {
            int? year = await client.Software.MaximumYear.GetAsync();

            return year ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<SoftwarePlatformDto>> GetPlatformsAsync()
    {
        try
        {
            List<SoftwarePlatformDto> platforms = await client.Software.Platforms.GetAsync();

            return platforms ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Company browsing methods ──

    public async Task<List<CompanyDto>> GetCompaniesAsync()
    {
        try
        {
            List<CompanyDto> companies = await client.Software.Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyDto>> GetCompaniesByLetterAsync(char c)
    {
        try
        {
            List<CompanyDto> companies = await client.Software.Companies.Letter[c.ToString()].GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Search methods ──

    public async Task<List<SoftwareDto>> GetSoftwareByLetterAsync(char c, SoftwareKind? kind = null,
                                                                  int? skip = null, int? take = null,
                                                                  CancellationToken cancellationToken = default)
    {
        try
        {
            List<SoftwareDto> software = await client.Software.ByLetter[c.ToString()].GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetSoftwareByLetterCountAsync(char c, SoftwareKind? kind = null,
                                                          CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Software.ByLetter[c.ToString()].Count.GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            }, cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareByYearAsync(int year, SoftwareKind? kind = null,
                                                                int? skip = null, int? take = null,
                                                                CancellationToken cancellationToken = default)
    {
        try
        {
            List<SoftwareDto> software = await client.Software.ByYear[year].GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetSoftwareByYearCountAsync(int year, SoftwareKind? kind = null,
                                                       CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Software.ByYear[year].Count.GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            }, cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareByPlatformAsync(int platformId, SoftwareKind? kind = null,
                                                                    int? skip = null, int? take = null,
                                                                    CancellationToken cancellationToken = default)
    {
        try
        {
            List<SoftwareDto> software = await client.Software.ByPlatform[platformId].GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetSoftwareByPlatformCountAsync(int platformId, SoftwareKind? kind = null,
                                                           CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Software.ByPlatform[platformId].Count.GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            }, cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<SoftwareGenreDto>> GetAllGenresAsync(bool includeUnused = false)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            List<SoftwareGenreDto> genres = await client.Software.Genres.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
                if(includeUnused) config.QueryParameters.IncludeUnused = true;
            });

            return genres ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareRankingDto>> GetRankingsAsync(SoftwareKind? kind = null,
                                                                 int? genreId = null,
                                                                 int? platformId = null,
                                                                 int? take = null,
                                                                 CancellationToken cancellationToken = default)
    {
        try
        {
            List<SoftwareRankingDto> ranked = await client.Software.Rankings.GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
                if(genreId.HasValue) config.QueryParameters.GenreId = genreId.Value;
                if(platformId.HasValue) config.QueryParameters.PlatformId = platformId.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return ranked ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareByGenreAsync(int genreId, SoftwareKind? kind = null,
                                                                 int? skip = null, int? take = null,
                                                                 CancellationToken cancellationToken = default)
    {
        try
        {
            List<SoftwareDto> software = await client.Software.ByGenre[genreId].GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetSoftwareByGenreCountAsync(int genreId, SoftwareKind? kind = null,
                                                        CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Software.ByGenre[genreId].Count.GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            }, cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<SoftwareDto>> GetAllSoftwareAsync(SoftwareKind? kind = null,
                                                             int? skip = null, int? take = null,
                                                             CancellationToken cancellationToken = default)
    {
        try
        {
            List<SoftwareDto> software = await client.Software.GetAsync(config =>
            {
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetPagedAsync(int skip, int take, string search = null,
                                                        string sortBy = null, bool sortDescending = false,
                                                        SoftwareKind? kind = null)
    {
        try
        {
            List<SoftwareDto> software = await client.Software.GetAsync(config =>
            {
                config.QueryParameters.Skip           = skip;
                config.QueryParameters.Take           = take;
                config.QueryParameters.Search         = search;
                config.QueryParameters.SortBy         = sortBy;
                config.QueryParameters.SortDescending = sortDescending;
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            });

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Admin-only paged software list (lean projection — no covers, no
    /// compilations). Backed by GET /software/admin. Use this from the
    /// /admin/software grid; it's significantly faster than
    /// <see cref="GetPagedAsync"/> because the server skips the cover
    /// sub-query and the compilations UNION the public endpoint pays for.
    /// </summary>
    public async Task<List<SoftwareDto>> GetAdminPagedAsync(int skip, int take, string search = null,
                                                            string sortBy = null, bool sortDescending = false,
                                                            SoftwareKind? kind = null)
    {
        try
        {
            List<SoftwareDto> software = await client.Software.Admin.GetAsync(config =>
            {
                config.QueryParameters.Skip           = skip;
                config.QueryParameters.Take           = take;
                config.QueryParameters.Search         = search;
                config.QueryParameters.SortBy         = sortBy;
                config.QueryParameters.SortDescending = sortDescending;
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            });

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareDto>> SearchSoftwareAsync(string search, SoftwareKind? kind = null)
    {
        try
        {
            List<SoftwareDto> software = await client.Software.GetAsync(config =>
            {
                config.QueryParameters.Take   = 20;
                config.QueryParameters.Search = search;
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            });

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    ///     Picker-friendly variant of <see cref="SearchSoftwareAsync" />: returns up to
    ///     <paramref name="take" /> rows sorted alphabetically even when <paramref name="search" /> is
    ///     null/whitespace, so admin pickers (e.g. Predecessor / Base Software in
    ///     <c>SoftwareDialog</c>) can show a browseable initial list on first focus instead of an
    ///     empty popover. Mirrors the precedent in <c>LinkBaseSoftwareDialog</c>.
    /// </summary>
    public async Task<List<SoftwareDto>> SearchSoftwareForPickerAsync(string            search,
                                                                      int               take              = 50,
                                                                      CancellationToken cancellationToken = default)
    {
        try
        {
            List<SoftwareDto> software = await client.Software.GetAsync(config =>
            {
                config.QueryParameters.Take   = take;
                config.QueryParameters.Search = search;
            }, cancellationToken);

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetCountAsync(string search = null, SoftwareKind? kind = null,
                                         CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Software.Count.GetAsync(config =>
            {
                config.QueryParameters.Search = search;
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            }, cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Admin-only software count companion to <see cref="GetAdminPagedAsync"/>.
    /// Software-only (no compilations); backed by GET /software/admin/count.
    /// </summary>
    public async Task<int> GetAdminCountAsync(string search = null, SoftwareKind? kind = null,
                                              CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Software.Admin.Count.GetAsync(config =>
            {
                config.QueryParameters.Search = search;
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            }, cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Admin-only paged list of pseudoduplicate Software groups for
    /// /admin/software/duplicates. Backed by GET /software/admin/duplicates.
    /// Pagination is applied to <em>groups</em> on the server side.
    /// </summary>
    public async Task<List<SoftwareDuplicateGroupDto>> GetAdminDuplicateGroupsAsync(int skip, int take,
        SoftwareKind? kind = null, bool excludeDlc = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            List<SoftwareDuplicateGroupDto> groups = await client.Software.Admin.Duplicates.GetAsync(config =>
            {
                config.QueryParameters.Skip       = skip;
                config.QueryParameters.Take       = take;
                config.QueryParameters.ExcludeDlc = excludeDlc;
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            }, cancellationToken);

            return groups ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Count of pseudoduplicate Software groups under the same filter set as
    /// <see cref="GetAdminDuplicateGroupsAsync"/>. Backed by
    /// GET /software/admin/duplicates/count.
    /// </summary>
    public async Task<int> GetAdminDuplicateGroupsCountAsync(SoftwareKind? kind = null, bool excludeDlc = false,
                                                             CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Software.Admin.Duplicates.Count.GetAsync(config =>
            {
                config.QueryParameters.ExcludeDlc = excludeDlc;
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            }, cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    // ── Orphan add-ons / DLCs ──

    /// <summary>
    ///     Page of <see cref="SoftwareAddonDto"/> rows for the
    ///     <c>/admin/software/orphan-addons</c> grid. When <paramref name="onlyOrphans"/>
    ///     is true (default), returns only DLC-kind rows with a broken
    ///     <c>BaseSoftwareId</c> link AND misclassified <c>Kind=Game</c> rows that
    ///     carry a DLC / add-on genre. When false, returns the full DLC + misclassified
    ///     set so the operator can also re-link rows that are already linked.
    /// </summary>
    public async Task<List<SoftwareAddonDto>> GetOrphanAddonsPagedAsync(int skip, int take, string search = null,
                                                                       string sortBy = null,
                                                                       bool sortDescending = false,
                                                                       bool onlyOrphans = true,
                                                                       CancellationToken cancellationToken = default)
    {
        try
        {
            List<SoftwareAddonDto> rows = await client.Software.Admin.Addons.GetAsync(config =>
            {
                config.QueryParameters.Skip           = skip;
                config.QueryParameters.Take           = take;
                config.QueryParameters.Search         = search;
                config.QueryParameters.SortBy         = sortBy;
                config.QueryParameters.SortDescending = sortDescending;
                config.QueryParameters.OnlyOrphans    = onlyOrphans;
            }, cancellationToken);

            return rows ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    ///     Count companion to <see cref="GetOrphanAddonsPagedAsync"/>. Not cached:
    ///     admins expect the total to drop as soon as they finish linking a row.
    /// </summary>
    public async Task<int> GetOrphanAddonsCountAsync(string search = null, bool onlyOrphans = true,
                                                     CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Software.Admin.Addons.Count.GetAsync(config =>
            {
                config.QueryParameters.Search      = search;
                config.QueryParameters.OnlyOrphans = onlyOrphans;
            }, cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    ///     Candidate base-software lookup for the link dialog's autocomplete. The
    ///     server excludes both DLC-kind rows and misclassified Game-with-DLC-genre
    ///     rows so the operator can't pick another orphan as the parent. Use
    ///     <paramref name="mode"/> = <see cref="AddonPrefixMode.Off"/> for plain
    ///     server-side <c>Contains(search)</c>; the other two values gate the result
    ///     on the SQL <c>NormalizeForMatch</c> of the candidate name.
    /// </summary>
    public async Task<List<SoftwareDto>> GetAddonCandidatesAsync(string search, string prefix, AddonPrefixMode mode,
                                                                 int take = 50,
                                                                 CancellationToken cancellationToken = default)
    {
        try
        {
            string modeToken = mode switch
            {
                AddonPrefixMode.Exact      => "exact",
                AddonPrefixMode.StartsWith => "startswith",
                _                          => "off"
            };

            List<SoftwareDto> candidates = await client.Software.Admin.Addons.Candidates.GetAsync(config =>
            {
                config.QueryParameters.Search = search;
                config.QueryParameters.Prefix = prefix;
                config.QueryParameters.Mode   = modeToken;
                config.QueryParameters.Take   = take;
            }, cancellationToken);

            return candidates ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    ///     Single-row link operation. Sets <c>Software.BaseSoftwareId</c> for
    ///     <paramref name="id"/> to <paramref name="baseSoftwareId"/> (or clears it
    ///     when null), and atomically flips <c>Kind</c> to <c>Dlc</c> for
    ///     misclassified <c>Kind=Game</c> rows.
    /// </summary>
    public async Task<(bool succeeded, string error)> SetBaseSoftwareAsync(ulong id, ulong? baseSoftwareId)
    {
        try
        {
            var body = new SetBaseSoftwareRequestDto
            {
                BaseSoftwareId = baseSoftwareId.HasValue ? (int?)baseSoftwareId.Value : null
            };

            await client.Software[(int)id].BaseSoftware.PatchAsync(body);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Bulk link operation. Loops the same per-row validation server-side and
    ///     returns <see cref="BulkSetBaseSoftwareResultDto"/> with the per-row
    ///     failures so the dialog can render a partial-success summary.
    /// </summary>
    public async Task<(BulkSetBaseSoftwareResultDto result, string error)> SetBaseSoftwareBulkAsync(
        IReadOnlyList<ulong> ids, ulong baseSoftwareId)
    {
        try
        {
            var body = new BulkSetBaseSoftwareRequestDto
            {
                BaseSoftwareId = (int)baseSoftwareId,
                SoftwareIds    = ids.Select(i => (int?)i).ToList()
            };

            BulkSetBaseSoftwareResultDto result = await client.Software.Admin.Addons.BaseSoftwareBulk.PatchAsync(body);

            return (result, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<List<SoftwareSpecKeyDto>> GetSpecificationsAsync()
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            List<SoftwareSpecKeyDto> specs = await client.Software.Specifications.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return specs ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetSoftwareBySpecAsync(string key, string value, SoftwareKind? kind = null,
                                                                int? skip = null, int? take = null,
                                                                CancellationToken cancellationToken = default)
    {
        try
        {
            List<SoftwareDto> software = await client.Software.BySpec.GetAsync(config =>
            {
                config.QueryParameters.Key   = key;
                config.QueryParameters.Value = value;
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
                if(skip.HasValue) config.QueryParameters.Skip = skip.Value;
                if(take.HasValue) config.QueryParameters.Take = take.Value;
            }, cancellationToken);

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetSoftwareBySpecCountAsync(string key, string value, SoftwareKind? kind = null,
                                                       CancellationToken cancellationToken = default)
    {
        try
        {
            int? count = await client.Software.BySpec.Count.GetAsync(config =>
            {
                config.QueryParameters.Key   = key;
                config.QueryParameters.Value = value;
                if(kind.HasValue) config.QueryParameters.Kind = (int)kind.Value;
            }, cancellationToken);

            return count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    // ── Detail methods ──

    public async Task<SoftwareDto> GetSoftwareByIdAsync(int id)
    {
        try
        {
            return await client.Software[id].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SoftwareCompanyRoleDto>> GetCompaniesAsync(int softwareId)
    {
        try
        {
            List<SoftwareCompanyRoleDto> companies = await client.Software[softwareId].CompanyRoles.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<PersonBySoftwareDto>> GetCreditsBySoftwareAsync(int softwareId)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            List<PersonBySoftwareDto> credits = await client.Software[softwareId].Credits.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return credits ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    ///     Returns the DISTINCT credit-role pairs already in use across the
    ///     <c>PeopleBySoftware</c> table. Each pair carries both the localized display value
    ///     (in the active UI language with English fallback) and the canonical English value
    ///     so the credits-suggestion dialog can show roles in the contributor's language while
    ///     still submitting canonical English on the wire. Allows reuse of established roles
    ///     while still accepting custom new entries.
    /// </summary>
    public async Task<List<SoftwareCreditRoleDto>> GetCreditsRolesAsync()
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            List<SoftwareCreditRoleDto> roles = await client.Software.Credits.Roles.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });
            return roles ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    ///     Server-typeahead person picker (people corpus is too large to pre-load). Min
    ///     2 chars — clients should debounce by ~300 ms.
    /// </summary>
    public async Task<List<PersonDto>> SearchPeopleAsync(string search)
    {
        if(string.IsNullOrWhiteSpace(search) || search.Length < 2) return [];
        try
        {
            List<PersonDto> people = await client.People.GetAsync(config =>
            {
                config.QueryParameters.Take   = 20;
                config.QueryParameters.Search = search;
            });
            return people ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareGenreDto>> GetGenresAsync(int softwareId)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            List<SoftwareGenreDto> genres = await client.Software[softwareId].Genres.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return genres ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareDto>> GetAddonsAsync(int softwareId)
    {
        try
        {
            List<SoftwareDto> addons = await client.Software[softwareId].Addons.GetAsync();

            return addons ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareAttributeDto>> GetAttributesAsync(int softwareId)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            List<SoftwareAttributeDto> attributes = await client.Software[softwareId].Attributes.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return attributes ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareAttributeDto>> GetReleaseAttributesAsync(int releaseId)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            List<SoftwareAttributeDto> attributes =
                await client.Software.Releases[releaseId].Attributes.GetAsync(config =>
                {
                    config.QueryParameters.Lang = lang;
                });

            return attributes ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareVersionDto>> GetVersionsAsync(int softwareId)
    {
        try
        {
            List<SoftwareVersionDto> versions = await client.Software[softwareId].Versions.GetAsync();

            return versions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetReleasesByVersionAsync(int versionId)
    {
        try
        {
            List<SoftwareReleaseDto> releases = await client.Software.Versions[versionId].Releases.GetAsync();

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<Guid?>> GetScreenshotIdsAsync(int softwareId)
    {
        try
        {
            List<Guid?> ids = await client.Software[softwareId].Screenshots.GetAsync();

            return ids ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoftwareScreenshotDto> GetScreenshotDetailsAsync(Guid id)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            return await client.Software.Screenshots[id].GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SoftwareScreenshotDto>> GetScreenshotsBySoftwareAsync(int softwareId)
    {
        try
        {
            List<Guid?> ids = await client.Software[softwareId].Screenshots.GetAsync();

            if(ids is null or { Count: 0 }) return [];

            // Fan out: fetch every screenshot's details in parallel instead of N
            // sequential round-trips. This turns an O(N) latency wall into O(1)
            // (capped by the HttpClient connection pool).
            Task<SoftwareScreenshotDto>[] tasks = ids
                                                 .Where(id => id.HasValue)
                                                 .Select(id => GetScreenshotDetailsAsync(id!.Value))
                                                 .ToArray();

            SoftwareScreenshotDto[] results = await Task.WhenAll(tasks);

            return results.Where(d => d is not null).ToList();
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> UpdateScreenshotAsync(Guid id, SoftwareScreenshotDto dto)
    {
        try
        {
            await client.Software.Screenshots[id].PutAsync(dto);

            return (true, null);
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Autocomplete fetch for the admin uploader + suggestion-dialog screenshot-group
    ///     picker. Returns up to 25 groups matching <paramref name="search" /> (substring,
    ///     case-insensitive) localized to the current UI culture with English fallback. Both
    ///     <see cref="SoftwareScreenshotGroupDto.Name" /> (localized) and
    ///     <see cref="SoftwareScreenshotGroupDto.CanonicalName" /> (English) are populated so
    ///     the UI can display the user-friendly localized text but submit the canonical English
    ///     value back to the server's resolve-or-create helper.
    /// </summary>
    public async Task<List<SoftwareScreenshotGroupDto>> GetScreenshotGroupsAsync(string search = null)
    {
        try
        {
            return await client.Software.Screenshots.Groups.GetAsync(config =>
            {
                if(!string.IsNullOrWhiteSpace(search))
                    config.QueryParameters.Search = search;
            }) ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Release detail methods ──

    public async Task<SoftwareVersionDto> GetVersionByIdAsync(int versionId)
    {
        try
        {
            return await client.Software.Versions[versionId].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<SoftwareReleaseDto> GetReleaseByIdAsync(int releaseId)
    {
        try
        {
            return await client.Software.Releases[releaseId].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SoftwareBarcodeDto>> GetBarcodesAsync(int releaseId)
    {
        try
        {
            List<SoftwareBarcodeDto> barcodes = await client.Software.Releases[releaseId].Barcodes.GetAsync();

            return barcodes ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareProductCodeDto>> GetProductCodesAsync(int releaseId)
    {
        try
        {
            List<SoftwareProductCodeDto> codes =
                await client.Software.Releases[releaseId].ProductCodes.GetAsync();

            return codes ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<GpuBySoftwareReleaseDto>> GetMinimumGpusAsync(int releaseId)
    {
        try
        {
            List<GpuBySoftwareReleaseDto> gpus =
                await client.Software.Releases[releaseId].MinimumGpus.GetAsync();

            return gpus ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<GpuBySoftwareReleaseDto>> GetRecommendedGpusAsync(int releaseId)
    {
        try
        {
            List<GpuBySoftwareReleaseDto> gpus =
                await client.Software.Releases[releaseId].RecommendedGpus.GetAsync();

            return gpus ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoundSynthBySoftwareReleaseDto>> GetSoundSynthsAsync(int releaseId)
    {
        try
        {
            List<SoundSynthBySoftwareReleaseDto> synths =
                await client.Software.Releases[releaseId].SoundSynths.GetAsync();

            return synths ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Company aggregation methods (for release detail) ──

    public async Task<List<CompanyBySoftwareVersionDto>> GetCompaniesByVersionAsync(int versionId)
    {
        try
        {
            List<CompanyBySoftwareVersionDto> companies =
                await client.Software.Versions[versionId].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<CompanyBySoftwareFamilyDto>> GetCompaniesByFamilyAsync(int familyId)
    {
        try
        {
            List<CompanyBySoftwareFamilyDto> companies =
                await client.Software.Families[familyId].Companies.GetAsync();

            return companies ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareVersionBySoftwareReleaseDto>> GetIncludedVersionsAsync(int releaseId)
    {
        try
        {
            List<SoftwareVersionBySoftwareReleaseDto> versions =
                await client.Software.Releases[releaseId].Versions.GetAsync();

            return versions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetCompilationsForSoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareReleaseDto> compilations = await client.Software[softwareId].Compilations.GetAsync();

            return compilations ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetReleasesBySoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareReleaseDto> releases = await client.Software[softwareId].Releases.GetAsync();

            return releases ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwareBySoftwareReleaseDto>> GetIncludedSoftwareAsync(int releaseId)
    {
        try
        {
            List<SoftwareBySoftwareReleaseDto> software =
                await client.Software.Releases[releaseId].Software.GetAsync();

            return software ?? [];
        }
        catch
        {
            return [];
        }
    }

    // ── Description methods ──

    public async Task<string> GetDescriptionTextAsync(int id, string lang = "eng")
    {
        try
        {
            var desc = await client.Software[id].Description.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });

            return desc?.Html ?? desc?.Markdown;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Fetch the full description DTO so the caller can detect language fallback (the
    ///     <c>LanguageCode</c> on the returned object is the language actually served, not the
    ///     language requested). Returns <c>null</c> when no description exists in any language.
    /// </summary>
    public async Task<SoftwareDescriptionDto> GetDescriptionAsync(int id, string lang = "eng")
    {
        try
        {
            return await client.Software[id].Description.GetAsync(rc =>
            {
                if(!string.IsNullOrWhiteSpace(lang)) rc.QueryParameters.Lang = lang;
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SoftwareDescriptionDto>> GetDescriptionsAsync(int softwareId)
    {
        try
        {
            List<SoftwareDescriptionDto> descriptions = await client.Software[softwareId].Descriptions.GetAsync();

            return descriptions ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool succeeded, string error)> CreateOrUpdateDescriptionAsync(int                    softwareId,
                                                                                      SoftwareDescriptionDto dto)
    {
        try
        {
            await client.Software[softwareId].Description.PostAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteDescriptionAsync(int softwareId, string languageCode)
    {
        try
        {
            await client.Software[softwareId].Description[languageCode].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Cover methods ──

    public async Task<List<SoftwareCoverDto>> GetCoversBySoftwareAsync(int softwareId)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            List<SoftwareCoverDto> covers = await client.Software[softwareId].Covers.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return covers ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<Guid?>> GetCoverIdsByReleaseAsync(int releaseId)
    {
        try
        {
            List<Guid?> ids = await client.Software.Releases[releaseId].Covers.GetAsync();

            return ids ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoftwareCoverDto> GetCoverDetailsAsync(Guid id)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            return await client.Software.Covers[id].GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<SoftwareCoverDto> UploadCoverAsync(ulong  releaseId, int    type, string caption,
                                                          byte[] fileBytes, string fileName)
    {
        try
        {
            string contentType = Path.GetExtension(fileName)?.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"            => "image/png",
                ".webp"           => "image/webp",
                ".tiff" or ".tif" => "image/tiff",
                ".bmp"            => "image/bmp",
                _                 => "application/octet-stream"
            };

            var body = new MultipartBody();
            body.AddOrReplacePart("file", contentType, new MemoryStream(fileBytes), fileName);
            body.AddOrReplacePart("releaseId", "text/plain", releaseId.ToString());
            body.AddOrReplacePart("type", "text/plain", type.ToString());

            if(!string.IsNullOrEmpty(caption))
                body.AddOrReplacePart("caption", "text/plain", caption);

            var pathParams = new Dictionary<string, object> { { "baseurl", requestAdapter.BaseUrl } };

            var requestInfo = new RequestInformation(Method.POST,
                "{+baseurl}/software/covers/upload", pathParams);

            requestInfo.Headers.TryAdd("Accept", "application/json");
            requestInfo.SetContentFromParsable(requestAdapter, "multipart/form-data", body);

            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", ProblemDetails.CreateFromDiscriminatorValue },
                { "401", ProblemDetails.CreateFromDiscriminatorValue }
            };

            return await requestAdapter.SendAsync(requestInfo,
                SoftwareCoverDto.CreateFromDiscriminatorValue, errorMapping);
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool succeeded, string error)> UpdateCoverAsync(Guid id, SoftwareCoverDto dto)
    {
        try
        {
            await client.Software.Covers[id].PutAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteCoverAsync(Guid id)
    {
        try
        {
            await client.Software.Covers[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<SoftwareMergePreviewDto> GetMergePreviewAsync(int targetId, int sourceId)
    {
        try
        {
            return await client.Software[targetId].MergePreview[sourceId].GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool succeeded, string error)> MergeSoftwareAsync(int targetId, int sourceId,
                                                                         string releaseTitle)
    {
        try
        {
            await client.Software[targetId].Merge[sourceId].PostAsync(config =>
            {
                config.QueryParameters.ReleaseTitle = releaseTitle;
            });

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Critic Reviews ──

    public async Task<List<SoftwareCriticReviewDto>> GetCriticReviewsAsync(int softwareId)
    {
        try
        {
            List<SoftwareCriticReviewDto> reviews = await client.Software[softwareId].CriticReviews.GetAsync();

            return reviews ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<CriticReviewSummaryDto> GetCriticReviewSummaryAsync(int softwareId)
    {
        try
        {
            return await client.Software[softwareId].CriticReviews.Summary.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    // ── User Ratings ──

    public async Task<UserReviewSummaryDto> GetUserReviewSummaryAsync(int softwareId)
    {
        try
        {
            return await client.Software[softwareId].UserRatings.Summary.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<SoftwareUserRatingDto> GetMyRatingAsync(int softwareId)
    {
        try
        {
            return await client.Software[softwareId].UserRatings.Me.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool succeeded, string error)> SetMyRatingAsync(int softwareId, float rating)
    {
        try
        {
            await client.Software[softwareId].UserRatings.Me.PutAsync(new SetRatingRequest
            {
                Rating = rating
            });

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteMyRatingAsync(int softwareId)
    {
        try
        {
            await client.Software[softwareId].UserRatings.Me.DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── User Reviews ──

    public async Task<List<SoftwareUserReviewDto>> GetUserReviewsAsync(int softwareId)
    {
        try
        {
            List<SoftwareUserReviewDto> reviews = await client.Software[softwareId].UserReviews.GetAsync();

            return reviews ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(SoftwareUserReviewDto review, string error)> CreateUserReviewAsync(int softwareId,
        SoftwareUserReviewDto dto)
    {
        try
        {
            SoftwareUserReviewDto result = await client.Software[softwareId].UserReviews.PostAsync(dto);

            return (result, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> UpdateUserReviewAsync(int softwareId, long reviewId,
        SoftwareUserReviewDto dto)
    {
        try
        {
            await client.Software[softwareId].UserReviews[reviewId].PutAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteUserReviewAsync(int softwareId, long reviewId)
    {
        try
        {
            await client.Software[softwareId].UserReviews[reviewId].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Review Votes ──

    public async Task<bool> VoteReviewAsync(int softwareId, long reviewId, bool isUpvote)
    {
        try
        {
            await client.Software[softwareId].UserReviews[reviewId].Vote.PostAsync(
                new SoftwareUserReviewVoteDto { IsUpvote = isUpvote });

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveReviewVoteAsync(int softwareId, long reviewId)
    {
        try
        {
            await client.Software[softwareId].UserReviews[reviewId].Vote.DeleteAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    // ── Review Reports ──

    public async Task<(bool succeeded, string error)> ReportReviewAsync(int softwareId, long reviewId,
        CreateReviewReportRequest dto)
    {
        try
        {
            await client.Software[softwareId].UserReviews[reviewId].Report.PostAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // ── Marechai Score ──

    public async Task<MarechaiScoreDto> GetMarechaiScoreAsync(int softwareId)
    {
        try
        {
            return await client.Software[softwareId].MarechaiScore.GetAsync();
        }
        catch
        {
            return null;
        }
    }

    // ── Promo Art ──

    public async Task<List<SoftwarePromoArtDto>> GetPromoArtBySoftwareAsync(int softwareId)
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            var result = await client.Software[softwareId].PromoArt.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<SoftwarePromoArtGroupDto>> GetPromoArtGroupsAsync()
    {
        try
        {
            string lang = UiLanguage.GetIso639_3();

            List<SoftwarePromoArtGroupDto> result = await client.Software.PromoArt.Groups.GetAsync(config =>
            {
                config.QueryParameters.Lang = lang;
            });

            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<SoftwarePromoArtDto> UploadPromoArtAsync(ulong  softwareId, string groupName, string caption,
                                                               byte[] fileBytes,  string fileName)
    {
        try
        {
            string contentType = Path.GetExtension(fileName)?.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"            => "image/png",
                ".webp"           => "image/webp",
                ".tiff" or ".tif" => "image/tiff",
                ".bmp"            => "image/bmp",
                _                 => "application/octet-stream"
            };

            var body = new MultipartBody();
            body.AddOrReplacePart("file", contentType, new MemoryStream(fileBytes), fileName);
            body.AddOrReplacePart("softwareId", "text/plain", softwareId.ToString());
            body.AddOrReplacePart("groupName", "text/plain", groupName);

            if(!string.IsNullOrEmpty(caption))
                body.AddOrReplacePart("caption", "text/plain", caption);

            var pathParams = new Dictionary<string, object> { { "baseurl", requestAdapter.BaseUrl } };

            var requestInfo = new RequestInformation(Method.POST,
                                                     "{+baseurl}/software/promo-art/upload", pathParams);

            requestInfo.Headers.TryAdd("Accept", "application/json");
            requestInfo.SetContentFromParsable(requestAdapter, "multipart/form-data", body);

            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", ProblemDetails.CreateFromDiscriminatorValue },
                { "401", ProblemDetails.CreateFromDiscriminatorValue }
            };

            return await requestAdapter.SendAsync(requestInfo,
                                                  SoftwarePromoArtDto.CreateFromDiscriminatorValue, errorMapping);
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool succeeded, string error)> UpdatePromoArtAsync(Guid id, UpdateSoftwarePromoArtRequest dto)
    {
        try
        {
            await client.Software.PromoArt[id].PutAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeletePromoArtAsync(Guid id)
    {
        try
        {
            await client.Software.PromoArt[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Delete a pending Software promo art image (a not-yet-submitted file the
    ///     collaborator staged via <c>POST /software/promo-art/pending</c>). Used by the
    ///     suggestion dialog when the user removes an image from the staging list before
    ///     submission, OR when they cancel the dialog with images still staged.
    /// </summary>
    public async Task<bool> DeletePendingPromoArtAsync(Guid guid)
    {
        try
        {
            await client.Software.PromoArt.Pending[guid].DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Cancel a pending Software screenshot upload by its sidecar guid (a
    ///     collaborator staged via <c>POST /software/screenshots/pending</c>). Used by the
    ///     suggestion dialog when the user removes an image from the staging list before
    ///     submission, OR when they cancel the dialog with images still staged.
    /// </summary>
    public async Task<bool> DeletePendingScreenshotAsync(Guid guid)
    {
        try
        {
            await client.Software.Screenshots.Pending[guid].DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ── Videos ──

    public async Task<List<SoftwareVideoDto>> GetVideosBySoftwareAsync(int softwareId)
    {
        try
        {
            var result = await client.Software[softwareId].Videos.GetAsync();

            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(SoftwareVideoDto dto, string error)> CreateVideoAsync(int    softwareId, string provider,
                                                                             string videoId,
                                                                             string title)
    {
        try
        {
            var dto = await client.Software[softwareId]
                                  .Videos.PostAsync(new CreateSoftwareVideoRequest
                                                    {
                                                        Provider = provider,
                                                        VideoId  = videoId,
                                                        Title    = title
                                                    });

            return (dto, null);
        }
        catch(ApiException ex)
        {
            return (null, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> UpdateVideoTitleAsync(long id, string title)
    {
        try
        {
            await client.Software.Videos[id].PutAsync(new UpdateSoftwareVideoRequest
                                                      {
                                                          Title = title
                                                      });

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string error)> DeleteVideoAsync(long id)
    {
        try
        {
            await client.Software.Videos[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            return (false, ExtractErrorMessage(ex));
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
