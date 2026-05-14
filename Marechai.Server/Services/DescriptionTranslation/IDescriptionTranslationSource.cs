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

using System.Threading;
using System.Threading.Tasks;

namespace Marechai.Server.Services.DescriptionTranslation;

/// <summary>
///     One missing-translation candidate as returned by
///     <see cref="IDescriptionTranslationSource.FetchNextMissingAsync" />. The entity-id type is
///     boxed as <see cref="object" /> because Marechai's description / synopsis tables use a mix
///     of <c>int</c>, <c>long</c> and <c>ulong</c> primary keys; each source owns the cast on save.
/// </summary>
public sealed record DescriptionTranslationItem(object EntityId, string LanguageCode, string SourceMarkdown);

/// <summary>
///     Per-entity adapter consumed by <see cref="DescriptionTranslationWorker" />. One implementation
///     per description / synopsis table (<c>SoftwareDescription</c>, <c>BookSynopsis</c>, etc.).
///     The worker iterates every registered source round-robin × non-<c>eng</c> language, calling
///     <see cref="FetchNextMissingAsync" /> + <see cref="SaveAsync" /> one row at a time.
/// </summary>
/// <remarks>
///     Every method opens its own <c>AsyncServiceScope</c> internally so the source can be
///     registered as a singleton. The save path deliberately ignores any slumber CT so an
///     in-progress translation always lands in the DB before the worker yields back to the
///     primary translation worker.
/// </remarks>
public interface IDescriptionTranslationSource
{
    /// <summary>Short diagnostic name (e.g. <c>"SoftwareDescription"</c>) used in worker log messages.</summary>
    string Name { get; }

    /// <summary>
    ///     <c>true</c> for description tables that carry both <c>Text</c> (markdown) and <c>Html</c>
    ///     columns; <c>false</c> for synopsis tables that store only the markdown text.
    /// </summary>
    bool RendersHtml { get; }

    /// <summary>
    ///     Anti-join query: find one entity that has an English (<c>eng</c>) row in this table but
    ///     no row for <paramref name="languageCode" />. Returns <c>null</c> when nothing is missing.
    /// </summary>
    Task<DescriptionTranslationItem> FetchNextMissingAsync(string languageCode, CancellationToken ct);

    /// <summary>
    ///     Persist the translated markdown (and rendered HTML for description tables) to the DB.
    ///     <paramref name="translatedHtml" /> is ignored when <see cref="RendersHtml" /> is
    ///     <c>false</c>. The save runs in its own scope and SHOULD complete even if the slumber CT
    ///     fires mid-call — the caller does not pass that token here.
    /// </summary>
    Task SaveAsync(DescriptionTranslationItem item, string translatedMarkdown, string translatedHtml,
                   CancellationToken          ct);
}
