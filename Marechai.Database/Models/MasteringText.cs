using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class MasteringText : BaseModel<ulong>
    {
        public MasteringTextType Type { get; set; }
        [Required]
        public string Text { get;  set; }
        public short? Side  { get; set; }
        public short? Layer { get; set; }
        [Required]
        public virtual Media Media { get; set; }
    }
}