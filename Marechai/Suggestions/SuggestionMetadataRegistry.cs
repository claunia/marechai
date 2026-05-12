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
using Marechai.Data;

namespace Marechai.Suggestions;

/// <summary>
///     Static registry mapping <see cref="SuggestionEntityType" /> to the corresponding
///     <see cref="SuggestionMetadata" /> instance. Per-entity classes call
///     <see cref="Register" /> from their static constructor (or the registry calls them
///     lazily via <see cref="EnsureSeeded" />).
/// </summary>
public static class SuggestionMetadataRegistry
{
    static readonly Dictionary<SuggestionEntityType, SuggestionMetadata> s_byType = new();
    static          bool                                                  s_seeded;
    static readonly object                                                s_lock   = new();

    public static void Register(SuggestionMetadata metadata)
    {
        if(metadata is null) return;

        lock(s_lock) s_byType[metadata.EntityType] = metadata;
    }

    public static SuggestionMetadata Get(SuggestionEntityType entityType)
    {
        EnsureSeeded();
        return s_byType.TryGetValue(entityType, out SuggestionMetadata m) ? m : null;
    }

    /// <summary>
    ///     Ensures the registry has been populated by triggering the static constructor of every
    ///     entity-metadata class. Phase 1 adds the call to <c>CompanySuggestionMetadata.EnsureRegistered()</c>;
    ///     subsequent phases would chain similar calls here.
    /// </summary>
    static void EnsureSeeded()
    {
        if(s_seeded) return;

        lock(s_lock)
        {
            if(s_seeded) return;
            // Phase 1+: each entity metadata's static initializer registers itself.
            // Calling .EnsureRegistered() from per-entity classes triggers their static ctor,
            // but for safety we also call it here so the registry is hydrated even if the
            // per-entity class hasn't been touched anywhere else in the process.
            Marechai.Suggestions.Metadata.CompanySuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.CompanyDescriptionSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.MachineDescriptionSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.BookSynopsisSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.DocumentSynopsisSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.MagazineSynopsisSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.GpuDescriptionSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.ProcessorDescriptionSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.SoundSynthDescriptionSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.PersonDescriptionSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.SoftwareDescriptionSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.MachineSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.BookSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.DocumentSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.MagazineSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.MagazineIssueSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.GpuSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.ProcessorSuggestionMetadata.EnsureRegistered();
            Marechai.Suggestions.Metadata.SoundSynthSuggestionMetadata.EnsureRegistered();
            s_seeded = true;
        }
    }
}
