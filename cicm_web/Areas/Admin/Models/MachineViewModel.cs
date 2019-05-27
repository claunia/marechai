using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Cicm.Database;

namespace cicm_web.Areas.Admin.Models
{
    public class MachineViewModel : BaseViewModel<int>
    {
        [StringLength(255)]
        public string Name { get;      set; }
        public MachineType Type { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}")]
        [DataType(DataType.Date)]
        public DateTime? Introduced { get; set; }
        public string Family { get;        set; }
        [StringLength(50)]
        public string Model { get;   set; }
        public string Company { get; set; }

        [DisplayName("Introduced")]
        public string IntroducedView =>
            Introduced == DateTime.MinValue ? "Prototype" : Introduced?.ToShortDateString() ?? "Unknown";
    }
}