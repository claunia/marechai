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
// Copyright © 2003-2020 Natalia Portillo
*******************************************************************************/

namespace Marechai.Helpers
{
    public static class ImageMagick
    {
        public static string GetExtension(string format)
        {
            switch(format)
            {
                case"3FR":    return".3fr";
                case"AAI":    return".aai";
                case"AI":     return".ai";
                case"ARW":    return".arw";
                case"AVS":    return".avs";
                case"BIE":    return".jbg";
                case"BMP":    return".bmp";
                case"BMP2":   return".bmp";
                case"BMP3":   return".bmp";
                case"CR2":    return".cr2";
                case"CR3":    return".cr3";
                case"CRW":    return".crw";
                case"CUR":    return".cur";
                case"CUT":    return".cut";
                case"DCM":    return".dcm";
                case"DCR":    return".dcr";
                case"DCRAW":  return".dng";
                case"DCX":    return".dcx";
                case"DDS":    return".dds";
                case"DNG":    return".dng";
                case"DPX":    return".dpx";
                case"DXT1":   return".dds";
                case"DXT5":   return".dds";
                case"EPDF":   return".pdf";
                case"EPI":    return".epi";
                case"EPS":    return".eps";
                case"EPS2":   return".eps";
                case"EPS3":   return".eps";
                case"EPSF":   return".eps";
                case"EPSI":   return".epi";
                case"ERF":    return".erf";
                case"EXR":    return".exr";
                case"GIF":    return".gif";
                case"GIF87":  return".gif";
                case"GROUP4": return".tif";
                case"HDR":    return".hdr";
                case"HEIC":   return".heic";
                case"ICB":    return".tga";
                case"ICO":    return".ico";
                case"ICON":   return".ico";
                case"J2C":    return".jp2";
                case"J2K":    return".jp2";
                case"JBG":    return".jbg";
                case"JBIG":   return".jbg";
                case"JNG":    return".jng";
                case"JP2":    return".jp2";
                case"JPC":    return".jp2";
                case"JPE":    return".jpg";
                case"JPEG":   return".jpg";
                case"JPG":    return".jpg";
                case"K25":    return".k25";
                case"KDC":    return".kdc";
                case"MIFF":   return".miff";
                case"MNG":    return".mng";
                case"MRW":    return".mrw";
                case"NEF":    return".nef";
                case"NRW":    return".nrw";
                case"ORF":    return".orf";
                case"PALM":   return".palm";
                case"PBM":    return".pbm";
                case"PCD":    return".pcd";
                case"PCDS":   return".pcd";
                case"PCL":    return".pcl";
                case"PCT":    return".pct";
                case"PCX":    return".pcx";
                case"PDB":    return".pdb";
                case"PDF":    return".pdf";
                case"PDFA":   return".pdf";
                case"PEF":    return".pef";
                case"PFM":    return".pfm";
                case"PGM":    return".pgm";
                case"PGX":    return".jp2";
                case"PICON":  return".xpm";
                case"PICT":   return".pct";
                case"PNG":    return".png";
                case"PNG00":  return".png";
                case"PNG24":  return".png";
                case"PNG32":  return".png";
                case"PNG48":  return".png";
                case"PNG64":  return".png";
                case"PNG8":   return".png";
                case"PNM":    return".pnm";
                case"PPM":    return".ppm";
                case"PS":     return".ps";
                case"PS2":    return".ps";
                case"PS3":    return".ps";
                case"PSB":    return".psd";
                case"PSD":    return".psd";
                case"PTIF":   return".tif";
                case"RAF":    return".raf";
                case"RAS":    return".ras";
                case"RAW":    return".dng";
                case"RLA":    return".rla";
                case"RMF":    return".rmf";
                case"RW2":    return".rw2";
                case"SCR":    return".scr";
                case"SGI":    return".sgi";
                case"SR2":    return".sr2";
                case"SRF":    return".srf";
                case"SUN":    return".ras";
                case"SVG":    return".svg";
                case"SVGZ":   return".svgz";
                case"TGA":    return".tga";
                case"TIFF":   return".tif";
                case"TIFF64": return".tif";
                case"TIM":    return".tim";
                case"TIM2":   return".tm2";
                case"TM2":    return".tm2";
                case"VIFF":   return".viff";
                case"VST":    return".vst";
                case"WBMP":   return".wbmp";
                case"WEBP":   return".webp";
                case"WMF":    return".wmf";
                case"WMZ":    return".wmf";
                case"WPG":    return".wpg";
                case"X":      return".x";
                case"X3F":    return".x3f";
                case"XBM":    return".xbm";
                case"XC":     return".xc";
                case"XCF":    return".xcf";
                case"XPM":    return".xpm";
                case"XPS":    return".xps";
                case"XV":     return".xv";
                case"XWD":    return".xwd";
                default:      return null;
            }
        }
    }
}