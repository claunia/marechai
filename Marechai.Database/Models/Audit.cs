using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class Audit : BaseModel<long>
    {
        public AuditType Type { get; set; }
        [Required]
        public string UserId { get;                                          set; }
        public string                                 Table           { get; set; }
        public JsonObject<Dictionary<string, object>> Keys            { get; set; }
        public JsonObject<Dictionary<string, object>> OldValues       { get; set; }
        public JsonObject<Dictionary<string, object>> NewValues       { get; set; }
        public JsonObject<List<string>>               AffectedColumns { get; set; }

        [Required]
        public virtual ApplicationUser User { get; set; }
    }
}