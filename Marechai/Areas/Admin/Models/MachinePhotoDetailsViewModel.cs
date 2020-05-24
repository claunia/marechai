using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Marechai.Database;

namespace Marechai.Areas.Admin.Models
{
    public class MachinePhotoDetailsViewModel : BaseViewModel<Guid>
    {
        public string Author { get; set; }
        [DisplayName("Camera manufacturer")]
        public string CameraManufacturer { get; set; }
        [DisplayName("Camera model")]
        public string CameraModel { get; set; }
        [DisplayName("Color space")]
        public ColorSpace? ColorSpace { get; set; }
        [DisplayName("User comments")]
        public string Comments { get;    set; }
        public Contrast? Contrast { get; set; }
        [DisplayName("Date and time of digitizing")]
        public DateTime? CreationDate { get; set; }
        [DisplayName("Digital zoom ratio")]
        public double? DigitalZoomRatio { get; set; }
        [DisplayName("Exif version")]
        public string ExifVersion { get; set; }
        [DisplayName("Exposure time")]
        public string Exposure { get; set; }
        [DisplayName("Exposure mode")]
        public ExposureMode? ExposureMethod { get; set; }
        [DisplayName("Exposure Program")]
        public ExposureProgram? ExposureProgram { get; set; }
        public Flash? Flash { get;                     set; }
        [DisplayName("F-number")]
        public double? Focal { get; set; }
        [DisplayName("Lens focal length")]
        public double? FocalLength { get; set; }
        [DisplayName("Focal length in 35 mm film")]
        public ushort? FocalLengthEquivalent { get; set; }
        [DisplayName("Horizontal resolution")]
        public double? HorizontalResolution { get; set; }
        [DisplayName("ISO speed rating")]
        public ushort? IsoRating { get; set; }
        [DisplayName("Lens used")]
        public string Lens { get; set; }
        [DisplayName("Light source")]
        public LightSource? LightSource { get; set; }
        [DisplayName("Metering mode")]
        public MeteringMode? MeteringMode { get; set; }
        [DisplayName("Resolution unit")]
        public ResolutionUnit? ResolutionUnit { get; set; }
        public Orientation? Orientation { get;       set; }
        public Saturation?  Saturation  { get;       set; }
        [DisplayName("Scene capture type")]
        public SceneCaptureType? SceneCaptureType { get; set; }
        [DisplayName("Sensing method")]
        public SensingMethod? SensingMethod { get; set; }
        public Sharpness? Sharpness { get;         set; }
        [DisplayName("Software used")]
        public string SoftwareUsed { get; set; }
        [DisplayName("Subject distance range")]
        public SubjectDistanceRange? SubjectDistanceRange { get; set; }
        [Timestamp, DisplayName("Uploaded on")]
        public DateTime UploadDate { get; set; }
        [DisplayName("Vertical resolution")]
        public double? VerticalResolution { get; set; }
        [DisplayName("White balance")]
        public WhiteBalance? WhiteBalance { get; set; }
        public string License { get;             set; }
        [DisplayName("Uploaded by")]
        public string UploadUser { get; set; }
        public string Machine   { get;  set; }
        public int    MachineId { get;  set; }
        public string Source    { get;  set; }
    }
}