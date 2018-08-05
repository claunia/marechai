namespace Cicm.Database.Models
{
    public class BrowserTests
    {
        public int    Id        { get; set; }
        public string UserAgent { get; set; }
        public string Browser   { get; set; }
        public string Version   { get; set; }
        public string Os        { get; set; }
        public string Platform  { get; set; }
        public sbyte  Gif87     { get; set; }
        public sbyte  Gif89     { get; set; }
        public sbyte  Jpeg      { get; set; }
        public sbyte  Png       { get; set; }
        public sbyte  Pngt      { get; set; }
        public sbyte  Agif      { get; set; }
        public sbyte  Table     { get; set; }
        public sbyte  Colors    { get; set; }
        public sbyte  Js        { get; set; }
        public sbyte  Frames    { get; set; }
        public sbyte  Flash     { get; set; }
    }
}