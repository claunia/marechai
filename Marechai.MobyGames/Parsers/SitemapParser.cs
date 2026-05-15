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
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;

namespace Marechai.MobyGames.Parsers;

/// <summary>
///     Streaming parser for the MobyGames sitemap XMLs.
///     <para>
///         The sitemap index (<c>https://www.mobygames.com/sitemap_index.xml</c>) points at ~113
///         sub-sitemap files hosted on the DigitalOcean Spaces CDN
///         (<c>https://sfo3.digitaloceanspaces.com/moby-images/&lt;uuid&gt;</c>). Those sub-sitemaps are
///         served as raw gzipped XML regardless of <c>Accept-Encoding</c>, so we decompress them
///         manually via <see cref="GZipStream" />.
///     </para>
///     <para>
///         Roughly five of the sub-sitemaps (~50,000 entries each) hold the main-game-page URLs
///         (<c>/game/{numericId}/{slug}/</c>) we care about for discovery. The remaining sub-sitemaps
///         contain attributes, companies, persons, groups, critics, and game sub-pages (covers,
///         promo art, screenshots), all of which the existing import / media pipeline does not need
///         from a sitemap.
///     </para>
/// </summary>
public static partial class SitemapParser
{
    [GeneratedRegex(@"^https://www\.mobygames\.com/game/(\d+)/([^/]+)/$", RegexOptions.IgnoreCase)]
    private static partial Regex MainGameUrlRegex();

    public readonly record struct SitemapEntry(string Url, DateTime? LastMod);

    /// <summary>
    ///     Parse the sitemap index and return the list of sub-sitemap URLs in the order the index
    ///     declares them.
    /// </summary>
    public static IReadOnlyList<string> ParseIndex(Stream xml)
    {
        var subSitemaps = new List<string>();

        using var reader = XmlReader.Create(xml, new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments   = true,
            DtdProcessing    = DtdProcessing.Ignore
        });

        while(reader.Read())
        {
            if(reader.NodeType != XmlNodeType.Element || reader.LocalName != "loc") continue;
            string loc = reader.ReadElementContentAsString();
            if(!string.IsNullOrWhiteSpace(loc))
                subSitemaps.Add(loc.Trim());
        }

        return subSitemaps;
    }

    /// <summary>
    ///     Stream-parse a sub-sitemap XML and yield each <c>&lt;url&gt;</c> entry. Caller is responsible
    ///     for <see cref="GZipStream" />-decompressing the bytes before passing the stream in.
    /// </summary>
    public static IEnumerable<SitemapEntry> ParseSubSitemap(Stream xml)
    {
        using var reader = XmlReader.Create(xml, new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments   = true,
            DtdProcessing    = DtdProcessing.Ignore
        });

        string  currentLoc     = null;
        DateTime? currentLastMod = null;

        while(reader.Read())
        {
            if(reader.NodeType == XmlNodeType.Element)
            {
                switch(reader.LocalName)
                {
                    case "url":
                        currentLoc     = null;
                        currentLastMod = null;
                        break;
                    case "loc":
                        currentLoc = reader.ReadElementContentAsString()?.Trim();
                        break;
                    case "lastmod":
                        string lm = reader.ReadElementContentAsString()?.Trim();
                        if(DateTime.TryParse(lm, CultureInfo.InvariantCulture,
                                             DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                             out DateTime parsed))
                            currentLastMod = parsed;
                        break;
                }
            }
            else if(reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "url")
            {
                if(!string.IsNullOrWhiteSpace(currentLoc))
                    yield return new SitemapEntry(currentLoc, currentLastMod);
            }
        }
    }

    /// <summary>
    ///     Match the new-site main game URL shape (<c>https://www.mobygames.com/game/N/slug/</c>) and
    ///     return the (numericId, slug) pair, or <c>null</c> for any other URL shape (sub-pages,
    ///     companies, etc.).
    /// </summary>
    public static (int NumericId, string Slug)? TryExtractGameSlug(string url)
    {
        if(string.IsNullOrWhiteSpace(url)) return null;
        Match m = MainGameUrlRegex().Match(url);
        if(!m.Success) return null;
        if(!int.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
            return null;
        return (id, m.Groups[2].Value);
    }
}
