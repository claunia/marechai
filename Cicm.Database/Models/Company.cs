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

        public Iso31661Numeric            Country                 { get; set; }
        public Company                    SoldTo                  { get; set; }
        public CompanyDescription         Description             { get; set; }
        public ICollection<CompanyLogo>   CompanyLogos            { get; set; }
        public ICollection<Gpu>           Gpus                    { get; set; }
        public ICollection<Company>       InverseSoldToNavigation { get; set; }
        public ICollection<MachineFamily> MachineFamilies         { get; set; }
        public ICollection<Machine>       Machines                { get; set; }
        public ICollection<Processor>     Processors              { get; set; }
        public ICollection<SoundSynth>    SoundSynths             { get; set; }
    }
}