namespace Cicm.Database.Models
{
    public class CompanyLogos
    {
        public int    Id        { get; set; }
        public int    CompanyId { get; set; }
        public int?   Year      { get; set; }
        public string LogoGuid  { get; set; }

        public Companies Company { get; set; }
    }
}