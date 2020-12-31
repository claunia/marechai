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

namespace Marechai.Database.Models
{
    public class Filesystem : BaseModel<ulong>
    {
        [Required]
        public string Type { get;                      set; }
        public DateTime? CreationDate           { get; set; }
        public DateTime? ModificationDate       { get; set; }
        public DateTime? BackupDate             { get; set; }
        public int       ClusterSize            { get; set; }
        public ulong     Clusters               { get; set; }
        public ulong?    FilesCount             { get; set; }
        public bool      Bootable               { get; set; }
        public string    Serial                 { get; set; }
        public string    Label                  { get; set; }
        public ulong?    FreeClusters           { get; set; }
        public DateTime? ExpirationDate         { get; set; }
        public DateTime? EffectiveDate          { get; set; }
        public string    SystemIdentifier       { get; set; }
        public string    VolumeSetIdentifier    { get; set; }
        public string    PublisherIdentifier    { get; set; }
        public string    DataPreparerIdentifier { get; set; }
        public string    ApplicationIdentifier  { get; set; }

        public virtual ICollection<FilesystemsByLogicalPartition> Partitions          { get; set; }
        public virtual ICollection<FilesystemsByMediaDumpFile>    MediaDumpFileImages { get; set; }
        public virtual ICollection<FilesByFilesystem>             Files               { get; set; }
    }
}