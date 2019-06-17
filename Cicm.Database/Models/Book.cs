using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cicm.Database.Models
{
    public class Book : DocumentBase
    {
        [StringLength(13, MinimumLength = 10)]
        public string Isbn { get;       set; }
        public short? Pages      { get; set; }
        public int?   Edition    { get; set; }
        public long?  PreviousId { get; set; }
        public long?  SourceId   { get; set; }

        public virtual Book                         Previous  { get; set; }
        public virtual Book                         Source    { get; set; }
        public virtual Book                         Next      { get; set; }
        public virtual Iso31661Numeric              Country   { get; set; }
        public virtual ICollection<Book>            Derivates { get; set; }
        public virtual ICollection<CompaniesByBook> Companies { get; set; }
        public virtual ICollection<PeopleByBook>    People    { get; set; }
    }
}