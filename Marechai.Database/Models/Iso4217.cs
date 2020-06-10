using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marechai.Database.Models
{
    public class Iso4217
    {
        [StringLength(3), Required, Key]
        public string Code { get; set; }
        [Column(TypeName = "smallint(3)"), Required]
        public short Numeric { get;    set; }
        public byte? MinorUnits { get; set; }
        [StringLength(150), Required]
        public string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime? Withdrawn { get; set; }
    }
}