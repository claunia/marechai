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

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class DocumentPerson : BaseModel<int>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get;  set; }
        public int?   PersonId { get; set; }
        public string Alias    { get; set; }
        [DisplayName("Name to be displayed")]
        public string DisplayName { get; set; }

        [NotMapped, DisplayName("Name")]
        public string FullName => DisplayName ?? Alias ?? $"{Name} {Surname}";

        [DisplayName("Linked person")]
        public virtual Person Person { get;                           set; }
        public virtual ICollection<PeopleByDocument> Documents { get; set; }
        public virtual ICollection<PeopleByBook>     Books     { get; set; }
        public virtual ICollection<PeopleByMagazine> Magazines { get; set; }
    }
}