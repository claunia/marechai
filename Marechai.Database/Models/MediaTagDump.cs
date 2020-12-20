using System.ComponentModel.DataAnnotations;
using Aaru.CommonTypes.Enums;

namespace Marechai.Database.Models
{
    public class MediaTagDump : BaseModel<ulong>
    {
        [Required]
        public virtual MediaDump MediaDump { get; set; }
        public MediaTagType Type { get;           set; }
        [Required]
        public virtual DbFile File { get; set; }

        public ulong FileId { get; set; }
    }
}