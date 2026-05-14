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
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Marechai.Server.Helpers;

/// <summary>
///     Best-effort fetcher for YouTube video titles via the public oEmbed endpoint
///     (<c>https://www.youtube.com/oembed?url=...&amp;format=json</c>).
///     <para>
///         Used by the GpuVideo collaborative-suggestion validator to populate the canonical
///         video title server-side at submission time so admins reviewing the suggestion see
///         meaningful context (rather than an opaque 11-character ID). Failures are silent —
///         the caller treats a null/empty return as "metadata unavailable" and rejects the
///         submission with a user-friendly retry message.
///     </para>
/// </summary>
public static class YouTubeOEmbedClient
{
    /// <summary>Logical name for the registered <see cref="HttpClient" /> in DI.</summary>
    public const string HttpClientName = "youtube-oembed";

    /// <summary>Maximum length we'll keep from oEmbed (defensive — YouTube titles are short).</summary>
    public const int MaxTitleLength = 512;

    /// <summary>
    ///     Attempt to fetch the canonical title for the supplied YouTube video ID. Returns
    ///     <c>null</c> on any failure (network, HTTP non-success, malformed JSON, missing
    ///     <c>title</c> field, request-cancelled). Truncates to <see cref="MaxTitleLength" />.
    /// </summary>
    public static async Task<string> TryFetchTitleAsync(IHttpClientFactory factory, string videoId,
                                                        CancellationToken ct = default)
    {
        if(factory is null || string.IsNullOrWhiteSpace(videoId)) return null;

        try
        {
            HttpClient http = factory.CreateClient(HttpClientName);

            // Build the oEmbed URL. We pass a canonical watch?v= URL because oEmbed expects a
            // public-facing URL, not a bare ID.
            string url = "https://www.youtube.com/oembed?url=https%3A//www.youtube.com/watch%3Fv%3D" +
                         Uri.EscapeDataString(videoId) +
                         "&format=json";

            using HttpResponseMessage resp = await http.GetAsync(url, ct).ConfigureAwait(false);

            if(!resp.IsSuccessStatusCode) return null;

            await using System.IO.Stream stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using JsonDocument             doc    = await JsonDocument.ParseAsync(stream, default, ct).ConfigureAwait(false);

            if(!doc.RootElement.TryGetProperty("title", out JsonElement titleEl) ||
               titleEl.ValueKind != JsonValueKind.String)
                return null;

            string title = titleEl.GetString();

            if(string.IsNullOrWhiteSpace(title)) return null;

            title = title.Trim();

            if(title.Length > MaxTitleLength) title = title[..MaxTitleLength];

            return title;
        }
        catch
        {
            // ignored — best-effort
            return null;
        }
    }
}
