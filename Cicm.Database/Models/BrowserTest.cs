namespace Cicm.Database.Models
{
    public class BrowserTest
    {
        public int    Id        { get; set; }
        public string UserAgent { get; set; }
        public string Browser   { get; set; }
        public string Version   { get; set; }
        public string Os        { get; set; }
        public string Platform  { get; set; }
        public bool   Gif87     { get; set; }
        public bool   Gif89     { get; set; }
        public bool   Jpeg      { get; set; }
        public bool   Png       { get; set; }
        public bool   Pngt      { get; set; }
        public bool   Agif      { get; set; }
        public bool   Table     { get; set; }
        public bool   Colors    { get; set; }
        public bool   Js        { get; set; }
        public bool   Frames    { get; set; }
        public bool   Flash     { get; set; }
    }
}