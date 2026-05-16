/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Marechai.Data;
using Marechai.Database.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace Marechai.Server.Helpers;

/// <summary>
///     Extracts EXIF metadata from an image and writes the recognised fields onto a
///     <see cref="BasePhoto" /> instance. Used by both the admin photo upload path and
///     the collaborative-suggestion photo-acceptance path so EXIF handling stays in one
///     place.
/// </summary>
public static class PhotoExifExtractor
{
    /// <summary>
    ///     Read EXIF metadata from <paramref name="content" /> and copy the recognised
    ///     fields onto <paramref name="target" />. The stream is left at its original
    ///     position. EXIF read failures are silently absorbed — fields stay null/default.
    /// </summary>
    public static void ExtractInto(BasePhoto target, Stream content)
    {
        if(target is null) throw new ArgumentNullException(nameof(target));
        if(content is null) throw new ArgumentNullException(nameof(content));

        long origPos = content.CanSeek ? content.Position : 0;

        try
        {
            if(content.CanSeek) content.Position = 0;

            IReadOnlyList<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(content);

            ExifIfd0Directory exifIfd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            ExifSubIfdDirectory exifSub = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            if(exifIfd0 is not null)
            {
                if(exifIfd0.TryGetUInt16(ExifDirectoryBase.TagOrientation, out ushort orientation))
                    target.Orientation = (Orientation)orientation;

                target.CameraManufacturer = exifIfd0.GetDescription(ExifDirectoryBase.TagMake);
                target.CameraModel        = exifIfd0.GetDescription(ExifDirectoryBase.TagModel);
                target.SoftwareUsed       = exifIfd0.GetDescription(ExifDirectoryBase.TagSoftware);
                target.Author             = exifIfd0.GetDescription(ExifDirectoryBase.TagArtist);

                if(exifIfd0.TryGetDouble(ExifDirectoryBase.TagXResolution, out double xRes))
                    target.HorizontalResolution = xRes;

                if(exifIfd0.TryGetDouble(ExifDirectoryBase.TagYResolution, out double yRes))
                    target.VerticalResolution = yRes;

                if(exifIfd0.TryGetUInt16(ExifDirectoryBase.TagResolutionUnit, out ushort resUnit))
                    target.ResolutionUnit = (ResolutionUnit)resUnit;
            }

            if(exifSub is not null)
            {
                if(exifSub.TryGetDouble(ExifDirectoryBase.TagFNumber, out double fNumber)) target.Focal = fNumber;

                if(exifSub.TryGetDouble(ExifDirectoryBase.TagAperture, out double aperture))
                    target.Aperture = aperture;

                if(exifSub.TryGetDouble(ExifDirectoryBase.TagExposureTime, out double exposureTime))
                    target.ExposureTime = exposureTime;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagExposureProgram, out ushort exposureProgram))
                    target.ExposureProgram = (ExposureProgram)exposureProgram;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagIsoEquivalent, out ushort isoRating))
                    target.IsoRating = isoRating;

                target.ExifVersion = exifSub.GetDescription(ExifDirectoryBase.TagExifVersion);

                if(exifSub.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime creationDate))
                    target.CreationDate = creationDate;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagMeteringMode, out ushort meteringMode))
                    target.MeteringMode = (MeteringMode)meteringMode;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagFlash, out ushort flash))
                    target.Flash = (Flash)flash;

                if(exifSub.TryGetDouble(ExifDirectoryBase.TagFocalLength, out double focalLength))
                    target.FocalLength = focalLength;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagColorSpace, out ushort colorSpace))
                    target.ColorSpace = (ColorSpace)colorSpace;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagExposureMode, out ushort exposureMode))
                    target.ExposureMethod = (ExposureMode)exposureMode;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagWhiteBalance, out ushort whiteBalance))
                    target.WhiteBalance = (WhiteBalance)whiteBalance;

                if(exifSub.TryGetDouble(ExifDirectoryBase.TagDigitalZoomRatio, out double digitalZoom))
                    target.DigitalZoomRatio = digitalZoom;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.Tag35MMFilmEquivFocalLength, out ushort focalLengthEquiv))
                    target.FocalLengthEquivalent = focalLengthEquiv;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagSceneCaptureType, out ushort sceneCaptureType))
                    target.SceneCaptureType = (SceneCaptureType)sceneCaptureType;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagContrast, out ushort contrast))
                    target.Contrast = (Contrast)contrast;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagSaturation, out ushort saturation))
                    target.Saturation = (Saturation)saturation;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagSharpness, out ushort sharpness))
                    target.Sharpness = (Sharpness)sharpness;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagSubjectDistanceRange, out ushort subjectDistRange))
                    target.SubjectDistanceRange = (SubjectDistanceRange)subjectDistRange;

                if(exifSub.TryGetUInt16(ExifDirectoryBase.TagSensingMethod, out ushort sensingMethod))
                    target.SensingMethod = (SensingMethod)sensingMethod;

                if(exifSub.TryGetUInt16(0x9208, out ushort lightSource)) target.LightSource = (LightSource)lightSource;

                target.Lens = exifSub.GetDescription(ExifDirectoryBase.TagLensModel);

                // Don't auto-overwrite an existing comment (collaborator-supplied per-photo
                // comment takes precedence over the EXIF UserComment tag).
                if(string.IsNullOrEmpty(target.Comments))
                    target.Comments = TruncateUtf8(exifSub.GetDescription(ExifDirectoryBase.TagUserComment),
                                                   ExifUserCommentMaxBytes);
            }
        }
        catch
        {
            // EXIF extraction failed — continue without metadata
        }
        finally
        {
            if(content.CanSeek) content.Position = origPos;
        }
    }

    /// <summary>
    ///     Maximum byte length of the EXIF 2.x <c>UserComment</c> tag — 8-byte
    ///     character-code prefix + up to 1015 bytes of payload, 1023 bytes total.
    ///     Some cameras and editors emit longer values; we cap so the field stays
    ///     spec-compliant and avoids surprising downstream consumers.
    /// </summary>
    private const int ExifUserCommentMaxBytes = 1023;

    /// <summary>
    ///     Truncate <paramref name="value" /> so its UTF-8 encoding is at most
    ///     <paramref name="maxBytes" /> bytes long, without splitting a multi-byte
    ///     code point. Returns <c>null</c> for null input and the original string
    ///     when it already fits.
    /// </summary>
    private static string TruncateUtf8(string value, int maxBytes)
    {
        if(string.IsNullOrEmpty(value)) return value;

        if(Encoding.UTF8.GetByteCount(value) <= maxBytes) return value;

        byte[] bytes = Encoding.UTF8.GetBytes(value);
        int    end   = maxBytes;

        // Walk backwards while we're inside a UTF-8 continuation byte (10xxxxxx)
        // so we never split a code point.
        while(end > 0 && (bytes[end] & 0xC0) == 0x80) end--;

        return Encoding.UTF8.GetString(bytes, 0, end);
    }
}
