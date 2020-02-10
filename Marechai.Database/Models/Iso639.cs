using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    /// <summary>ISO-639 codes</summary>
    public class Iso639
    {
        [Column(TypeName = "char(3)"), Key, Required]
        public string Id { get; set; }
        [Column(TypeName = "char(3)")]
        public string Part2B { get; set; }
        [Column(TypeName = "char(3)")]
        public string Part2T { get; set; }
        [Column(TypeName = "char(2)")]
        public string Part1 { get; set; }
        [Column(TypeName = "char(1)"), Required]
        public string Scope { get; set; }
        [Column(TypeName = "char(1)"), Required]
        public string Type { get; set; }
        [Column(TypeName = "varchar(150)"), Required]
        public string ReferenceName { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string Comment { get; set; }
    }
}