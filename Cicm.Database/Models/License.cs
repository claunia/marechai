using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class License : BaseModel<int>
    {
        [Required]
        public string Name { get; set; }
        [DisplayName("SPDX identifier")]
        public string SPDX { get; set; }
        [DisplayName("FSF approved")]
        [Required]
        public bool FsfApproved { get; set; }
        [DisplayName("OSI approved")]
        [Required]
        public bool OsiApproved { get; set; }
        [DisplayName("License text link")]
        [StringLength(512)]
        public string Link { get; set; }
        [DisplayName("License text")]
        [Column(TypeName = "longtext")]
        [StringLength(131072)]
        public string Text { get; set; }
    }
}