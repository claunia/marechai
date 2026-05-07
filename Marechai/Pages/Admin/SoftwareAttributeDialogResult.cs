/*******************************************************************************
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

namespace Marechai.Pages.Admin;

public sealed class SoftwareAttributeDialogResult
{
    public ulong  SoftwareReleaseId { get; set; }
    public string Category          { get; set; }
    public string Key               { get; set; }
    public string Value             { get; set; }

    /// <summary>True when the dialog committed a split operation rather than a normal save.</summary>
    public bool SplitApplied { get; set; }

    /// <summary>Human-readable summary used as the page-level success message after a split.</summary>
    public string Summary { get; set; }
}
