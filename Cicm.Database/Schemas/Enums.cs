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
}