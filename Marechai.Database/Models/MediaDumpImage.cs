using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class MediaDumpImage : BaseModel<ulong>
    {
        public ulong MediaDumpId { get; set; }
        [Required]
        public virtual MediaDump MediaDump { get; set; }
        public ulong Size { get;                  set; }
        [Column(TypeName = "binary(16)")]
        public string Md5 { get; set; }
        [Column(TypeName = "binary(20)")]
        public string Sha1 { get; set; }
        [Column(TypeName = "binary(32)")]
        public string Sha256 { get; set; }
        [Column(TypeName = "binary(64)")]
        public string Sha3 { get;      set; }
        public string Spamsum   { get; set; }
        public string AccoustId { get; set; }
    }
}