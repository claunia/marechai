using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class Audit : BaseModel<long>
    {
        public AuditType Type { get; set; }
        [Required]
        public string UserId { get; set; }
        public string Table { get;  set; }
        [Column(TypeName = "json")]
        public Dictionary<string, object> Keys { get; set; }
        [Column(TypeName = "json")]
        public Dictionary<string, object> OldValues { get; set; }
        [Column(TypeName = "json")]
        public Dictionary<string, object> NewValues { get; set; }
        [Column(TypeName = "json")]
        public List<string> AffectedColumns { get; set; }

        [Required]
        public virtual ApplicationUser User { get; set; }
    }
}