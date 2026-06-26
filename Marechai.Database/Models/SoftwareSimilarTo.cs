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

namespace Marechai.Database.Models;

/// <summary>
///     A symmetric "is similar to" link between two <see cref="Software" /> entries
///     (e.g. Microsoft Excel and Microsoft Excel for Macintosh). Each unordered pair is
///     stored as a single row with <see cref="SoftwareId" /> &lt; <see cref="SimilarSoftwareId" />;
///     callers must normalize the ordering before writing and query both columns when reading.
/// </summary>
public class SoftwareSimilarTo
{
    public ulong SoftwareId { get; set; }

    public virtual Software Software { get; set; }

    public ulong SimilarSoftwareId { get; set; }

    public virtual Software SimilarSoftware { get; set; }
}
