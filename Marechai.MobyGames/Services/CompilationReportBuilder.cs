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

using System.Collections.Generic;
using System.Text;
using Marechai.MobyGames.Models;

namespace Marechai.MobyGames.Services;

/// <summary>
///     Builds the Markdown subject + body for the system message sent to admins
///     when a MobyGames compilation is imported (or retroactively converted) with
///     missing contained games.
/// </summary>
internal static class CompilationReportBuilder
{
    public static string BuildSubject(string compilationName) =>
        $"Partial compilation import: {compilationName}";

    /// <summary>
    ///     Composes the Markdown body. <paramref name="resolvedGames" /> is the list of contained
    ///     games that were successfully linked; <paramref name="unresolvedSlugs" /> are MobyGames
    ///     slugs that exist as anchors but couldn't be looked up locally; <paramref name="unresolvableLinks" />
    ///     are anchors that don't even point at a MobyGames game entry (search URLs, external links, etc.).
    /// </summary>
    public static string BuildBody(string compilationName, string mobyGameId,
                                   IReadOnlyCollection<(ulong Id, string Name)> resolvedGames,
                                   IReadOnlyCollection<string> unresolvedSlugs,
                                   IReadOnlyCollection<UnresolvableCompilationLink> unresolvableLinks,
                                   bool compilationCreated)
    {
        var sb = new StringBuilder();

        int resolvedCount     = resolvedGames?.Count     ?? 0;
        int unresolvedCount   = unresolvedSlugs?.Count   ?? 0;
        int unresolvableCount = unresolvableLinks?.Count ?? 0;

        if(compilationCreated)
        {
            sb.Append("The MobyGames importer created compilation **").Append(compilationName)
              .Append("** with ").Append(resolvedCount).Append(" of ")
              .Append(resolvedCount + unresolvedCount + unresolvableCount)
              .AppendLine(" contained games linked.");
        }
        else
        {
            sb.Append("The MobyGames importer could **not** create compilation **").Append(compilationName)
              .AppendLine("** because none of its contained games could be linked.");
        }

        sb.AppendLine();
        sb.Append("- Resolved: ").Append(resolvedCount).AppendLine();
        sb.Append("- Unresolved MobyGames slugs: ").Append(unresolvedCount).AppendLine();
        sb.Append("- Search / non-game anchors: ").Append(unresolvableCount).AppendLine();
        sb.AppendLine();

        if(!string.IsNullOrWhiteSpace(mobyGameId))
        {
            string normalizedSlug = mobyGameId.TrimStart('-');

            sb.Append("Source: [https://www.mobygames.com/game/").Append(normalizedSlug)
              .Append("](https://www.mobygames.com/game/").Append(normalizedSlug).AppendLine(")");
            sb.AppendLine();
        }

        if(resolvedCount > 0)
        {
            sb.AppendLine("### Linked games");

            foreach((ulong id, string name) in resolvedGames!)
            {
                sb.Append("- [").Append(id).Append("] ").AppendLine(name ?? "(unnamed)");
            }

            sb.AppendLine();
        }

        if(unresolvedCount > 0)
        {
            sb.AppendLine("### MobyGames slugs not in the local database");
            sb.AppendLine();
            sb.AppendLine("These anchors point at a real MobyGames game entry but we don't have it imported yet. " +
                          "Re-run the importer after scraping the missing games to attach them.");
            sb.AppendLine();

            foreach(string slug in unresolvedSlugs!)
            {
                string normalized = slug?.TrimStart('-') ?? "";
                sb.Append("- [`").Append(slug).Append("`](https://www.mobygames.com/game/")
                  .Append(normalized).AppendLine(")");
            }

            sb.AppendLine();
        }

        if(unresolvableCount > 0)
        {
            sb.AppendLine("### Anchors with no MobyGames entry");
            sb.AppendLine();
            sb.AppendLine("These anchors point at search URLs or non-game pages — the contained game has no MobyGames " +
                          "entry to import. Investigate manually and either create the missing game record on " +
                          "MobyGames + re-scrape, or attach the closest match through `/admin/software`.");
            sb.AppendLine();

            foreach(UnresolvableCompilationLink link in unresolvableLinks!)
            {
                sb.Append("- **").Append(link.Name).Append("** — [")
                  .Append(link.Href ?? "(no href)").Append("](")
                  .Append(link.Href ?? "#").AppendLine(")");
            }

            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine("Manage compilations at [/admin/software](/admin/software).");

        return sb.ToString();
    }
}
