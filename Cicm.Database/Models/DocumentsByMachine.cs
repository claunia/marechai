namespace Cicm.Database.Models
{
    public class DocumentsByMachine : BaseModel<long>
    {
        public long DocumentId { get; set; }
        public int  MachineId  { get; set; }

        public virtual Document Document { get; set; }
        public virtual Machine  Machine  { get; set; }
    }
}