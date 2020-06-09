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

using System.ComponentModel.DataAnnotations;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Marechai.Database
{
    public enum NewsType
    {
        NewComputerInDb             = 1, NewConsoleInDb             = 2, NewComputerInCollection = 3,
        NewConsoleInCollection      = 4, UpdatedComputerInDb        = 5, UpdatedConsoleInDb      = 6,
        UpdatedComputerInCollection = 7, UpdatedConsoleInCollection = 8, NewMoneyDonation        = 9
    }

    public enum StatusType
    {
        [Display(Name = "Unknown")]
        Unknown = 0, [Display(Name = "Tested good")]
        TestedGood = 1, [Display(Name = "Not tested")]
        NotTested = 2, [Display(Name = "Tested bad")]
        TestedBad = 3
    }

    public enum CompanyStatus
    {
        /// <summary>Status is unknown or not set</summary>
        Unknown = 0,
        /// <summary>Company is still existing</summary>
        Active = 1,
        /// <summary>Company was sold, totally or partially</summary>
        Sold = 2,
        /// <summary>Company merged with another company to make yet another company</summary>
        Merged = 3,
        /// <summary>Company filled for bankruptcy</summary>
        Bankrupt = 4,
        /// <summary>Company ceased operations for reasons different to bankruptcy</summary>
        Defunct = 5,
        /// <summary>Company renamed possibly with a change of intentions</summary>
        Renamed = 6
    }

    public enum MachineType
    {
        /// <summary>Unknown machine type, should not happen</summary>
        Unknown = 0,
        /// <summary>Computer</summary>
        Computer = 1,
        /// <summary>Videogame console</summary>
        Console = 2
    }

    public enum MemoryType
    {
        /// <summary>Unknown memory type</summary>
        Unknown = 0,
        /// <summary>Dynamic RAM</summary>
        DRAM = 1,
        /// <summary>Fast page mode DRAM</summary>
        FPM = 2,
        /// <summary>Extended data out DRAM</summary>
        EDO = 3,
        /// <summary>Dual-ported video DRAM</summary>
        VRAM = 4,
        /// <summary>Synchronous DRAM</summary>
        SDRAM = 5,
        /// <summary>Double data rate SDRAM</summary>
        DDR = 6,
        /// <summary>Double data rate SDRAM v2</summary>
        DDR2 = 7,
        /// <summary>Double data rate SDRAM v3</summary>
        DDR3 = 8,
        /// <summary>Double data rate SDRAM v4</summary>
        DDR4 = 9,
        /// <summary>Rambus DRAM</summary>
        RDRAM = 10,
        /// <summary>Synchronous graphics RAM</summary>
        SGRAM = 11,
        /// <summary>Pseudostatic RAM</summary>
        PSRAM = 12,
        /// <summary>Static RAM</summary>
        SRAM = 13,
        /// <summary>Read-only memory</summary>
        ROM = 14,
        /// <summary>Programmable ROM</summary>
        PROM = 15,
        /// <summary>Erasable programmable ROM</summary>
        EPROM = 16,
        /// <summary>Electronically-erasable programmable ROM</summary>
        EEPROM = 17,
        /// <summary>NAND flash</summary>
        NAND = 18,
        /// <summary>NOR flash</summary>
        NOR = 19,
        /// <summary>Resistive RAM</summary>
        ReRAM = 20,
        /// <summary>Conductive-bridging RAM</summary>
        CBRAM = 21,
        /// <summary>Domain-wall memory</summary>
        DWM = 22,
        /// <summary>Nano-RAM</summary>
        NanoRAM = 23,
        /// <summary>Millipede memory</summary>
        Millipede = 24,
        /// <summary>Floating Junction Gate RAM</summary>
        FJG = 25,
        /// <summary>Punched paper</summary>
        PunchedPaper = 26,
        /// <summary>Drum memory</summary>
        DrumMemory = 27,
        /// <summary>Magnetic-core</summary>
        MagneticCore = 28,
        /// <summary>Plated wire memory</summary>
        PlatedWire = 29,
        /// <summary>Core rope memory</summary>
        CoreRope = 30,
        /// <summary>Thin-film memory</summary>
        ThinFilm = 31,
        /// <summary>Twistor memory</summary>
        Twistor = 32,
        /// <summary>Bubble memory</summary>
        Bubble = 33
    }

    public enum MemoryUsage
    {
        /// <summary>Unknown usage</summary>
        Unknown = 0,
        /// <summary>
        ///     Contains a boot loader (usually read-only) whose only function is to load the next memory (firmware or
        ///     cartridge)
        /// </summary>
        Bootloader = 1,
        /// <summary>
        ///     Contains hardware initializing, some (or many) low level calls and code to load software from secondary
        ///     storage
        /// </summary>
        Firmware = 2,
        /// <summary>Memory used by software running on the machine</summary>
        Work = 3,
        /// <summary>Memory used by the graphics processing units</summary>
        Video = 4,
        /// <summary>Memory used by the sound synthesizers</summary>
        Sound = 5,
        /// <summary>Memory used to store wave tables</summary>
        Wavetable = 6,
        /// <summary>Memory used as a buffer from secondary storage</summary>
        StorageBuffer = 7,
        /// <summary>Memory used to save arbitrary data and possible also configuration</summary>
        Save = 8,
        /// <summary>Memory used to save only configuration</summary>
        Configuration = 9,
        /// <summary>
        ///     Memory accessible directly to any of the processors in the machine, including graphics processors and sound
        ///     synthesizers
        /// </summary>
        Unified = 10
    }

    public enum StorageType
    {
        /// <summary>Contains an empty interface for user connection</summary>
        Empty = -1,
        /// <summary>Unknown</summary>
        Unknown = 0,
        /// <summary>Unknown magneto-optical</summary>
        MagnetoOptical = 1,
        /// <summary>Generic hard disk</summary>
        HardDisk = 2,
        /// <summary>Microdrive type hard disk</summary>
        Microdrive = 3,
        /// <summary>Zoned hard disk</summary>
        ZonedHardDisk = 4,
        /// <summary>USB flash drives</summary>
        FlashDrive = 5,
        /// <summary>CompactDisc</summary>
        CompactDisc = 6,
        /// <summary>Double-Density CompactDisc (Purple Book)</summary>
        DDCD = 7,
        /// <summary>120mm, Phase-Change, 1298496 sectors, 512 bytes/sector, PD650, ECMA-240, ISO 15485</summary>
        PD650 = 8,
        /// <summary>DVD</summary>
        Dvd = 9,
        /// <summary>DVD-RAM (cartridge only)</summary>
        DVDRAM = 10,
        /// <summary>HD DVD</summary>
        HDDVDROM = 11,
        /// <summary>Blu-ray Disc</summary>
        Bluray = 12,
        /// <summary>Enhanced Versatile Disc</summary>
        EVD = 13,
        /// <summary>Forward Versatile Disc</summary>
        FVD = 14,
        /// <summary>Holographic Versatile Disc</summary>
        HVD = 15,
        /// <summary>China Blue High Definition</summary>
        CBHD = 16,
        /// <summary>High Definition Versatile Multilayer Disc</summary>
        HDVMD = 17,
        /// <summary>Versatile Compact Disc High Density</summary>
        VCDHD = 18,
        /// <summary>Stacked Volumetric Optical Disc</summary>
        SVOD = 19,
        /// <summary>Five Dimensional disc</summary>
        FDDVD = 20,
        /// <summary>Pioneer LaserDisc</summary>
        LD = 21,
        /// <summary>Pioneer LaserDisc data</summary>
        LDROM = 22, LDROM2 = 23, LVROM = 24, MegaLD = 254,
        /// <summary>Sony Hi-MD</summary>
        HiMD = 26,
        /// <summary>Sony MiniDisc</summary>
        MD = 27, MDData = 28, MDData2 = 29,
        /// <summary>5.25", Phase-Change, 1834348 sectors, 8192 bytes/sector, Ultra Density Optical, ECMA-350, ISO 17345</summary>
        UDO = 30,
        /// <summary>5.25", Phase-Change, 3669724 sectors, 8192 bytes/sector, Ultra Density Optical 2, ECMA-380, ISO 11976</summary>
        UDO2 = 31, PlayStationMemoryCard = 32, PlayStationMemoryCard2 = 33,
        /// <summary>Sony PlayStation game CD</summary>
        PS1CD = 34,
        /// <summary>Sony PlayStation 2 game CD</summary>
        PS2CD = 35,
        /// <summary>Sony PlayStation 2 game DVD</summary>
        PS2DVD = 36,
        /// <summary>Sony PlayStation 3 game DVD</summary>
        PS3DVD = 37,
        /// <summary>Sony PlayStation 3 game Blu-ray</summary>
        PS3BD = 38,
        /// <summary>Sony PlayStation 4 game Blu-ray</summary>
        PS4BD = 39,
        /// <summary>Sony PlayStation Portable Universal Media Disc (ECMA-365)</summary>
        UMD = 40,
        /// <summary>Microsoft X-box Game Disc</summary>
        XGD = 41,
        /// <summary>Microsoft X-box 360 Game Disc</summary>
        XGD2 = 42,
        /// <summary>Microsoft X-box 360 Game Disc</summary>
        XGD3 = 43,
        /// <summary>Microsoft X-box One Game Disc</summary>
        XGD4 = 44,
        /// <summary>Sega MegaCD</summary>
        MEGACD = 45,
        /// <summary>Sega Saturn disc</summary>
        SATURNCD = 46,
        /// <summary>Sega/Yamaha Gigabyte Disc</summary>
        GDROM = 47, SegaCard = 48,
        /// <summary>PC-Engine / TurboGrafx cartridge</summary>
        HuCard = 49,
        /// <summary>PC-Engine / TurboGrafx CD</summary>
        SuperCDROM2 = 50,
        /// <summary>Atari Jaguar CD</summary>
        JaguarCD = 51,
        /// <summary>3DO CD</summary>
        ThreeDO = 52,
        /// <summary>NEC PC-FX</summary>
        PCFX = 53,
        /// <summary>NEO-GEO CD</summary>
        NeoGeoCD = 54,
        /// <summary>8" floppy</summary>
        Floppy = 55,
        /// <summary>5.25" floppy</summary>
        Minifloppy = 56,
        /// <summary>3.5" floppy</summary>
        Microfloppy = 57,
        /// <summary>5.25", DS, ?D, ?? tracks, ?? spt, 512 bytes/sector, GCR, opposite side heads, aka Twiggy</summary>
        AppleFileWare = 58, Bernoulli = 59, Bernoulli2            = 60, Ditto              = 61,
        DittoMax                      = 62, Jaz                   = 63, Jaz2               = 64,
        PocketZip                     = 65, REV120                = 66, REV35              = 67,
        REV70                         = 68, ZIP100                = 69, ZIP250             = 70,
        ZIP750                        = 71, CompactCassette       = 72, Data8              = 73,
        MiniDV                        = 74, CFast                 = 75, CompactFlash       = 76,
        CompactFlashType2             = 77, EZ135                 = 78, EZ230              = 79,
        Quest                         = 80, SparQ                 = 81, SQ100              = 82,
        SQ200                         = 83, SQ300                 = 84, SQ310              = 85,
        SQ327                         = 86, SQ400                 = 87, SQ800              = 88,
        SQ1500                        = 89, SQ2000                = 90, SyJet              = 91,
        FamicomGamePak                = 92, GameBoyAdvanceGamePak = 93, GameBoyGamePak     = 94,
        GOD                           = 95, N64DD                 = 96, N64GamePak         = 97,
        NESGamePak                    = 98, Nintendo3DSGameCard   = 99, NintendoDiskCard   = 100,
        NintendoDSGameCard            = 101, NintendoDSiGameCard  = 102, SNESGamePak       = 103,
        SNESGamePakUS                 = 104, WOD                  = 105, WUOD              = 106,
        SwitchGameCard                = 107, MemoryStick          = 108, MemoryStickDuo    = 109,
        MemoryStickMicro              = 110, MemoryStickPro       = 111, MemoryStickProDuo = 112,
        microSD                       = 113, miniSD               = 114, SecureDigital     = 115,
        MMC                           = 116, MMCmicro             = 117, RSMMC             = 118,
        MMCplus                       = 118, MMCmobile            = 119, eMMC              = 120,
        MO120                         = 121, MO90                 = 122, MO300             = 123,
        MO356                         = 124, CompactFloppy        = 125, DemiDiskette      = 126,
        /// <summary>3.5", 652 tracks, 2 sides, 512 bytes/sector, Floptical, ECMA-207, ISO 14169</summary>
        Floptical = 127, HiFD = 128, QuickDisk = 129, UHD144       = 130,
        VideoFloppy           = 131, Wafer     = 132, ZXMicrodrive = 133,
        BeeCard               = 134, Borsu     = 135, DataStore    = 136,
        MiniCard              = 137, Orb       = 138, Orb5         = 139,
        SmartMedia            = 140, xD        = 141, XQD          = 142,
        DataPlay              = 143, LS120     = 144, LS240        = 145,
        FD32MB                = 146, RDX       = 147, PunchedCard  = 148
    }

    public enum StorageInterface
    {
        Unknown = 0, ACSI     = 1, ATA          = 2,
        XTA     = 3, ESDI     = 4, SCSI         = 5,
        USB     = 6, FireWire = 7, SASI         = 8,
        ST506   = 9, IPI      = 10, SMD         = 11,
        SATA    = 12, SSA     = 13, DSSI        = 14,
        HIPPI   = 15, SAS     = 16, FC          = 17,
        PCIe    = 18, M2      = 19, SataExpress = 20
    }

    public enum ColorSpace : ushort
    {
        [Display(Name = "sRGB")]
        Srgb = 1, [Display(Name = "Adobe RGB")]
        AdobeRgb = 2, [Display(Name = "Wide Gamut RGB")]
        WideGamutRgb = 4093, [Display(Name = "ICC Profile")]
        IccProfile = 65534, [Display(Name = "Uncalibrated")]
        Uncalibrated = 65535
    }

    public enum Contrast : ushort
    {
        Normal = 0, Low = 1, High = 2
    }

    public enum ExposureMode : ushort
    {
        Auto = 0, Manual = 1, [Display(Name = "Auto bracket")]
        AutoBracket = 2
    }

    public enum ExposureProgram : ushort
    {
        [Display(Name = "Not Defined")]
        Undefined = 0, [Display(Name = "Manual")]
        Manual = 1, [Display(Name = "Program AE")]
        ProgramAe = 2, [Display(Name = "Aperture-priority AE")]
        ApAe = 3, [Display(Name = "Shutter speed priority AE")]
        ShutterAe = 4, [Display(Name = "Creative (Slow speed)")]
        Creative = 5, [Display(Name = "Action (High speed)")]
        Action = 6, [Display(Name = "Portrait")]
        Portrait = 7, [Display(Name = "Landscape")]
        Landscape = 8, [Display(Name = "Bulb")]
        Bulb = 9
    }

    public enum Flash : ushort
    {
        [Display(Name = "No Flash")]
        None = 0, [Display(Name = "Fired")]
        Fired = 1, [Display(Name = "Fired, Return not detected")]
        FiredNoReturn = 5, [Display(Name = "Fired, Return detected")]
        FiredReturn = 7, [Display(Name = "On, Did not fire")]
        OnDidNotFire = 8, [Display(Name = "On, Fired")]
        OnFired = 9, [Display(Name = "On, Return not detected")]
        OnNoReturn = 13, [Display(Name = "On, Return detected")]
        OnReturn = 15, [Display(Name = "Off, Did not fire")]
        OffDidNotFire = 16, [Display(Name = "Off, Did not fire, Return not detected")]
        OffDidNotFireNoReturn = 20, [Display(Name = "Auto, Did not fire")]
        AutoDidNotFire = 24, [Display(Name = "Auto, Fired")]
        AutoFired = 25, [Display(Name = "Auto, Fired, Return not detected")]
        AutoFiredNoReturn = 29, [Display(Name = "Auto, Fired, Return detected")]
        AutoFiredReturn = 31, [Display(Name = "No flash function")]
        NoFlash = 32, [Display(Name = "Off, No flash function")]
        OffNoFlash = 48, [Display(Name = "Fired, Red-eye reduction")]
        FiredRedEye = 65, [Display(Name = "Fired, Red-eye reduction, Return not detected")]
        FiredRedEyeNoReturn = 69, [Display(Name = "Fired, Red-eye reduction, Return detected")]
        FiredRedEyeReturn = 71, [Display(Name = "On, Red-eye reduction")]
        OnRedEye = 73, [Display(Name = "On, Red-eye reduction, Return not detected")]
        OnRedEyeNoReturn = 77, [Display(Name = "On, Red-eye reduction, Return detected")]
        OnRedEyeReturn = 79, [Display(Name = "Off, Red-eye reduction")]
        OffRedEye = 80, [Display(Name = "Auto, Did not fire, Red-eye reduction")]
        AutoNotFireRedEye = 88, [Display(Name = "Auto, Fired, Red-eye reduction")]
        AutoFiredRedEye = 89, [Display(Name = "Auto, Fired, Red-eye reduction, Return not detected")]
        AutoFiredRedEyeNoReturn = 93, [Display(Name = "Auto, Fired, Red-eye reduction, Return detected")]
        AutoFiredRedEyeReturn = 95
    }

    public enum LightSource : ushort
    {
        [Display(Name = "Unknown")]
        Unknown = 0, [Display(Name = "Daylight")]
        Daylight = 1, [Display(Name = "Fluorescent")]
        Fluorescent = 2, [Display(Name = "Tungsten (Incandescent)")]
        Incandescent = 3, [Display(Name = "Flash")]
        Flash = 4, [Display(Name = "Fine Weather")]
        FineWeather = 9, [Display(Name = "Cloudy")]
        Cloudy = 10, [Display(Name = "Shade")]
        Shade = 11, [Display(Name = "Daylight Fluorescent")]
        DaylightFluorescent = 12, [Display(Name = "Day White Fluorescent")]
        DayWhiteFluorescent = 13, [Display(Name = "Cool White Fluorescent")]
        CoolWhiteFluorescent = 14, [Display(Name = "White Fluorescent")]
        WhiteFluorescent = 15, [Display(Name = "Warm White Fluorescent")]
        WarmWhiteFluorescent = 16, [Display(Name = "Standard Light A")]
        StandardLightA = 17, [Display(Name = "Standard Light B")]
        StandardLightB = 18, [Display(Name = "Standard Light C")]
        StandardLightC = 19, [Display(Name = "D55")]
        D55 = 20, [Display(Name = "D65")]
        D65 = 21, [Display(Name = "D75")]
        D75 = 22, [Display(Name = "D50")]
        D50 = 23, [Display(Name = "ISO Studio Tungsten")]
        ISOStudioTungsten = 24, [Display(Name = "Other")]
        Other = 255
    }

    public enum MeteringMode : ushort
    {
        [Display(Name = "Unknown")]
        Unknown = 0, [Display(Name = "Average")]
        Average = 1, [Display(Name = "Center-weighted average")]
        CenterWeightedAverage = 2, [Display(Name = "Spot")]
        Spot = 3, [Display(Name = "Multi-spot")]
        MultiSpot = 4, [Display(Name = "Multi-segment")]
        MultiSegment = 5, [Display(Name = "Partial")]
        Partial = 6, [Display(Name = "Other")]
        Other = 255
    }

    public enum Orientation : ushort
    {
        [Display(Name = "Horizontal (normal)")]
        Horizontal = 1, [Display(Name = "Mirror horizontal")]
        MirrorHorizontal = 2, [Display(Name = "Rotate 180")]
        Rotate180 = 3, [Display(Name = "Mirror vertical")]
        MirrorVertical = 4, [Display(Name = "Mirror horizontal and rotate 270 CW")]
        MirrorHorizontalAndRotate270CW = 5, [Display(Name = "Rotate 90 CW")]
        Rotate90CW = 6, [Display(Name = "Mirror horizontal and rotate 90 CW")]
        MirrorHorizontalAndRotate90CW = 7, [Display(Name = "Rotate 270 CW")]
        Rotate270CW = 8
    }

    public enum ResolutionUnit : ushort
    {
        None = 1, Inches = 2, Centimeters = 3
    }

    public enum Saturation : ushort
    {
        Normal = 0, Low = 1, High = 2
    }

    public enum SceneCaptureType : ushort
    {
        Standard = 0, Landscape = 1, Portrait = 2,
        Night    = 3
    }

    public enum SensingMethod : ushort
    {
        [Display(Name = "Not defined")]
        Undefined = 1, [Display(Name = "One-chip color area")]
        OneChipColorArea = 2, [Display(Name = "Two-chip color area")]
        TwoChipColorArea = 3, [Display(Name = "Three-chip color area")]
        ThreeChipColorArea = 4, [Display(Name = "Color sequential area")]
        ColorSequentialArea = 5, [Display(Name = "Trilinear")]
        Trilinear = 7, [Display(Name = "Color sequential linear")]
        ColorSequentialLinear = 8
    }

    public enum SubjectDistanceRange : ushort
    {
        Unknown = 0, Macro = 1, Close = 2,
        Distant = 3
    }

    public enum WhiteBalance : ushort
    {
        Auto = 0, Manual = 1
    }

    public enum Sharpness : ushort
    {
        Normal = 0, Low = 1, High = 2
    }

    public enum AuditType : byte
    {
        None    = 0, Created = 1, Updated = 2,
        Deleted = 3
    }
}