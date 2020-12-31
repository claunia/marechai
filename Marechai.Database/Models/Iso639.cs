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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    /// <summary>ISO-639 codes</summary>
    public class Iso639
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedOn { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedOn { get; set; }

        [Column(TypeName = "char(3)"), Key, Required]
        public string Id { get; set; }
        [Column(TypeName = "char(3)")]
        public string Part2B { get; set; }
        [Column(TypeName = "char(3)")]
        public string Part2T { get; set; }
        [Column(TypeName = "char(2)")]
        public string Part1 { get; set; }
        [Column(TypeName = "char(1)"), Required]
        public string Scope { get; set; }
        [Column(TypeName = "char(1)"), Required]
        public string Type { get; set; }
        [Column(TypeName = "varchar(150)"), Required]
        public string ReferenceName { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string Comment { get; set; }

        public virtual ICollection<LanguagesBySoftwareVariant> Software { get; set; }
    }
}