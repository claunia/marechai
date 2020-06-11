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
using Aaru.CommonTypes.Structs;

namespace Marechai.Database.Models
{
    public class MediaFile : BaseModel<ulong>
    {
        [StringLength(8192), Required]
        public string Path { get; set; }
        [StringLength(255), Required]
        public string Name { get; set; }
        [Required, DefaultValue('/')]
        public char PathSeparator { get;              set; }
        public bool           IsDirectory      { get; set; }
        public DateTime?      CreationDate     { get; set; }
        public DateTime?      AccessDate       { get; set; }
        public DateTime?      StatusChangeDate { get; set; }
        public DateTime?      BackupDate       { get; set; }
        public DateTime?      LastWriteDate    { get; set; }
        public FileAttributes Attributes       { get; set; }
        public ushort?        PosixMode        { get; set; }
        public uint?          DeviceNumber     { get; set; }
        public ulong?         GroupId          { get; set; }
        public ulong?         UserId           { get; set; }
        public ulong?         Inode            { get; set; }
        public ulong?         Links            { get; set; }

        public virtual ICollection<FileDataStreamsByMediaFile> DataStreams { get; set; }
    }
}