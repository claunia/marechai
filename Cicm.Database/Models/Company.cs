/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Company.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Describes a company.
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
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Company
    {
        public Company()
        {
            CompanyLogos            = new HashSet<CompanyLogo>();
            Gpus                    = new HashSet<Gpu>();
            InverseSoldToNavigation = new HashSet<Company>();
            MachineFamilies         = new HashSet<MachineFamily>();
            Machines                = new HashSet<Machine>();
            Processors              = new HashSet<Processor>();
            SoundSynths             = new HashSet<SoundSynth>();
        }

        public int           Id         { get; set; }
        public string        Name       { get; set; }
        public DateTime?     Founded    { get; set; }
        public string        Website    { get; set; }
        public string        Twitter    { get; set; }
        public string        Facebook   { get; set; }
        public DateTime?     Sold       { get; set; }
        public int?          SoldToId   { get; set; }
        public string        Address    { get; set; }
        public string        City       { get; set; }
        public string        Province   { get; set; }
        public string        PostalCode { get; set; }
        public short?        CountryId  { get; set; }
        public CompanyStatus Status     { get; set; }

        public virtual Iso31661Numeric            Country                 { get; set; }
        public virtual Company                    SoldTo                  { get; set; }
        public virtual CompanyDescription         Description             { get; set; }
        public virtual ICollection<CompanyLogo>   CompanyLogos            { get; set; }
        public virtual ICollection<Gpu>           Gpus                    { get; set; }
        public virtual ICollection<Company>       InverseSoldToNavigation { get; set; }
        public virtual ICollection<MachineFamily> MachineFamilies         { get; set; }
        public virtual ICollection<Machine>       Machines                { get; set; }
        public virtual ICollection<Processor>     Processors              { get; set; }
        public virtual ICollection<SoundSynth>    SoundSynths             { get; set; }
    }
}