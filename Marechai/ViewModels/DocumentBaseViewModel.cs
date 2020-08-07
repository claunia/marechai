using System;

namespace Marechai.ViewModels
{
    public abstract class DocumentBaseViewModel : BaseViewModel<long>
    {
        public string    Title       { get; set; }
        public string    NativeTitle { get; set; }
        public DateTime? Published   { get; set; }
        public short?    CountryId   { get; set; }
        public string    Country     { get; set; }
        public string    Synopsis    { get; set; }
    }
}