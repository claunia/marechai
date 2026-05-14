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

namespace Marechai.Data;

/// <summary>
///     Identifies the source entity type for a row in the <c>SearchEntries</c> denormalized
///     full-text/fuzzy search index.
/// </summary>
public enum SearchEntityType : byte
{
    Company             = 1,
    Computer            = 2,
    Console             = 3,
    Smartphone          = 4,
    Book                = 5,
    Document            = 6,
    Magazine            = 7,
    Gpu                 = 8,
    Processor           = 9,
    SoundSynth          = 10,
    Person              = 11,
    Software            = 12,
    SoftwareCompilation = 13,
    Pda                 = 14
}
