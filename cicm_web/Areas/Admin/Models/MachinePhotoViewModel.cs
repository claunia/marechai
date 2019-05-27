using System;
using System.ComponentModel;

namespace cicm_web.Areas.Admin.Models
{
    public class MachinePhotoViewModel : BaseViewModel<Guid>
    {
        public string Author;
        public string License;
        public string Machine;
        [DisplayName("Uploaded")]
        public DateTime UploadDate;
        [DisplayName("Uploaded by")]
        public string UploadUser;
    }
}