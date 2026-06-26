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

namespace Marechai.Translation;

/// <summary>
///     Plug-in contract used by the background <c>TranslationWorker</c> to discover and translate
///     entities for which a DB-backed translation table exists. One implementation per entity type
///     (e.g. <c>SoftwareGenreTranslationProvider</c>, <c>SoftwareAttributeTranslationProvider</c>).
///     The worker iterates registered providers SERIALLY on each tick, but each provider may run
///     its own bounded translation waves based on the server's background-translation settings.
/// </summary>
/// <remarks>
///     Lifecycle on each tick:
///     <list type="number">
///         <item><see cref="EnsureCacheLoadedAsync" /> — first tick only; subsequent ticks are no-ops once loaded.</item>
///         <item><see cref="DiscoverNewItemsAsync" /> — delta query for entities added since last tick.</item>
///         <item><see cref="TranslateMissingAsync" /> — once per non-<c>eng</c> language, in order.</item>
///     </list>
/// </remarks>
public interface ITranslationProvider
{
    /// <summary>Short diagnostic name (e.g. <c>"SoftwareGenre"</c>) used in worker log messages.</summary>
    string Name { get; }

    /// <summary>
    ///     Make sure the in-memory cache backing this provider has been loaded from the DB. Idempotent
    ///     and thread-safe; the second concurrent caller waits and returns immediately.
    /// </summary>
    Task EnsureCacheLoadedAsync(CancellationToken ct);

    /// <summary>
    ///     Discovers entities added to the DB since the last tick (typically via a delta query
    ///     <c>WHERE Id &gt; LastSeenId</c>) and registers them in the cache so the next
    ///     <see cref="TranslateMissingAsync" /> sweep picks them up. Returns the number of newly
    ///     discovered items so the worker can log a summary.
    /// </summary>
    Task<int> DiscoverNewItemsAsync(CancellationToken ct);

    /// <summary>
    ///     For the given non-<c>eng</c> language, snapshot the items that lack a translation, call the
    ///     <c>TranslationService</c> in bounded waves, then bulk-insert the resulting rows. Returns
    ///     the number of rows inserted (may be 0 if every item is already translated or every
    ///     translation call failed).
    /// </summary>
    Task<int> TranslateMissingAsync(string languageCode, CancellationToken ct);
}
