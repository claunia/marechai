using System;

namespace Cicm.Database.Models
{
    public class CicmDb
    {
        public int       Id      { get; set; }
        public int       Version { get; set; }
        public DateTime? Updated { get; set; }
    }
}