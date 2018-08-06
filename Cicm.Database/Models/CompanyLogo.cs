namespace Cicm.Database.Models
{
    public class CompanyLogo
    {
        public int    Id        { get; set; }
        public int    CompanyId { get; set; }
        public int?   Year      { get; set; }
        public string Guid      { get; set; }

        public Company Company { get; set; }
    }
}