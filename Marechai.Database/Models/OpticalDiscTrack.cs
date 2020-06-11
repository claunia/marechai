using Aaru.CommonTypes.Enums;

namespace Marechai.Database.Models
{
    // Not for a table
    public class OpticalDiscTrack
    {
        public int       TrackNumber   { get; set; }
        public int       SessionNumber { get; set; }
        public long      FirstSector   { get; set; }
        public long      LastSector    { get; set; }
        public TrackType Type          { get; set; }
    }
}