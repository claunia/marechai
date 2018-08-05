namespace Cicm.Database.Models
{
    public class News
    {
        public int    Id      { get; set; }
        public string Date    { get; set; }
        public int    Type    { get; set; }
        public int    AddedId { get; set; }
    }
}