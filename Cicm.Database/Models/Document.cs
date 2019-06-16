namespace Cicm.Database.Models
{
    public class Document : DocumentBase
    {
        public virtual Iso31661Numeric Country { get; set; }
    }
}