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
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Data;
using Marechai.Database.Models;
using Marechai.Server.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Suggestions;

/// <summary>
///     Applier for collaborative SoundSynth video link suggestions
///     (<see cref="SuggestionEntityType.SoundSynthVideo" />).
///     <para>
///         A single suggestion row carries one brand-new YouTube video link to be added to an
///         existing <see cref="Marechai.Database.Models.SoundSynth" />. The user types only a
///         URL (or 11-character video ID); the canonical title is fetched server-side from
///         YouTube's oEmbed endpoint at SUBMISSION time and patched into the
///         <c>SuggestedValues</c> dictionary BEFORE the suggestion is persisted, so admins
///         see the canonical title in the diff panel from the start. The whole suggestion is
///         accepted or rejected as a unit — there are no per-item dynamic accept-keys.
///     </para>
///     <para>
///         The Suggestion <c>Subkey</c> is populated with the extracted YouTube video ID so
///         the per-(user, sound synth, video) Pending dedupe gates resubmission of the same
///         video while a previous suggestion is still pending.
///     </para>
///     <para>
///         Acceptance creates a new <see cref="SoundSynthVideo" /> row with
///         <c>Provider = "YouTube"</c>. Only YouTube URLs are currently supported.
///     </para>
/// </summary>
public static class SoundSynthVideoSuggestionApplier
{
    /// <summary>
    ///     Wire field name carrying the user-typed YouTube URL or 11-character video ID. The
    ///     server normalises to a bare video ID via <see cref="YouTubeUrlParser.TryExtract" />.
    /// </summary>
    public const string FieldVideoUrl = "video_url";

    /// <summary>
    ///     Wire field name carrying the canonical YouTube video title. Populated SERVER-SIDE
    ///     by the validator from oEmbed at submission time — never from user input.
    /// </summary>
    public const string FieldTitle = "title";

    /// <summary>Hardcoded provider literal stored on every accepted row.</summary>
    public const string ProviderName = "YouTube";

    /// <summary>Maximum length for the user-typed URL (defensive cap; YouTube URLs are short).</summary>
    public const int MaxVideoUrlLength = 2048;

    /// <summary>Maximum length for the title (matches DB column).</summary>
    public const int MaxTitleLength = 512;

    /// <summary>
    ///     Validate the field-name keys carried by a SoundSynthVideo suggestion. Recognises
    ///     only the two literal scalars (<see cref="FieldVideoUrl" />, <see cref="FieldTitle" />);
    ///     no dynamic keys. Used both by the controller's blanket field-name allow-list check
    ///     and by the post-accept generic-machinery filter (see Phase-3e Fix-2 in
    ///     <c>SuggestionsController.ReviewAsync</c>).
    /// </summary>
    public static bool IsKnownFieldName(string fieldName) =>
        fieldName == FieldVideoUrl || fieldName == FieldTitle;

    /// <summary>
    ///     Returns an empty (but non-null) dictionary when the parent SoundSynth exists, else
    ///     <c>null</c> so the diff endpoint can mark the suggestion as targeting a deleted
    ///     entity (Phase-3e Fix-1).
    /// </summary>
    public static async Task<Dictionary<string, object>> GetCurrentValuesAsync(MarechaiContext context, long entityId)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));

        int synthId = (int)entityId;
        bool exists     = await context.SoundSynths.AsNoTracking().AnyAsync(p => p.Id == synthId);
        return exists ? new Dictionary<string, object>(StringComparer.Ordinal) : null;
    }

    /// <summary>
    ///     Validate the submitted payload BEFORE the suggestion row is persisted. Verifies
    ///     the parent SoundSynth exists, the URL parses to a YouTube video ID, the (sound synth,
    ///     YouTube, videoId) triple is not already present in the <c>SoundSynthVideos</c>
    ///     table, and fetches the canonical title from YouTube's oEmbed endpoint. Returns
    ///     <c>(true, null, videoId, title)</c> on success or <c>(false, errorDetail, null,
    ///     null)</c> otherwise. The caller MUST patch <c>suggested[FieldTitle] = title</c>
    ///     and override <c>subkey = videoId</c> on success before persistence.
    /// </summary>
    public static async Task<(bool ok, string error, string videoId, string title)> ValidateAsync(
        MarechaiContext context, long? entityId, Dictionary<string, object> suggested,
        IHttpClientFactory httpFactory, CancellationToken ct = default)
    {
        if(context is null) throw new ArgumentNullException(nameof(context));
        if(suggested is null) return (false, "Missing suggestion payload.", null, null);

        if(!entityId.HasValue || entityId.Value <= 0)
            return (false, "SoundSynth video suggestions must reference an existing sound synth via entity_id.", null,
                    null);

        int synthId = (int)entityId.Value;
        bool synthExists = await context.SoundSynths.AsNoTracking().AnyAsync(p => p.Id == synthId);
        if(!synthExists) return (false, $"SoundSynth #{synthId} not found.", null, null);

        // ── URL (mandatory) ────────────────────────────────────────────────────
        if(!suggested.TryGetValue(FieldVideoUrl, out object urlRaw) || urlRaw is null)
            return (false, "A sound synth video suggestion must include a 'video_url' field.", null, null);

        string url = CoerceString(urlRaw)?.Trim();

        if(string.IsNullOrEmpty(url))
            return (false, "'video_url' cannot be empty.", null, null);

        if(url.Length > MaxVideoUrlLength)
            return (false, $"'video_url' cannot exceed {MaxVideoUrlLength} characters.", null, null);

        if(!YouTubeUrlParser.TryExtract(url, out string videoId) || string.IsNullOrEmpty(videoId))
            return (false, "This is not a valid YouTube URL or video ID.", null, null);

        // ── Duplicate check against existing SoundSynthVideos ───────────────────
        bool duplicate = await context.SoundSynthVideos.AsNoTracking()
                                      .AnyAsync(v => v.SoundSynthId == synthId   &&
                                                     v.Provider    == ProviderName  &&
                                                     v.VideoId     == videoId);

        if(duplicate)
            return (false, "This video is already linked to this sound synth.", null, null);

        // ── oEmbed title fetch (mandatory) ─────────────────────────────────────
        // Done at submission time so admins reviewing the suggestion see the canonical title
        // verbatim from the start. If oEmbed fails (network error, deleted/private/unlisted
        // video, region-restricted, etc.) we reject the submission with a friendly retry
        // message — better to catch dud URLs at submission than have them clutter the queue.
        string title = await YouTubeOEmbedClient.TryFetchTitleAsync(httpFactory, videoId, ct);

        if(string.IsNullOrWhiteSpace(title))
            return (false,
                    "Could not retrieve video metadata from YouTube. Verify the URL or try again later.",
                    null, null);

        if(title.Length > MaxTitleLength) title = title[..MaxTitleLength];

        return (true, null, videoId, title);
    }

    /// <summary>
    ///     Apply an admin's accept/reject decision. If the admin accepted the
    ///     <see cref="FieldVideoUrl" /> key (the whole suggestion is treated as one unit),
    ///     create a new <see cref="SoundSynthVideo" /> row with <c>Provider = "YouTube"</c>,
    ///     <c>VideoId</c> from the parsed URL, and <c>Title</c> from the oEmbed-fetched
    ///     value (also accepted via the <see cref="FieldTitle" /> key). Otherwise the
    ///     suggestion is fully rejected (no row created, no side-effects). Returns the set
    ///     of accept-keys actually applied (subset of <paramref name="accepted" />) plus a
    ///     flag indicating the parent SoundSynth was missing (so the controller can mark the
    ///     suggestion stale).
    /// </summary>
    public static async Task<(HashSet<string> applied, bool entityMissing)> ApplyAsync(MarechaiContext context,
        long entityId, Dictionary<string, object> suggested, HashSet<string> accepted, string creditedUserId)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);
        if(suggested is null) return (applied, false);

        int synthId        = (int)entityId;
        bool synthExists   = await context.SoundSynths.AnyAsync(p => p.Id == synthId);
        if(!synthExists) return (applied, true);

        // Whole-suggestion gate: admin must have accepted the video_url key for us to create
        // the row. If only `title` is accepted we silently no-op (defensive — UI never lets
        // that combination happen, but we don't want to create an orphan row either way).
        if(!accepted.Contains(FieldVideoUrl)) return (applied, false);

        if(!suggested.TryGetValue(FieldVideoUrl, out object urlRaw) || urlRaw is null) return (applied, false);

        string url = CoerceString(urlRaw)?.Trim();
        if(string.IsNullOrEmpty(url)) return (applied, false);

        if(!YouTubeUrlParser.TryExtract(url, out string videoId) || string.IsNullOrEmpty(videoId))
            return (applied, false);

        // Defensive: re-check duplicate at accept time (a previous accept may have created
        // the same row in the interval between submission and review).
        bool duplicate = await context.SoundSynthVideos.AnyAsync(v => v.SoundSynthId == synthId   &&
                                                                      v.Provider    == ProviderName  &&
                                                                      v.VideoId     == videoId);

        if(duplicate) return (applied, false);

        string title = null;

        if(accepted.Contains(FieldTitle) &&
           suggested.TryGetValue(FieldTitle, out object titleRaw))
        {
            title = CoerceString(titleRaw)?.Trim();

            if(string.IsNullOrEmpty(title)) title = null;
            else if(title.Length > MaxTitleLength) title = title[..MaxTitleLength];
        }

        var video = new SoundSynthVideo
        {
            SoundSynthId = synthId,
            Provider    = ProviderName,
            VideoId     = videoId,
            Title       = title
        };

        await context.SoundSynthVideos.AddAsync(video);
        await context.SaveChangesWithUserAsync(creditedUserId);

        applied.Add(FieldVideoUrl);
        if(title is not null) applied.Add(FieldTitle);

        return (applied, false);
    }

    // ───────────────────────────── helpers ─────────────────────────────

    static string CoerceString(object value)
    {
        return value switch
        {
            null                                                     => null,
            string s                                                 => s,
            JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
            JsonElement je when je.ValueKind == JsonValueKind.Null   => null,
            _                                                        => value.ToString()
        };
    }
}
