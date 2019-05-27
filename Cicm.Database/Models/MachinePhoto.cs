using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Cicm.Database.Models
{
    public class MachinePhoto : BaseModel<Guid>
    {
        public string Author { get; set; }
        [DisplayName("Camera manufacturer")]
        public string CameraManufacturer { get; set; }
        [DisplayName("Camera model")]
        public string CameraModel { get; set; }
        [DisplayName("Color space")]
        public string ColorSpace { get; set; }
        [DisplayName("User comments")]
        public string Comments { get; set; }
        public string Contrast { get; set; }
        [DisplayName("Date and time of digitizing")]
        public DateTime? CreationDate { get; set; }
        [DisplayName("Digital zoom ratio")]
        public double? DigitalZoomRatio { get; set; }
        [DisplayName("Exif version")]
        public string ExifVersion { get; set; }
        [DisplayName("Exposure time")]
        public double? Exposure { get; set; }
        [DisplayName("Exposure mode")]
        public string ExposureMethod { get; set; }
        [DisplayName("Exposure Program")]
        public string ExposureProgram { get; set; }
        public string Flash { get;           set; }
        [DisplayName("F-number")]
        public int? Focal { get; set; }
        [DisplayName("Lens focal length")]
        public int? FocalLength { get; set; }
        [DisplayName("Focal length in 35 mm film")]
        public string FocalLengthEquivalent { get; set; }
        [DisplayName("Horizontal resolution")]
        public int? HorizontalResolution { get; set; }
        [DisplayName("ISO speed rating")]
        public int? IsoRating { get; set; }
        [DisplayName("Lens used")]
        public string Lens { get; set; }
        [DisplayName("Light source")]
        public string LightSource { get; set; }
        [DisplayName("Metering mode")]
        public string MeteringMode { get; set; }
        public string Orientation { get;  set; }
        [DisplayName("Pixel composition")]
        public string PixelComposition { get; set; }
        public string Saturation { get;       set; }
        [DisplayName("Scene capture type")]
        public string SceneCaptureType { get; set; }
        [DisplayName("Scene control")]
        public string SceneControl { get; set; }
        [DisplayName("Sensing method")]
        public string SensingMethod { get; set; }
        public string Sharpness { get;     set; }
        [DisplayName("Software used")]
        public string SoftwareUsed { get; set; }
        [DisplayName("Subject distance range")]
        public string SubjectDistanceRange { get; set; }
        [Timestamp]
        public DateTime UploadDate { get; set; }
        [DisplayName("Vertical resolution")]
        public int? VerticalResolution { get; set; }
        [DisplayName("White balance")]
        public string WhiteBalance { get; set; }

        public virtual ApplicationUser User    { get; set; }
        public virtual Machine         Machine { get; set; }
        [Required]
        public virtual License License { get; set; }
    }
}