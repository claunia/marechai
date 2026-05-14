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
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

/// <summary>
///     One translated <see cref="SoftwareScreenshot.Caption" /> per (<see cref="ScreenshotId" />,
///     <see cref="LanguageCode" />) pair. English (<c>eng</c>) is treated as the identity copy of
///     <see cref="SoftwareScreenshot.Caption" /> and is NEVER stored here — read endpoints fall back
///     to the parent <c>Caption</c> column when no translation row exists for the requested
///     language. The background <c>TranslationWorker</c> populates this table by calling OpenAI /
///     NLLB; rows are append-only and are bulk-deleted whenever the canonical caption changes.
/// </summary>
public class SoftwareScreenshotCaptionTranslation : BaseModel<int>
{
    public Guid ScreenshotId { get; set; }

    [StringLength(3)]
    [Required]
    public string LanguageCode { get; set; }

    [Required]
    public string Caption { get; set; }

    public virtual SoftwareScreenshot Screenshot { get; set; }
    public virtual Iso639             Language   { get; set; }
}
