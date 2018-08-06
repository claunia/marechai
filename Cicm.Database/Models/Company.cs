using System;
using System.Collections.Generic;

namespace Cicm.Database.Models
{
    public class Company
    {
        public Company()
        {
            CompanyLogos            = new HashSet<CompanyLogo>();
            Gpus                    = new HashSet<Gpu>();
            InverseSoldToNavigation = new HashSet<Company>();
            MachineFamilies         = new HashSet<MachineFamily>();
            Machines                = new HashSet<Machine>();
            Processors              = new HashSet<Processor>();
            SoundSynths             = new HashSet<SoundSynth>();
        }

        public int           Id         { get; set; }
        public string        Name       { get; set; }
        public DateTime?     Founded    { get; set; }
        public string        Website    { get; set; }
        public string        Twitter    { get; set; }
        public string        Facebook   { get; set; }
        public DateTime?     Sold       { get; set; }
        public int?          SoldToId   { get; set; }
        public string        Address    { get; set; }
        public string        City       { get; set; }
        public string        Province   { get; set; }
        public string        PostalCode { get; set; }
        public short?        CountryId  { get; set; }
        public CompanyStatus Status     { get; set; }

        public virtual Iso31661Numeric            Country                 { get; set; }
        public virtual Company                    SoldTo                  { get; set; }
        public virtual CompanyDescription         Description             { get; set; }
        public virtual ICollection<CompanyLogo>   CompanyLogos            { get; set; }
        public virtual ICollection<Gpu>           Gpus                    { get; set; }
        public virtual ICollection<Company>       InverseSoldToNavigation { get; set; }
        public virtual ICollection<MachineFamily> MachineFamilies         { get; set; }
        public virtual ICollection<Machine>       Machines                { get; set; }
        public virtual ICollection<Processor>     Processors              { get; set; }
        public virtual ICollection<SoundSynth>    SoundSynths             { get; set; }
    }
}