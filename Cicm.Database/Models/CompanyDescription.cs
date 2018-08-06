namespace Cicm.Database.Models
{
    public class CompanyDescription
    {
        public int    Id        { get; set; }
        public int    CompanyId { get; set; }
        public string Text      { get; set; }

        public Company Company { get; set; }
    }
}