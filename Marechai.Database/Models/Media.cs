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
using Aaru.CommonTypes;

namespace Marechai.Database.Models
{
    public class Media : BaseModel<ulong>
    {
        public string                          Title             { get; set; }
        public ushort?                         Sequence          { get; set; }
        public ushort?                         LastSequence      { get; set; }
        public MediaType                       Type              { get; set; }
        public int?                            WriteOffset       { get; set; }
        public ushort?                         Sides             { get; set; }
        public ushort?                         Layers            { get; set; }
        public ushort?                         Sessions          { get; set; }
        public ushort?                         Tracks            { get; set; }
        public ulong                           Sectors           { get; set; }
        public ulong                           Size              { get; set; }
        public string                          CopyProtection    { get; set; }
        public string                          PartNumber        { get; set; }
        public string                          SerialNumber      { get; set; }
        public string                          Barcode           { get; set; }
        public string                          CatalogueNumber   { get; set; }
        public string                          Manufacturer      { get; set; }
        public string                          Model             { get; set; }
        public string                          Revision          { get; set; }
        public string                          Firmware          { get; set; }
        public int?                            PhysicalBlockSize { get; set; }
        public int?                            LogicalBlockSize  { get; set; }
        public JsonObject<VariableBlockSize[]> BlockSizes        { get; set; }
        public StorageInterface?               StorageInterface  { get; set; }

        public virtual ICollection<LogicalPartitionsByMedia> LogicalPartitions { get; set; }
    }
}