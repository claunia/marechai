using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

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
        [NotMapped]
        [Required(ErrorMessage = "Image file required")]
        [DisplayName("Upload photo:")]
        public IFormFile Photo { get;     set; }
        public int    MachineId    { get; set; }
        public int    LicenseId    { get; set; }
        public string ErrorMessage { get; set; }
        [Url]
        public string Source { get; set; }
    }
}