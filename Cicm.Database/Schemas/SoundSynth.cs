﻿/******************************************************************************
// Canary Islands Computer Museum Website
// ----------------------------------------------------------------------------
//
// Filename       : SoundSynth.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ Description ] ----------------------------------------------------------
//
//     High level representation of a sound synthetizer.
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

using System;

namespace Cicm.Database.Schemas
{
    /// <summary>Sound synthetizer</summary>
    public class SoundSynth
    {
        /// <summary>Company</summary>
        public Company Company;
        /// <summary>Sample rate in bits of the generate sound</summary>
        public int Depth;
        /// <summary>Frequency in Hz of the generated sound</summary>
        public double Frequency;
        /// <summary>ID</summary>
        public int Id;
        /// <summary>Datetime of introduction</summary>
        public DateTime Introduced;
        /// <summary>Model/SKU code</summary>
        public string ModelCode;
        /// <summary>Name</summary>
        public string Name;
        /// <summary>Simultaneous square wave generators</summary>
        public int SquareWave;
        /// <summary>Type of sound synthetizer</summary>
        public int Type;
        /// <summary>Simultaneous voices that can be generated</summary>
        public int Voices;
        /// <summary>Simultaneous white noise generators</summary>
        public int WhiteNoise;
    }
}