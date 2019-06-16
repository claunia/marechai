using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Document : DocumentBase
    {
        public virtual Iso31661Numeric Country { get; set; }

        public virtual ICollection<PeopleByDocument>    People    { get; set; }
        public virtual ICollection<CompaniesByDocument> Companies { get; set; }
    }
}