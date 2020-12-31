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
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public abstract class DocumentBase : BaseModel<long>
    {
        [Required]
        public string Title { get;       set; }
        public string NativeTitle { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}"), DataType(DataType.Date)]
        public DateTime? Published { get; set; }
        public short? CountryId { get;    set; }
        [MaxLength(262144, ErrorMessage = "Synopsis is too long")]
        public string Synopsis { get; set; }
    }
}