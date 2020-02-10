using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
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
        [Url]
        public string Link { get; set; }
        [DisplayName("License text")]
        [Column(TypeName = "longtext")]
        [StringLength(131072)]
        [DataType(DataType.MultilineText)]
        public string Text { get;                                               set; }
        public virtual ICollection<MachinePhoto>      Photos             { get; set; }
        public virtual ICollection<OwnedMachinePhoto> OwnedMachinePhotos { get; set; }
    }
}