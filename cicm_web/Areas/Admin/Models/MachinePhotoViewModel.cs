using System;
using System.ComponentModel;

namespace cicm_web.Areas.Admin.Models
{
    public class MachinePhotoViewModel : BaseViewModel<Guid>
    {
        public string Author  { get; set; }
        public string License { get; set; }
        public string Machine { get; set; }
        [DisplayName("Uploaded")]
        public DateTime UploadDate { get; set; }
        [DisplayName("Uploaded by")]
        public string UploadUser { get; set; }
    }
}