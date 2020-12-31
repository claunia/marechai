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
using System.ComponentModel;

namespace Marechai.Database.Models
{
    public class OwnedMachine : BaseModel<long>
    {
        [DisplayName("Acquisition date")]
        public DateTime AcquisitionDate { get; set; }
        [DisplayName("Date when sold, traded, or otherwise lost")]
        public DateTime? LostDate { get; set; }
        public StatusType Status { get;  set; }
        [DisplayName("Last status check date")]
        public DateTime? LastStatusDate { get; set; }
        [DisplayName("Available for trade or sale"), DefaultValue(false)]
        public bool Trade { get; set; }
        [DisplayName("Has original boxes"), DefaultValue(false)]
        public bool Boxed { get; set; }
        [DisplayName("Has original manuals"), DefaultValue(false)]
        public bool Manuals { get; set; }
        [DisplayName("Serial number")]
        public string SerialNumber { get; set; }
        [DisplayName("Serial number visible to other users"), DefaultValue(false)]
        public bool SerialNumberVisible { get; set; }
        public int    MachineId { get;         set; }
        public string UserId    { get;         set; }

        public virtual ICollection<GpusByOwnedMachine>       Gpus       { get; set; }
        public virtual ICollection<MemoryByOwnedMachine>     Memory     { get; set; }
        public virtual ICollection<ProcessorsByOwnedMachine> Processors { get; set; }
        public virtual ICollection<SoundByOwnedMachine>      Sound      { get; set; }
        public virtual ICollection<StorageByOwnedMachine>    Storage    { get; set; }
        public virtual ICollection<OwnedMachinePhoto>        Photos     { get; set; }

        public virtual ApplicationUser User    { get; set; }
        public virtual Machine         Machine { get; set; }
    }
}