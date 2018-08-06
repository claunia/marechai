namespace Cicm.Database.Models
{
    public class CompanyDescriptions
    {
        public int    Id        { get; set; }
        public int    CompanyId { get; set; }
        public string Text      { get; set; }

        public Companies Company { get; set; }
    }
}