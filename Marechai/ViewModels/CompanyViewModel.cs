using System;
using Marechai.Database;

namespace Marechai.ViewModels
{
    public class CompanyViewModel : BaseViewModel<int>
    {
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
        public Guid?         LastLogo   { get; set; }
        public string        SoldTo     { get; set; }
        public string        Country    { get; set; }

        public string SoldView => Status != CompanyStatus.Active && Status != CompanyStatus.Unknown
                                      ? Sold?.ToShortDateString() ?? "Unknown"
                                      : Sold?.ToShortDateString() ?? (SoldTo is null ? "" : "Unknown");
    }
}