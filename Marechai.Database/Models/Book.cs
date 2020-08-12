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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class Book : DocumentBase
    {
        [StringLength(13, MinimumLength = 10)]
        public string Isbn { get;       set; }
        public short? Pages      { get; set; }
        public int?   Edition    { get; set; }
        public long?  PreviousId { get; set; }
        public long?  SourceId   { get; set; }

        public virtual Book                              Previous        { get; set; }
        public virtual Book                              Source          { get; set; }
        public virtual Book                              Next            { get; set; }
        public virtual Iso31661Numeric                   Country         { get; set; }
        public virtual ICollection<Book>                 Derivates       { get; set; }
        public virtual ICollection<CompaniesByBook>      Companies       { get; set; }
        public virtual ICollection<PeopleByBook>         People          { get; set; }
        public virtual ICollection<BooksByMachine>       Machines        { get; set; }
        public virtual ICollection<BooksByMachineFamily> MachineFamilies { get; set; }
        public virtual ICollection<BookScan>             Scans           { get; set; }
    }
}