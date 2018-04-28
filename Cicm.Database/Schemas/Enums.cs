/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     Enumerations.
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
// Copyright © 2003-2018 Natalia Portillo
*******************************************************************************/

namespace Cicm.Database.Schemas
{
    public enum NewsType
    {
        NewComputerInDb             = 1,
        NewConsoleInDb              = 2,
        NewComputerInCollection     = 3,
        NewConsoleInCollection      = 4,
        UpdatedComputerInDb         = 5,
        UpdatedConsoleInDb          = 6,
        UpdatedComputerInCollection = 7,
        UpdatedConsoleInCollection  = 8,
        NewMoneyDonation            = 9
    }

    public enum StatusType
    {
        TestedGood = 1,
        NotTested  = 2,
        TestedBad  = 3
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
        /// <summary>Memory used by the sound synthetizers</summary>
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
        ///     synthetizers
        /// </summary>
        Unified = 10
    }
}