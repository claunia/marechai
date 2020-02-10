namespace Marechai.Database.Models
{
    public class OwnedMachinePhoto : BasePhoto
    {
        public virtual OwnedMachine OwnedMachine   { get; set; }
        public         long         OwnedMachineId { get; set; }
    }
}