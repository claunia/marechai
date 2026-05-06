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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Marechai.Data;

namespace Marechai.Database.Models;

public class SoftwareCriticReview : BaseModel<long>
{
    [Required]
    public ulong SoftwareId { get; set; }
    public virtual Software Software { get; set; }

    [Required]
    public long MagazineId { get; set; }
    public virtual Magazine Magazine { get; set; }

    public ulong? PlatformId { get; set; }
    public virtual SoftwarePlatform Platform { get; set; }

    public int? NormalizedScore { get; set; }

    public float? OriginalScore { get; set; }

    public float? OriginalScoreMaximum { get; set; }

    public string ReviewText { get; set; }

    public DateTime? ReviewDate { get; set; }

    [DefaultValue(DatePrecision.Full)]
    public DatePrecision ReviewDatePrecision { get; set; }

    [StringLength(1024)]
    public string ReviewUrl { get; set; }
}
