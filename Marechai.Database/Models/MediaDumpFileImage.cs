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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class MediaDumpFileImage : BaseModel<ulong>
    {
        [Required]
        public virtual MediaDump MediaDump { get; set; }
        public long  FileSequence      { get;     set; }
        public short PartitionSequence { get;     set; }
        public ulong Size              { get;     set; }
        [Column(TypeName = "binary(16)")]
        public string Md5 { get; set; }
        [Column(TypeName = "binary(20)")]
        public string Sha1 { get; set; }
        [Column(TypeName = "binary(32)")]
        public string Sha256 { get; set; }
        [Column(TypeName = "binary(64)")]
        public string Sha3 { get;    set; }
        public string Spamsum { get; set; }

        public virtual ICollection<FilesystemsByMediaDumpFile> Filesystems { get; set; }
    }
}