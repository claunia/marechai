using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cicm.Database.Models
{
    public class DocumentRole
    {
        [Column(TypeName = "char(3)")]
        [Key]
        [Required]
        public string Id { get;      set; }
        public string Name    { get; set; }
        public bool   Enabled { get; set; }
    }
}