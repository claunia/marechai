namespace Marechai.Database.Models
{
    public class MagazinesByMachine : BaseModel<long>
    {
        public long MagazineId { get; set; }
        public int  MachineId  { get; set; }

        public virtual MagazineIssue Magazine { get; set; }
        public virtual Machine       Machine  { get; set; }
    }
}