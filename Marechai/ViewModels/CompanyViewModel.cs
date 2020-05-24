using System;
using Marechai.Database;

namespace Marechai.ViewModels
{
    public class CompanyViewModel
    {
        public int    Id       { get; set; }
        public Guid?  LastLogo { get; set; }
        public string Name     { get; set; }

        public DateTime?     Founded { get; set; }
        public DateTime?     Sold    { get; set; }
        public string        SoldTo  { get; set; }
        public string        Country { get; set; }
        public CompanyStatus Status  { get; set; }

        public string SoldView => Status != CompanyStatus.Active && Status != CompanyStatus.Unknown
                                      ? Sold?.ToShortDateString() ?? "Unknown"
                                      : Sold?.ToShortDateString() ?? (SoldTo is null
                                                                          ? ""
                                                                          : "Unknown");
    }
}