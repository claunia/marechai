namespace Cicm.Database.Models
{
    public class OwnedConsoles
    {
        public int    Id      { get; set; }
        public int    DbId    { get; set; }
        public string Date    { get; set; }
        public int    Status  { get; set; }
        public int    Trade   { get; set; }
        public int    Boxed   { get; set; }
        public int    Manuals { get; set; }
    }
}