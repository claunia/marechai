using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Cicm.Database;

namespace cicm_web.Areas.Admin.Models
{
    public class OwnedMachineViewModel : BaseViewModel<long>
    {
        [DataType(DataType.Date)]
        [DisplayName("Acquired")]
        public DateTime   AcquisitionDate     { get; set; }
        public StatusType Status              { get; set; }
                public string Machine           { get; set; }
                public string User { get; set; }
    }
}