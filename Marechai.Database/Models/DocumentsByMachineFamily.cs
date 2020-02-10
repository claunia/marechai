namespace Marechai.Database.Models
{
    public class DocumentsByMachineFamily : BaseModel<long>
    {
        public long DocumentId      { get; set; }
        public int  MachineFamilyId { get; set; }

        public virtual Document      Document      { get; set; }
        public virtual MachineFamily MachineFamily { get; set; }
    }
}