using System;
using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Companies
    {
        public Companies()
        {
            CompanyLogos            = new HashSet<CompanyLogos>();
            Gpus                    = new HashSet<Gpus>();
            InverseSoldToNavigation = new HashSet<Companies>();
            MachineFamilies         = new HashSet<MachineFamilies>();
            Machines                = new HashSet<Machines>();
            Processors              = new HashSet<Processors>();
            SoundSynths             = new HashSet<SoundSynths>();
        }

        public int       Id         { get; set; }
        public string    Name       { get; set; }
        public DateTime? Founded    { get; set; }
        public string    Website    { get; set; }
        public string    Twitter    { get; set; }
        public string    Facebook   { get; set; }
        public DateTime? Sold       { get; set; }
        public int?      SoldToId   { get; set; }
        public string    Address    { get; set; }
        public string    City       { get; set; }
        public string    Province   { get; set; }
        public string    PostalCode { get; set; }
        public short?    CountryId  { get; set; }
        public int       Status     { get; set; }

        public Iso31661Numeric              Country                 { get; set; }
        public Companies                    SoldTo                  { get; set; }
        public CompanyDescriptions          CompanyDescriptions     { get; set; }
        public ICollection<CompanyLogos>    CompanyLogos            { get; set; }
        public ICollection<Gpus>            Gpus                    { get; set; }
        public ICollection<Companies>       InverseSoldToNavigation { get; set; }
        public ICollection<MachineFamilies> MachineFamilies         { get; set; }
        public ICollection<Machines>        Machines                { get; set; }
        public ICollection<Processors>      Processors              { get; set; }
        public ICollection<SoundSynths>     SoundSynths             { get; set; }
    }
}