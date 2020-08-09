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
    public class SoftwareVariant : BaseModel<ulong>
    {
        public         string          Name       { get; set; }
        public         string          Version    { get; set; }
        public         DateTime?       Introduced { get; set; }
        public virtual SoftwareVariant Parent     { get; set; }
        [Required]
        public virtual SoftwareVersion SoftwareVersion { get; set; }
        public ulong?           MinimumMemory     { get;      set; }
        public ulong?           RecommendedMemory { get;      set; }
        public ulong?           RequiredStorage   { get;      set; }
        public string           PartNumber        { get;      set; }
        public string           SerialNumber      { get;      set; }
        public string           ProductCode       { get;      set; }
        public string           CatalogueNumber   { get;      set; }
        public DistributionMode DistributionMode  { get;      set; }

        public virtual ICollection<SoftwareVariant>                          Derivates                { get; set; }
        public virtual ICollection<CompaniesBySoftwareVariant>               Companies                { get; set; }
        public virtual ICollection<GpusBySoftwareVariant>                    Gpus                     { get; set; }
        public virtual ICollection<InstructionSetsBySoftwareVariant>         Architectures            { get; set; }
        public virtual ICollection<LanguagesBySoftwareVariant>               Languages                { get; set; }
        public virtual ICollection<MachineFamiliesBySoftwareVariant>         MachineFamilies          { get; set; }
        public virtual ICollection<MachinesBySoftwareVariant>                Machines                 { get; set; }
        public virtual ICollection<MediaBySoftwareVariant>                   Media                    { get; set; }
        public virtual ICollection<PeopleBySoftwareVariant>                  People                   { get; set; }
        public virtual ICollection<ProcessorsBySoftwareVariant>              Processors               { get; set; }
        public virtual ICollection<RequiredOperatingSystemsBySofwareVariant> RequiredOperatingSystems { get; set; }
        public virtual ICollection<RequiredSoftwareBySoftwareVariant>        RequiredSoftware         { get; set; }
        public virtual ICollection<SoundBySoftwareVariant>                   SupportedSound           { get; set; }
        public virtual ICollection<StandaloneFile>                           Files                    { get; set; }
        public         ulong?                                                ParentId                 { get; set; }
        public         ulong                                                 SoftwareVersionId        { get; set; }
    }
}