namespace Marechai.ViewModels
{
    public class BookViewModel : DocumentBaseViewModel
    {
        public string Isbn       { get; set; }
        public short? Pages      { get; set; }
        public int?   Edition    { get; set; }
        public long?  PreviousId { get; set; }
        public long?  SourceId   { get; set; }
    }
}