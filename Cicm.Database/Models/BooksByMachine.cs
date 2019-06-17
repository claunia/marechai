namespace Cicm.Database.Models
{
    public class BooksByMachine : BaseModel<long>
    {
        public long BookId    { get; set; }
        public int  MachineId { get; set; }

        public virtual Book    Book    { get; set; }
        public virtual Machine Machine { get; set; }
    }
}