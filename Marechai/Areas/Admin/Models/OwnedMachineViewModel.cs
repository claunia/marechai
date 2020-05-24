using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Marechai.Database;

namespace Marechai.Areas.Admin.Models
{
    public class OwnedMachineViewModel : BaseViewModel<long>
    {
        [DataType(DataType.Date), DisplayName("Acquired")]
        public DateTime AcquisitionDate { get; set; }
        public StatusType Status  { get;       set; }
        public string     Machine { get;       set; }
        public string     User    { get;       set; }

        [DisplayName("Date when sold, traded, or otherwise lost")]
        public DateTime? LostDate { get; set; }
        [DisplayName("Last status check date")]
        public DateTime? LastStatusDate { get; set; }
        [DisplayName("Available for trade or sale")]
        public bool Trade { get; set; }
        [DisplayName("Has original boxes")]
        public bool Boxed { get; set; }
        [DisplayName("Has original manuals")]
        public bool Manuals { get; set; }
        [DisplayName("Serial number")]
        public string SerialNumber { get; set; }
        [DisplayName("Serial number visible to other users")]
        public bool SerialNumberVisible { get; set; }

        public string LostDateDisplay       => LostDate?.ToLongDateString()       ?? "Never";
        public string LastStatusDateDisplay => LastStatusDate?.ToLongDateString() ?? "Never";
    }
}