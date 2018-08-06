using System;

namespace Cicm.Database.Models
{
    public class CompanyLogo
    {
        public int  Id        { get; set; }
        public int  CompanyId { get; set; }
        public int? Year      { get; set; }
        public Guid Guid      { get; set; }

        public virtual Company Company { get; set; }
    }
}