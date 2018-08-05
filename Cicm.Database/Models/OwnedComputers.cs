namespace Cicm.Database.Models
{
    public class OwnedComputers
    {
        public int     Id      { get; set; }
        public int     DbId    { get; set; }
        public string  Date    { get; set; }
        public int     Status  { get; set; }
        public int     Trade   { get; set; }
        public int     Boxed   { get; set; }
        public int     Manuals { get; set; }
        public int     Cpu1    { get; set; }
        public decimal Mhz1    { get; set; }
        public int     Cpu2    { get; set; }
        public decimal Mhz2    { get; set; }
        public int     Ram     { get; set; }
        public int     Vram    { get; set; }
        public string  Rigid   { get; set; }
        public int     Disk1   { get; set; }
        public int     Cap1    { get; set; }
        public int     Disk2   { get; set; }
        public int     Cap2    { get; set; }
    }
}