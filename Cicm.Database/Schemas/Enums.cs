﻿/******************************************************************************
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
}