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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class Magazine : DocumentBase
    {
        [StringLength(8, MinimumLength = 8)]
        public string Issn { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}"), DataType(DataType.Date)]
        public DateTime? FirstPublication { get; set; }

        public virtual Iso31661Numeric                  Country   { get; set; }
        public virtual ICollection<MagazineIssue>       Issues    { get; set; }
        public virtual ICollection<CompaniesByMagazine> Companies { get; set; }
    }
}