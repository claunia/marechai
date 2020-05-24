using System;

namespace Marechai.ViewModels
{
    public class CompanyViewModel
    {
        public int    Id       { get; set; }
        public Guid?  LastLogo { get; set; }
        public string Name     { get; set; }
    }
}