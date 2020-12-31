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
using System.ComponentModel;

namespace Marechai.Database.Models
{
    public class PeopleByCompany : BaseModel<long>
    {
        public int       PersonId  { get; set; }
        public int       CompanyId { get; set; }
        public string    Position  { get; set; }
        public DateTime? Start     { get; set; }
        public DateTime? End       { get; set; }
        [DefaultValue(false)]
        public bool Ongoing { get; set; }

        public virtual Person  Person  { get; set; }
        public virtual Company Company { get; set; }
    }
}