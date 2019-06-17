namespace Cicm.Database.Models
{
    public class MagazinesByMachineFamily : BaseModel<long>
    {
        public long MagazineId      { get; set; }
        public int  MachineFamilyId { get; set; }

        public virtual MagazineIssue Magazine      { get; set; }
        public virtual MachineFamily MachineFamily { get; set; }
    }
}