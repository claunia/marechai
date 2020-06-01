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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Marechai.Database.Models
{
    public class Company : BaseModel<int>
    {
        public Company()
        {
            Logos                   = new HashSet<CompanyLogo>();
            Gpus                    = new HashSet<Gpu>();
            InverseSoldToNavigation = new HashSet<Company>();
            MachineFamilies         = new HashSet<MachineFamily>();
            Machines                = new HashSet<Machine>();
            Processors              = new HashSet<Processor>();
            SoundSynths             = new HashSet<SoundSynth>();
        }

        [Required]
        public string Name { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true), DataType(DataType.Date)]
        public DateTime? Founded { get; set; }
        [Url, StringLength(255)]
        public string Website { get; set; }
        [StringLength(45)]
        public string Twitter { get; set; }
        [StringLength(45)]
        public string Facebook { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}"), DataType(DataType.Date)]
        public DateTime? Sold { get; set; }
        public int? SoldToId { get;  set; }
        [StringLength(80)]
        public string Address { get; set; }
        [StringLength(80)]
        public string City { get; set; }
        [StringLength(80)]
        public string Province { get; set; }
        [StringLength(25), DisplayName("Postal code")]
        public string PostalCode { get; set; }
        public short? CountryId { get;  set; }
        [Required]
        public CompanyStatus Status { get;   set; }
        public int? DocumentCompanyId { get; set; }

        public virtual Iso31661Numeric Country { get; set; }
        [DisplayName("Sold to")]
        public virtual Company SoldTo { get; set; }
        public virtual ICollection<CompanyDescription> Descriptions { get; set; }
        public virtual ICollection<CompanyLogo> Logos { get; set; }
        public virtual ICollection<Gpu> Gpus { get; set; }
        public virtual ICollection<Company> InverseSoldToNavigation { get; set; }
        public virtual ICollection<MachineFamily> MachineFamilies { get; set; }
        public virtual ICollection<Machine> Machines { get; set; }
        public virtual ICollection<Processor> Processors { get; set; }
        public virtual ICollection<SoundSynth> SoundSynths { get; set; }
        public virtual ICollection<PeopleByCompany> People { get; set; }
        public virtual CompanyLogo LastLogo => Logos?.OrderByDescending(l => l.Year).FirstOrDefault();
        public virtual DocumentCompany DocumentCompany { get; set; }
    }
}