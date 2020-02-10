using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models
{
    public class MachinePhoto : BasePhoto
    {
        [Url]
        public string Source { get; set; }

        public virtual Machine Machine { get; set; }

        public int MachineId { get; set; }
    }
}