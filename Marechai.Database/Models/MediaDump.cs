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

namespace Marechai.Database.Models
{
    public class MediaDump : BaseModel<ulong>
    {
        [Required]
        public virtual Media Media { get; set; }
        public string     Format { get;   set; }
        public DumpStatus Status { get;   set; }

        public virtual ICollection<MediaDumpFileImage>  Files      { get; set; }
        public virtual MediaDumpImage                   Image      { get; set; }
        public virtual MediaDumpSubchannelImage         Subchannel { get; set; }
        public virtual ICollection<MediaDumpTrackImage> Tracks     { get; set; }
        public virtual ICollection<Dump>                Dumps      { get; set; }
        public virtual ICollection<MediaTagDump>        Tags       { get; set; }
    }
}