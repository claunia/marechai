using System;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public abstract class DocumentBase : BaseModel<long>
    {
        [Required]
        public string Title { get;       set; }
        public string NativeTitle { get; set; }
        [DisplayFormat(DataFormatString = "{0:d}"), DataType(DataType.Date)]
        public DateTime? Published { get; set; }
        public short? CountryId { get;    set; }
        [MaxLength(262144, ErrorMessage = "Synopsis is too long")]
        public string Synopsis { get; set; }
    }
}