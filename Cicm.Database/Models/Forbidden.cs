namespace Cicm.Database.Models
{
    public class Forbidden
    {
        public int    Id      { get; set; }
        public string Browser { get; set; }
        public string Date    { get; set; }
        public string Ip      { get; set; }
        public string Referer { get; set; }
    }
}