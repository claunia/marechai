using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Marechai.Database;
using Marechai.ViewModels;

namespace Marechai.Helpers
{
    public class Exif
    {
        public string      Make        { get; set; }
        public string      Model       { get; set; }
        public string      Author      { get; set; }
        public ColorSpace? ColorSpace  { get; set; }
        public string      Description { get; set; }
        public Contrast    Contrast    { get; set; }
        [JsonConverter(typeof(ExiftoolDateConverter))]
        public DateTime? CreateDate { get; set; }
        [JsonConverter(typeof(ExiftoolDateConverter))]
        public DateTime? DateTimeOriginal { get; set; }
        [JsonConverter(typeof(ExiftoolDateConverter))]
        public DateTime? ModifyDate { get;                          set; }
        public double?               DigitalZoomRatio        { get; set; }
        public string                ExifVersion             { get; set; }
        public double?               ExposureTime            { get; set; }
        public ExposureMode?         ExposureMode            { get; set; }
        public ExposureProgram?      ExposureProgram         { get; set; }
        public Flash?                Flash                   { get; set; }
        public double?               FNumber                 { get; set; }
        public double?               FocalLength             { get; set; }
        public double?               FocalLengthIn35mmFormat { get; set; }
        public double?               XResolution             { get; set; }
        public ushort?               ISO                     { get; set; }
        public string                LensModel               { get; set; }
        public string                Lens                    { get; set; }
        public LightSource?          LightSource             { get; set; }
        public MeteringMode?         MeteringMode            { get; set; }
        public ResolutionUnit?       ResolutionUnit          { get; set; }
        public Orientation?          Orientation             { get; set; }
        public Saturation?           Saturation              { get; set; }
        public SceneCaptureType?     SceneCaptureType        { get; set; }
        public SensingMethod?        SensingMethod           { get; set; }
        public Sharpness?            Sharpness               { get; set; }
        public string                Software                { get; set; }
        public SubjectDistanceRange? SubjectDistanceRange    { get; set; }
        public double?               YResolution             { get; set; }
        public WhiteBalance?         WhiteBalance            { get; set; }
        public double?               ApertureValue           { get; set; }

        public void ToViewModel(BasePhotoViewModel model)
        {
            model.CameraManufacturer    = Make;
            model.CameraModel           = Model;
            model.Author                = Author;
            model.CreationDate          = DateTimeOriginal ?? CreateDate ?? ModifyDate;
            model.DigitalZoomRatio      = DigitalZoomRatio;
            model.ExifVersion           = ExifVersion;
            model.ExposureTime          = ExposureTime;
            model.Focal                 = FNumber;
            model.HorizontalResolution  = XResolution;
            model.IsoRating             = ISO;
            model.Lens                  = LensModel ?? Lens;
            model.SoftwareUsed          = Software;
            model.VerticalResolution    = YResolution;
            model.Aperture              = ApertureValue;
            model.ColorSpace            = ColorSpace;
            model.Contrast              = Contrast;
            model.ExposureMethod        = ExposureMode;
            model.ExposureProgram       = ExposureProgram;
            model.Flash                 = Flash;
            model.FocalLength           = FocalLength;
            model.FocalLengthEquivalent = FocalLengthIn35mmFormat;
            model.LightSource           = LightSource;
            model.MeteringMode          = MeteringMode;
            model.ResolutionUnit        = ResolutionUnit;
            model.Orientation           = Orientation;
            model.Saturation            = Saturation;
            model.SceneCaptureType      = SceneCaptureType;
            model.SensingMethod         = SensingMethod;
            model.Sharpness             = Sharpness;
            model.SubjectDistanceRange  = SubjectDistanceRange;
            model.WhiteBalance          = WhiteBalance;
            model.Comments              = Description;
        }
    }

    internal class ExiftoolDateConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime?));
            string read = reader.GetString();

            if(read is null)
                return null;

            return DateTime.ParseExact(reader.GetString(), "yyyy':'MM':'dd' 'HH':'mm':'ssK",
                                       CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if(value is null)
                return;

            writer.WriteStringValue(value?.ToUniversalTime().ToString("yyyy':'MM':'dd' 'HH':'mm':'ssK"));
        }
    }
}